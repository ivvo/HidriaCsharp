#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Snap7Manager;
using XMLSettings;

namespace IvoSnap7Test
{
    // -------------------------------------------------------------------------
    //  Form1 – PLC Communication Client (multi-PLC)
    //
    //  Every <Connection Label="..."> in Settings.xml becomes its own tab with its own PlcView
    //  (own Snap7CommunicationManager = own thread + own S7 connection), so the app talks to
    //  several PLCs at the same time. Without any <Connection> segment the old single-PLC
    //  layout ("Read"/"Write" DBSections, default IP) is used.
    //
    //  Layout (top → bottom):
    //    [Toolbar]               Connect All | Disconnect All
    //    [Tabs]                  one PlcView per PLC
    // -------------------------------------------------------------------------
    public partial class Form1 : Form
    {
        private XMLSettingsManager _settings;
        private TabControl _tabs;
        private readonly List<PlcView> _views = new List<PlcView>();

        // ── Constructor ───────────────────────────────────────────────────────
        public Form1()
        {
            BuildUI();
            foreach (var config in LoadSettings())
                AddPlcTab(config);

            FormClosing += (s, e) => { foreach (var v in _views) v.Disconnect(); };
        }

        // =====================================================================
        //  UI Construction
        // =====================================================================
        private void BuildUI()
        {
            SuspendLayout();

            Text = "PLC Communication Client";
            MinimumSize = new Size(820, 660);
            Size = new Size(900, 760);
            Font = new Font("Segoe UI", 9f);
            BackColor = Color.FromArgb(240, 240, 245);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 38,
                Padding = new Padding(6, 5, 6, 3),
                BackColor = Color.FromArgb(35, 42, 56)
            };

            var btnConnectAll = new Button
            {
                Text = "Connect All",
                Size = new Size(100, 26),
                BackColor = Color.FromArgb(46, 160, 67),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnConnectAll.FlatAppearance.BorderSize = 0;
            btnConnectAll.Click += (s, e) => { foreach (var v in _views) v.Connect(); };
            toolbar.Controls.Add(btnConnectAll);

            var btnDisconnectAll = new Button
            {
                Text = "Disconnect All",
                Size = new Size(110, 26),
                BackColor = Color.FromArgb(200, 60, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDisconnectAll.FlatAppearance.BorderSize = 0;
            btnDisconnectAll.Click += (s, e) => { foreach (var v in _views) v.Disconnect(); };
            toolbar.Controls.Add(btnDisconnectAll);

            _tabs = new TabControl { Dock = DockStyle.Fill };

            Controls.Add(_tabs);
            Controls.Add(toolbar);

            ResumeLayout(true);
        }

        private void AddPlcTab(PlcConfig config)
        {
            var view = new PlcView(config);
            var page = new TabPage(TabTitle(config.Name, PLCConnectionStatus.Offline));
            view.StatusChanged += (s, status) => page.Text = TabTitle(config.Name, status);
            page.Controls.Add(view);
            _tabs.TabPages.Add(page);
            _views.Add(view);
        }

        private static string TabTitle(string name, PLCConnectionStatus status) => $"{name}  [{status}]";

        // =====================================================================
        //  Settings loading
        //
        //  <Connection Label="PLC1">
        //    <IPAddress Type="System.String">192.168.0.10</IPAddress>
        //    <Rack Type="System.Int32">0</Rack>
        //    <Slot Type="System.Int32">1</Slot>
        //    <ConnectionType Type="System.String">PG</ConnectionType>       (PG / OP / Basic, optional)
        //    <CycleTimeMs Type="System.Int32">10</CycleTimeMs>              (optional)
        //    <ReconnectDelayMs Type="System.Int32">1000</ReconnectDelayMs>  (optional)
        //    <WriteOnChangeOnly Type="System.Boolean">False</WriteOnChangeOnly> (optional)
        //  </Connection>
        //  <DBSection Label="PLC1.Read"> ... </DBSection>    (falls back to Label="Read")
        //  <DBSection Label="PLC1.Write"> ... </DBSection>   (falls back to Label="Write")
        // =====================================================================
        private List<PlcConfig> LoadSettings()
        {
            var configs = new List<PlcConfig>();

            try
            {
                _settings = new XMLSettingsManager(
                    AppDomain.CurrentDomain.BaseDirectory, "Settings.xml");

                var labels = _settings.GetSegmentLabels("Connection");
                if (labels.Count == 0) labels.Add("PLC"); // legacy single-PLC settings

                foreach (string label in labels)
                {
                    var c = new PlcConfig { Name = label };

                    // Connection parameters (optional keys; fall back to defaults)
                    TryGet(() => c.Ip = (string)_settings.GetElement("Connection", label, "IPAddress"));
                    TryGet(() => c.Rack = (int)_settings.GetElement("Connection", label, "Rack"));
                    TryGet(() => c.Slot = (int)_settings.GetElement("Connection", label, "Slot"));
                    TryGet(() => c.ConnectionType = (PLCConnectionType)Enum.Parse(typeof(PLCConnectionType),
                        (string)_settings.GetElement("Connection", label, "ConnectionType"), true));
                    TryGet(() => c.CycleTimeMs = (int)_settings.GetElement("Connection", label, "CycleTimeMs"));
                    TryGet(() => c.ReconnectDelayMs = (int)_settings.GetElement("Connection", label, "ReconnectDelayMs"));
                    TryGet(() => c.WriteOnChangeOnly = (bool)_settings.GetElement("Connection", label, "WriteOnChangeOnly"));

                    // DB sections: per-PLC first, then shared
                    c.ReadSection = TryLoadDBSection(label + ".Read") ?? TryLoadDBSection("Read");
                    c.WriteSection = TryLoadDBSection(label + ".Write") ?? TryLoadDBSection("Write");

                    configs.Add(c);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Settings.xml: {ex.Message}\n(using defaults)", "Settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (configs.Count == 0) configs.Add(new PlcConfig());
            return configs;
        }

        private DBSection TryLoadDBSection(string label)
        {
            try { return LoadDBSection(label); }
            catch { return null; /* section absent */ }
        }

        /// <summary>
        /// Loads a DBSection (reads DBNumber + list of DBData) from Settings.xml.
        /// </summary>
        private DBSection LoadDBSection(string label)
        {
            // DBNumber
            int dbNum = (int)_settings.GetElement("DBSection", label, "DBNumber");

            // DBData rows  → GetSegments("DBSection", label, "DBData")
            var rows = _settings.GetSegments("DBSection", label, "DBData");

            var section = new DBSection
            {
                DBNumber = dbNum,
                Data = new List<DBData>()
            };

            foreach (dynamic row in rows)
            {
                var dataType = Type.GetType((string)row.DataType)
                               ?? typeof(byte);

                section.Data.Add(new DBData
                {
                    Name = (string)row.Name,
                    DataType = dataType,
                    StartAddress = (int)row.StartAddress,
                    Length = (int)row.Length
                });
            }

            return section;
        }

        private static void TryGet(Action a)
        {
            try { a(); } catch { /* key absent — use default */ }
        }
    }
}
