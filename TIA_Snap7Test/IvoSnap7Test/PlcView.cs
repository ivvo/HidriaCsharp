#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Snap7Manager;

namespace IvoSnap7Test
{
    // -------------------------------------------------------------------------
    //  PlcConfig – settings of one PLC connection (one <Connection Label=".."> in Settings.xml)
    // -------------------------------------------------------------------------
    public class PlcConfig
    {
        public string Name = "PLC";
        public string Ip = "192.168.0.10";
        public int Rack = 0;
        public int Slot = 1;
        public PLCConnectionType ConnectionType = PLCConnectionType.PG;
        public int CycleTimeMs = 10;
        public int ReconnectDelayMs = 1000;
        public bool WriteOnChangeOnly = false;
        public DBSection ReadSection;
        public DBSection WriteSection;
    }

    // -------------------------------------------------------------------------
    //  PlcView – UI + communication for ONE PLC (Form1 hosts one per tab)
    //
    //  Layout (top → bottom):
    //    [Connection Bar]        IP | Rack | Slot | Type | Connect | Disconnect | Status
    //    [Inputs panel]          Read DB values received from PLC (ListView)
    //    [Outputs panel]         Write DB values to send to PLC
    //      • Bool outputs  (CheckBox per signal)
    //      • Double outputs (Label + TextBox per signal)
    //      • String outputs (Label + TextBox per signal)
    //    [Log panel]             Timestamped event log (TextBox)
    // -------------------------------------------------------------------------
    public class PlcView : UserControl
    {
        // ── Communication ─────────────────────────────────────────────────────
        private Snap7CommunicationManager _manager;
        private readonly PlcConfig _config;

        // ── Write-value state (mirrors what will be sent on the next write cycle)
        // Key = "Name:TypeName"  (matches the format used by HSnap7 / SetSections)
        private Dictionary<string, List<object>> _pendingWrites = new Dictionary<string, List<object>>();

        // ── UI controls ──────────────────────────────────────────────────────
        // Connection bar
        private TextBox _tbIp;
        private TextBox _tbRack;
        private TextBox _tbSlot;
        private ComboBox _cbConnType;
        private Button _btnConnect;
        private Button _btnDisconnect;
        private Label _lblStatus;

        // Inputs panel
        private ListView _lvInputs;

        // Outputs panel  (dynamic rows added in BuildOutputControls)
        private Panel _outputsPanel;
        private FlowLayoutPanel _boolOutputsFlow;
        private TableLayoutPanel _numStringOutputsTable;
        private Button _btnSendAll;

        // Log
        private TextBox _tbLog;

        /// <summary>Raised on the UI thread when the connection status of this PLC changes.</summary>
        public event EventHandler<PLCConnectionStatus> StatusChanged;

        public string PlcName => _config.Name;
        public bool IsRunning => _manager != null && _manager.IsManagerRunning;

        // ── Constructor ───────────────────────────────────────────────────────
        public PlcView(PlcConfig config)
        {
            _config = config;
            BuildUI();
            ApplyConfig();
        }

        // =====================================================================
        //  UI Construction
        // =====================================================================
        private void BuildUI()
        {
            SuspendLayout();

            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 9f);
            BackColor = Color.FromArgb(240, 240, 245);

            // ── Connection bar ────────────────────────────────────────────────
            var connPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(50, 60, 80),
                Padding = new Padding(8, 8, 8, 4)
            };

            var lblIp = MakeLabel("IP Address:", Color.White);
            lblIp.Location = new Point(8, 13);
            connPanel.Controls.Add(lblIp);

            _tbIp = new TextBox { Width = 130, Location = new Point(80, 10) };
            connPanel.Controls.Add(_tbIp);

            var lblRack = MakeLabel("Rack:", Color.White);
            lblRack.Location = new Point(220, 13);
            connPanel.Controls.Add(lblRack);

            _tbRack = new TextBox { Width = 36, Location = new Point(258, 10) };
            connPanel.Controls.Add(_tbRack);

            var lblSlot = MakeLabel("Slot:", Color.White);
            lblSlot.Location = new Point(304, 13);
            connPanel.Controls.Add(lblSlot);

            _tbSlot = new TextBox { Width = 36, Location = new Point(340, 10) };
            connPanel.Controls.Add(_tbSlot);

            var lblType = MakeLabel("Type:", Color.White);
            lblType.Location = new Point(386, 13);
            connPanel.Controls.Add(lblType);

            _cbConnType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 64,
                Location = new Point(424, 10)
            };
            _cbConnType.Items.AddRange(Enum.GetNames(typeof(PLCConnectionType)));
            connPanel.Controls.Add(_cbConnType);

            _btnConnect = new Button
            {
                Text = "Connect",
                Location = new Point(498, 8),
                Size = new Size(80, 26),
                BackColor = Color.FromArgb(46, 160, 67),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnConnect.FlatAppearance.BorderSize = 0;
            _btnConnect.Click += (s, e) => Connect();
            connPanel.Controls.Add(_btnConnect);

            _btnDisconnect = new Button
            {
                Text = "Disconnect",
                Location = new Point(586, 8),
                Size = new Size(90, 26),
                BackColor = Color.FromArgb(200, 60, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnDisconnect.FlatAppearance.BorderSize = 0;
            _btnDisconnect.Click += (s, e) => Disconnect();
            connPanel.Controls.Add(_btnDisconnect);

            _lblStatus = new Label
            {
                Text = "● Offline",
                ForeColor = Color.Silver,
                Location = new Point(688, 13),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            connPanel.Controls.Add(_lblStatus);

            Controls.Add(connPanel);

            // ── Log panel (bottom) ────────────────────────────────────────────
            var logGroup = new GroupBox
            {
                Text = "Event Log",
                Dock = DockStyle.Bottom,
                Height = 130,
                Padding = new Padding(6)
            };
            _tbLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 8.5f)
            };
            logGroup.Controls.Add(_tbLog);
            Controls.Add(logGroup);

            // ── Main splitter area ────────────────────────────────────────────
            var mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200,
                Panel1MinSize = 100,
                Panel2MinSize = 100
            };

            // -- Top half: Inputs (received from PLC) -------------------------
            var inputsGroup = new GroupBox
            {
                Text = "Inputs  (received from PLC)",
                Dock = DockStyle.Fill,
                Padding = new Padding(6)
            };
            _lvInputs = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.White
            };
            _lvInputs.Columns.Add("Signal", 220);
            _lvInputs.Columns.Add("Type", 90);
            _lvInputs.Columns.Add("Value", 160);
            _lvInputs.Columns.Add("Updated", 160);
            inputsGroup.Controls.Add(_lvInputs);
            mainSplit.Panel1.Controls.Add(inputsGroup);

            // -- Bottom half: Outputs (send to PLC) ---------------------------
            var outputsGroup = new GroupBox
            {
                Text = "Outputs  (send to PLC)",
                Dock = DockStyle.Fill,
                Padding = new Padding(6)
            };
            _outputsPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            _boolOutputsFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 2, 0, 4)
            };
            _outputsPanel.Controls.Add(_boolOutputsFlow);

            _numStringOutputsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 4,
                Padding = new Padding(0, 4, 0, 4)
            };
            _numStringOutputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            _numStringOutputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _numStringOutputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _numStringOutputsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            _numStringOutputsTable.Controls.Add(MakeBold("Signal"), 0, 0);
            _numStringOutputsTable.Controls.Add(MakeBold("Type"), 1, 0);
            _numStringOutputsTable.Controls.Add(MakeBold("Value"), 2, 0);
            _numStringOutputsTable.Controls.Add(MakeBold(""), 3, 0);
            _outputsPanel.Controls.Add(_numStringOutputsTable);

            _btnSendAll = new Button
            {
                Text = "Send All Outputs",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnSendAll.FlatAppearance.BorderSize = 0;
            _btnSendAll.Click += BtnSendAll_Click;

            outputsGroup.Controls.Add(_outputsPanel);
            outputsGroup.Controls.Add(_btnSendAll);
            mainSplit.Panel2.Controls.Add(outputsGroup);

            Controls.Add(mainSplit);

            ResumeLayout(true);
        }

        // =====================================================================
        //  Config → UI
        // =====================================================================
        private void ApplyConfig()
        {
            _tbIp.Text = _config.Ip;
            _tbRack.Text = _config.Rack.ToString();
            _tbSlot.Text = _config.Slot.ToString();
            _cbConnType.SelectedItem = _config.ConnectionType.ToString();

            // ── Populate Inputs list with signal names ─────────────────────
            if (_config.ReadSection?.Data != null)
            {
                foreach (var d in _config.ReadSection.Data)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add(d.DataType.Name);
                    item.SubItems.Add("—");
                    item.SubItems.Add("—");
                    _lvInputs.Items.Add(item);
                }
            }

            // ── Populate Output controls ───────────────────────────────────
            if (_config.WriteSection?.Data != null)
                BuildOutputControls(_config.WriteSection.Data);

            Log($"[{_config.Name}] Read DB{_config.ReadSection?.DBNumber.ToString() ?? "-"}, " +
                $"Write DB{_config.WriteSection?.DBNumber.ToString() ?? "-"}, " +
                $"cycle {_config.CycleTimeMs} ms, write on change only: {_config.WriteOnChangeOnly}");
        }

        /// <summary>
        /// Builds output UI rows for every DBData entry in the write section.
        /// Bool  → CheckBox
        /// Byte/Int16/Int32/Single/Double → TextBox (numeric)
        /// String → TextBox (text)
        /// </summary>
        private void BuildOutputControls(List<DBData> data)
        {
            int row = 1; // row 0 = header

            foreach (var d in data)
            {
                string key = $"{d.Name}:{d.DataType.Name}";

                if (d.DataType == typeof(bool))
                {
                    // Bool → CheckBox in the flow panel
                    var cb = new CheckBox
                    {
                        Text = d.Name,
                        Tag = key,
                        Margin = new Padding(4, 4, 12, 4),
                        AutoSize = true
                    };
                    cb.CheckedChanged += (s, _) =>
                    {
                        var c = (CheckBox)s;
                        _pendingWrites[c.Tag.ToString()] = new List<object> { (byte)(c.Checked ? 1 : 0) };
                    };
                    _pendingWrites[key] = new List<object> { (byte)0 };
                    _boolOutputsFlow.Controls.Add(cb);
                }
                else
                {
                    // Numeric or String → row in the table
                    _numStringOutputsTable.RowCount = row + 1;
                    _numStringOutputsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                    var lblName = new Label
                    {
                        Text = d.Name,
                        AutoSize = true,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Padding = new Padding(0, 4, 0, 0)
                    };

                    var lblType = new Label
                    {
                        Text = d.DataType.Name,
                        AutoSize = true,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top,
                        Padding = new Padding(0, 4, 0, 0),
                        ForeColor = Color.DimGray
                    };

                    var tb = new TextBox
                    {
                        Tag = key,
                        Text = GetDefaultText(d.DataType),
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2)
                    };

                    var btnSend = new Button
                    {
                        Text = "Send",
                        Tag = new object[] { key, d, tb },
                        Size = new Size(70, 24),
                        BackColor = Color.FromArgb(0, 120, 212),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnSend.FlatAppearance.BorderSize = 0;
                    btnSend.Click += BtnSendSingle_Click;

                    // Seed pending-writes with default value
                    _pendingWrites[key] = BuildValueList(d.DataType, d.Length, tb.Text);

                    _numStringOutputsTable.Controls.Add(lblName, 0, row);
                    _numStringOutputsTable.Controls.Add(lblType, 1, row);
                    _numStringOutputsTable.Controls.Add(tb, 2, row);
                    _numStringOutputsTable.Controls.Add(btnSend, 3, row);
                    row++;
                }
            }
        }

        // =====================================================================
        //  Connect / Disconnect
        // =====================================================================
        public void Connect()
        {
            if (IsRunning) return;

            try
            {
                // Manager is (re)created on every connect, so edited IP/Rack/Slot/Type take effect
                string ip = _tbIp.Text.Trim();
                if (!int.TryParse(_tbRack.Text, out int rack)) rack = 0;
                if (!int.TryParse(_tbSlot.Text, out int slot)) slot = 1;
                if (!Enum.TryParse(_cbConnType.SelectedItem?.ToString(), out PLCConnectionType connType))
                    connType = PLCConnectionType.PG;

                _manager = new Snap7CommunicationManager(ip, rack, slot, connType)
                {
                    CycleTimeMs = _config.CycleTimeMs,
                    ReconnectDelayMs = _config.ReconnectDelayMs,
                    WriteOnChangeOnly = _config.WriteOnChangeOnly
                };
                _manager.ConnectionStatusChanged += Manager_ConnectionStatusChanged;

                if (_config.ReadSection != null) _manager.AddReadSection(_config.ReadSection, ReadCallback);
                if (_config.WriteSection != null) _manager.AddWriteSection(_config.WriteSection);

                // Push current UI values into the manager before the first write cycle
                CollectAllOutputValues();
                FlushPendingWrites();

                _manager.Start();
                _btnConnect.Enabled = false;
                _btnDisconnect.Enabled = true;
                Log($"Connecting to {ip} (rack {rack}, slot {slot}, {connType}) ...");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"[{_config.Name}] Configuration error:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Disconnect()
        {
            if (IsRunning)
                _manager.Stop();

            _btnConnect.Enabled = true;
            _btnDisconnect.Enabled = false;
        }

        // =====================================================================
        //  Button handlers
        // =====================================================================
        private void BtnSendAll_Click(object sender, EventArgs e)
        {
            // Collect every output row and validate, then push to manager
            CollectAllOutputValues();
            FlushPendingWrites();
            Log("All outputs sent.");
        }

        private void BtnSendSingle_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var args = (object[])btn.Tag;
            string key = (string)args[0];
            DBData d = (DBData)args[1];
            var tb = (TextBox)args[2];

            try
            {
                var values = BuildValueList(d.DataType, d.Length, tb.Text);
                _pendingWrites[key] = values;

                if (IsRunning)
                    _manager.WriteValue(key, values);

                Log($"Sent [{d.Name}] = {tb.Text}");
            }
            catch (Exception ex)
            {
                Log($"[Send Error] {d.Name}: {ex.Message}");
                MessageBox.Show(ex.Message, "Send Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // =====================================================================
        //  Event: connection status changed
        // =====================================================================
        private void Manager_ConnectionStatusChanged(object sender, Snap7onnectionStatusChangedEventArgs e)
        {
            UIInvoke(() => HandleStatusChange(e.ConnectionStatus));
        }

        private void HandleStatusChange(PLCConnectionStatus status)
        {
            switch (status)
            {
                case PLCConnectionStatus.Online:
                    _lblStatus.Text = "● Online";
                    _lblStatus.ForeColor = Color.LimeGreen;
                    break;
                case PLCConnectionStatus.Error:
                    _lblStatus.Text = "● Error";
                    _lblStatus.ForeColor = Color.OrangeRed;
                    break;
                default:
                    _lblStatus.Text = IsRunning ? "● Offline (retrying)" : "● Offline";
                    _lblStatus.ForeColor = Color.Silver;
                    break;
            }

            Log($"Connection status → {status}");
            StatusChanged?.Invoke(this, status);
        }

        // =====================================================================
        //  Event: data received from PLC (Read callback)
        // =====================================================================
        private void ReadCallback(string signalKey, List<object> values)
        {
            UIInvoke(() => UpdateInputDisplay(signalKey, values));
        }

        private void UpdateInputDisplay(string signalKey, List<object> values)
        {
            // signalKey format: "SignalName:TypeName"
            string name = signalKey.Contains(':')
                ? signalKey.Split(':')[0]
                : signalKey;
            string typeName = signalKey.Contains(':')
                ? signalKey.Split(':')[1]
                : "";

            string displayValue = FormatValues(values, typeName);

            // Find or create the row
            ListViewItem item = _lvInputs.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Text == name);
            if (item == null)
            {
                item = new ListViewItem(name);
                item.SubItems.Add(typeName);
                item.SubItems.Add(displayValue);
                item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss.fff"));
                _lvInputs.Items.Add(item);
            }
            else
            {
                item.SubItems[1].Text = typeName;
                item.SubItems[2].Text = displayValue;
                item.SubItems[3].Text = DateTime.Now.ToString("HH:mm:ss.fff");
                item.BackColor = Color.FromArgb(230, 255, 230); // flash green
            }

            Log($"Received [{name}] = {displayValue}");
        }

        // =====================================================================
        //  Helpers – value conversion
        // =====================================================================

        /// <summary>
        /// Builds a List&lt;object&gt; from the text-box string for the given type and length.
        /// Strings are encoded as ASCII bytes padded/trimmed to <paramref name="length"/>.
        /// </summary>
        private List<object> BuildValueList(Type type, int length, string text)
        {
            var list = new List<object>();

            if (type == typeof(string) || type == typeof(String))
            {
                // Encode as ASCII bytes, pad or truncate to length
                byte[] bytes = Encoding.ASCII.GetBytes(text.PadRight(length, ' '));
                if (bytes.Length > length) bytes = bytes.Take(length).ToArray();
                foreach (var b in bytes) list.Add(b);
                return list;
            }

            // For numeric types the text is a single value; length is the element count
            for (int i = 0; i < length; i++)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Byte:
                        list.Add(byte.TryParse(text, out byte bv) ? bv : (byte)0);
                        break;
                    case TypeCode.Int16:
                        list.Add(short.TryParse(text, out short sv) ? sv : (short)0);
                        break;
                    case TypeCode.Int32:
                        list.Add(int.TryParse(text, out int iv) ? iv : 0);
                        break;
                    case TypeCode.Single:
                        list.Add(float.TryParse(text,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out float fv) ? fv : 0f);
                        break;
                    case TypeCode.Double:
                        // S7 doubles are represented as Single on wire; cast down
                        list.Add(double.TryParse(text,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double dv) ? (float)dv : 0f);
                        break;
                    default:
                        list.Add((byte)0);
                        break;
                }
            }
            return list;
        }

        /// <summary>
        /// Formats a values list for display. Byte arrays whose contents are
        /// printable ASCII are shown as a string.
        /// </summary>
        private string FormatValues(List<object> values, string typeName)
        {
            if ((typeName == "Byte" || typeName == "String") && values.Count > 1)
            {
                // Try to display as ASCII string
                var bytes = values.Select(v => Convert.ToByte(v)).ToArray();
                bool printable = bytes.All(b => b >= 32 && b < 127);
                if (printable)
                    return $"\"{Encoding.ASCII.GetString(bytes).TrimEnd()}\"";
            }
            return string.Join(", ", values);
        }

        private string GetDefaultText(Type type) =>
            (type == typeof(string) || type == typeof(String)) ? "" : "0";

        // =====================================================================
        //  Helpers – write flush
        // =====================================================================
        private void CollectAllOutputValues()
        {
            // Collect TextBox values from the numeric/string table
            foreach (Control c in _numStringOutputsTable.Controls)
            {
                if (c is TextBox tb && tb.Tag is string key)
                {
                    // Find the corresponding DBData to know type/length
                    var dbData = _config.WriteSection?.Data
                        .Where(d => $"{d.Name}:{d.DataType.Name}" == key)
                        .Cast<DBData?>()
                        .FirstOrDefault();
                    if (dbData.HasValue)
                        _pendingWrites[key] = BuildValueList(dbData.Value.DataType, dbData.Value.Length, tb.Text);
                }
            }
            // CheckBox values are already updated via CheckedChanged
        }

        private void FlushPendingWrites()
        {
            if (_manager == null) return;
            if (_config.WriteSection?.Data == null) return;

            // Build write list ordered exactly as DBSection.Data so every key HSnap7
            // expects is present - mirrors the working WriteValues call pattern
            var writes = new List<(string, List<object>)>();
            foreach (var d in _config.WriteSection.Data)
            {
                string key = $"{d.Name}:{d.DataType.Name}";
                if (_pendingWrites.TryGetValue(key, out var values))
                    writes.Add((key, values));
                else
                    writes.Add((key, BuildValueList(d.DataType, d.Length, "0")));
            }

            try { _manager.WriteValues(writes); }
            catch (Exception ex) { Log($"[WriteValues Error] {ex.Message}"); }
        }

        // =====================================================================
        //  Helpers – UI utilities
        // =====================================================================

        /// <summary>
        /// Marshals to the UI thread asynchronously. Must NOT be synchronous (Invoke): Disconnect() waits
        /// for the communication thread on the UI thread, a synchronous Invoke from that thread would deadlock.
        /// </summary>
        private void UIInvoke(Action action)
        {
            if (!InvokeRequired) { action(); return; }
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(action); }
            catch (InvalidOperationException) { /* control is being destroyed */ }
        }

        private void Log(string msg)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}]  {msg}";
            UIInvoke(() => AppendLog(line));
        }

        private void AppendLog(string line)
        {
            _tbLog.AppendText(line + Environment.NewLine);
            _tbLog.ScrollToCaret();
        }

        private static Label MakeLabel(string text, Color color = default)
        {
            var lbl = new Label { Text = text, AutoSize = true, ForeColor = color == default ? Color.Black : color };
            return lbl;
        }

        private static Label MakeBold(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 80),
                Margin = new Padding(2, 4, 2, 2)
            };
        }
    }
}
