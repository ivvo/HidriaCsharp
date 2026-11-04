using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TIA_Generator.Extract;
using TIA_Generator.Generate;
using TIA_Generator.Model;

namespace TIA_Generator.Gui
{
    /// <summary>
    /// Glavno okno: export → model stroja (faza 1), pregled enot, urejanje besedil SL/EN/DE in nastavitev, opozorila, spremembe PLC.
    /// Brez designerja – vsi gradniki so ustvarjeni v kodi.
    /// </summary>
    sealed class MainForm : Form
    {
        // ---------------------------------------------------------------- stanje
        MachineModelFile _model;
        string _modelPath;
        bool _dirty;
        bool _loadingUnit;
        Unit _unit;
        List<UnitRow> _allUnitRows = new List<UnitRow>();

        // ---------------------------------------------------------------- gradniki
        readonly ToolStripButton _btnExport, _btnOpen, _btnSave, _btnSaveAs, _btnPlc, _btnNew, _btnStructure, _btnSkeleton;
        TextDictionary _dict;
        TextDictionary Dict => _dict ?? (_dict = TextDictionary.LoadDefault(new[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slovar_lokalni.json") }));
        readonly ToolStripLabel _lblMachine;
        readonly ToolStripStatusLabel _status;
        readonly ToolStripProgressBar _progress;
        readonly TextBox _filter;
        readonly DataGridView _gridUnits;
        readonly TabControl _unitTabs;
        readonly TextBox _txtSl, _txtEn, _txtDe, _txtInfo;
        readonly NumericUpDown _numPlc;
        readonly Label _lblHmi;
        readonly CheckBox _chkPc, _chkPanel;
        readonly DataGridView _gridTimers, _gridParams, _gridRecipe, _gridCyl, _gridAlarms;
        readonly DataGridView _gridWarnings, _gridGlobalAlarms;
        readonly TextBox _txtSummary, _txtLog;

        public MainForm()
        {
            Text = "TIA_Generator – generator HMI (Standard 2.0)";
            Font = new Font("Segoe UI", 9f);
            Size = new Size(1500, 950);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            // --- orodna vrstica
            var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(6, 2, 6, 2) };
            _btnExport = new ToolStripButton("Izdelaj model iz exporta ...") { ToolTipText = "Izberi mapo 'Export all' (z manifest.json) – Ctrl+E" };
            _btnOpen = new ToolStripButton("Odpri model ...") { ToolTipText = "Odpri *.hmimodel.json – Ctrl+O" };
            _btnSave = new ToolStripButton("Shrani") { ToolTipText = "Ctrl+S", Enabled = false };
            _btnSaveAs = new ToolStripButton("Shrani kot ...") { Enabled = false };
            _btnPlc = new ToolStripButton("Spremembe PLC (CSV) ...") { ToolTipText = "Časovniki → Delay[0..9]: seznam za Excel", Enabled = false };
            _btnNew = new ToolStripButton("Nov stroj ...") { ToolTipText = "Nov stroj: sektorji, postaje, podpostaje – Ctrl+N" };
            _btnStructure = new ToolStripButton("Struktura ...") { ToolTipText = "Sektorji, število postaj, podpostaje, PLC indeksi", Enabled = false };
            _btnSkeleton = new ToolStripButton("PLC ogrodje ...") { ToolTipText = "FB/DB novih postaj kot klon vzorčne enote (SimaticML za uvoz)", Enabled = false };
            _lblMachine = new ToolStripLabel("") { Alignment = ToolStripItemAlignment.Right, Font = new Font(Font, FontStyle.Bold) };
            tools.Items.AddRange(new ToolStripItem[] { _btnNew, _btnExport, _btnOpen, new ToolStripSeparator(), _btnSave, _btnSaveAs, new ToolStripSeparator(),
                                                       _btnStructure, _btnSkeleton, _btnPlc, _lblMachine });
            _btnNew.Click += (s, e) => NewMachine();
            _btnStructure.Click += (s, e) => EditStructure();
            _btnSkeleton.Click += (s, e) => GenerateSkeleton();
            _btnExport.Click += async (s, e) => await ExtractFromExport();
            _btnOpen.Click += (s, e) => OpenModel();
            _btnSave.Click += (s, e) => Save(false);
            _btnSaveAs.Click += (s, e) => Save(true);
            _btnPlc.Click += (s, e) => ExportPlcChanges();

            var statusStrip = new StatusStrip();
            _status = new ToolStripStatusLabel("Izberi export ali odpri model.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _progress = new ToolStripProgressBar { Style = ProgressBarStyle.Marquee, Visible = false, Width = 160 };
            statusStrip.Items.AddRange(new ToolStripItem[] { _status, _progress });

            // --- levo: enote
            _filter = new TextBox { Dock = DockStyle.Top };
            SetCue(_filter, "Filter (oznaka ali opis) ...");
            _filter.TextChanged += (s, e) => ApplyFilter();
            _gridUnits = Grid(readOnly: true);
            Col(_gridUnits, "Enota", "Id", 90, true);
            Col(_gridUnits, "Del", "Part", 45, true);
            Col(_gridUnits, "PLC", "Plc", 40, true);
            Col(_gridUnits, "HMI", "Hmi", 40, true);
            Col(_gridUnits, "Settings", "Settings", 62, true);
            Col(_gridUnits, "Čas.", "Timers", 45, true);
            Col(_gridUnits, "Param", "Params", 48, true);
            Col(_gridUnits, "Rec.", "Recipe", 40, true);
            Col(_gridUnits, "Cil.", "Cylinders", 36, true);
            Col(_gridUnits, "Alarm", "Alarms", 46, true);
            Col(_gridUnits, "Opoz.", "Warnings", 44, true);
            Col(_gridUnits, "Opis", "Text", 160, true).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            // SelectionChanged se lahko sproži, preden je CurrentRow posodobljen → beremo izbrano vrstico
            _gridUnits.SelectionChanged += (s, e) =>
            {
                var row = _gridUnits.SelectedRows.Count > 0 ? _gridUnits.SelectedRows[0] : _gridUnits.CurrentRow;
                var u = (row?.DataBoundItem as UnitRow)?.U;
                if (u != _unit) ShowUnit(u);
            };
            _gridUnits.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && _gridUnits.Columns[e.ColumnIndex].DataPropertyName == "Warnings" && e.Value is int n && n > 0)
                    e.CellStyle.ForeColor = Color.DarkOrange;
            };
            var left = new Panel { Dock = DockStyle.Fill };
            left.Controls.Add(_gridUnits);
            left.Controls.Add(_filter);

            // --- desno: zavihki enote
            _unitTabs = new TabControl { Dock = DockStyle.Fill };

            // Enota
            var pUnit = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10), AutoScroll = true };
            pUnit.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            pUnit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _txtSl = new TextBox { Dock = DockStyle.Fill };
            _txtEn = new TextBox { Dock = DockStyle.Fill };
            _txtDe = new TextBox { Dock = DockStyle.Fill };
            _numPlc = new NumericUpDown { Minimum = 0, Maximum = 99, Width = 70 };
            _lblHmi = new Label { AutoSize = true, Padding = new Padding(8, 6, 0, 0) };
            _chkPc = new CheckBox { Text = "PC (WinCC RT Advanced)", AutoSize = true };
            _chkPanel = new CheckBox { Text = "Panel (Comfort)", AutoSize = true };
            _txtInfo = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = SystemColors.Window, Height = 260 };
            var idxPanel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            idxPanel.Controls.Add(_numPlc); idxPanel.Controls.Add(_lblHmi);
            var showPanel = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            showPanel.Controls.Add(_chkPc); showPanel.Controls.Add(_chkPanel);
            AddRow(pUnit, "Opis (sl-SI)", _txtSl);
            AddRow(pUnit, "Opis (en-US)", _txtEn);
            AddRow(pUnit, "Opis (de-DE)", _txtDe);
            AddRow(pUnit, "PLC indeks postaje", idxPanel);
            AddRow(pUnit, "Prikaži na", showPanel);
            AddRow(pUnit, "Podatki iz PLC", _txtInfo);
            _txtSl.TextChanged += (s, e) => UnitText("sl-SI", _txtSl.Text);
            _txtEn.TextChanged += (s, e) => UnitText("en-US", _txtEn.Text);
            _txtDe.TextChanged += (s, e) => UnitText("de-DE", _txtDe.Text);
            _numPlc.ValueChanged += (s, e) => UnitIndexChanged();
            _chkPc.CheckedChanged += (s, e) => UnitShowOn("PC", _chkPc.Checked);
            _chkPanel.CheckedChanged += (s, e) => UnitShowOn("Panel", _chkPanel.Checked);
            AddTab(_unitTabs, "Enota", pUnit);

            // Časovniki
            _gridTimers = Grid();
            Col(_gridTimers, "#", "Index", 30, true);
            Col(_gridTimers, "Član v PLC", "Member", 170, true);
            Col(_gridTimers, "Element", "Element", 70, true);
            Check(_gridTimers, "Prikaži", "Show", 55);
            TextCols(_gridTimers);
            Col(_gridTimers, "Izvor", "Source", 80, true);
            Col(_gridTimers, "Uporabljen v (PLC)", "UsedIn", 300, true);
            AddTab(_unitTabs, "Časovniki", _gridTimers);

            // Parametri
            _gridParams = Grid();
            Col(_gridParams, "Član v PLC", "Member", 220, true);
            Col(_gridParams, "Tip", "DataType", 60, true);
            Combo(_gridParams, "Polje", "Access", 75, "input", "display");
            Col(_gridParams, "Enota", "Unit", 50, false);
            TextCols(_gridParams);
            Col(_gridParams, "Izvor", "Source", 80, true);
            AddTab(_unitTabs, "Parametri", _gridParams);

            // Recept
            _gridRecipe = Grid();
            Col(_gridRecipe, "Član Recipe", "Member", 220, true);
            Col(_gridRecipe, "Tip", "DataType", 60, true);
            Col(_gridRecipe, "Enota", "Unit", 50, false);
            TextCols(_gridRecipe);
            Col(_gridRecipe, "Izvor", "Source", 80, true);
            AddTab(_unitTabs, "Recept", _gridRecipe);

            // Cilindri
            _gridCyl = Grid();
            Col(_gridCyl, "Cilinder", "Name", 130, true);
            Col(_gridCyl, "Ukaz naprej", "FwdCmd", 190, true);
            Col(_gridCyl, "Ukaz nazaj", "BwdCmd", 190, true);
            Col(_gridCyl, "Indeks F", "IndexF", 70, true);
            Col(_gridCyl, "Indeks B", "IndexB", 70, true);
            TextCols(_gridCyl);
            Col(_gridCyl, "Izvor", "Source", 80, true);
            AddTab(_unitTabs, "Cilindri", _gridCyl);

            // Alarmi enote
            _gridAlarms = AlarmGrid();
            AddTab(_unitTabs, "Alarmi", _gridAlarms);

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 640, FixedPanel = FixedPanel.Panel1 };
            split.Panel1.Controls.Add(left);
            split.Panel2.Controls.Add(_unitTabs);

            // --- spodaj: opozorila, povzetek, alarmi brez enote, dnevnik
            var bottom = new TabControl { Dock = DockStyle.Fill };
            _gridWarnings = Grid(readOnly: true);
            Col(_gridWarnings, "Enota", "Unit", 120, true);
            Col(_gridWarnings, "Koda", "Code", 140, true);
            Col(_gridWarnings, "Sporočilo", "Message", 400, true).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            _gridWarnings.CellDoubleClick += (s, e) => SelectUnitFromWarning();
            AddTab(bottom, "Opozorila", _gridWarnings);
            _txtSummary = LogBox();
            AddTab(bottom, "Povzetek", _txtSummary);
            _gridGlobalAlarms = AlarmGrid();
            AddTab(bottom, "Alarmi brez enote", _gridGlobalAlarms);
            _txtLog = LogBox();
            AddTab(bottom, "Dnevnik", _txtLog);

            var main = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 600 };
            main.Panel1.Controls.Add(split);
            main.Panel2.Controls.Add(bottom);

            Controls.Add(main);
            Controls.Add(tools);
            Controls.Add(statusStrip);

            Load += (s, e) => { split.SplitterDistance = 640; main.SplitterDistance = (int)(ClientSize.Height * 0.68); };
            FormClosing += (s, e) => { if (!ConfirmDiscard()) e.Cancel = true; };
            KeyDown += async (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.S && _model != null) { Save(false); e.Handled = true; }
                else if (e.Control && e.KeyCode == Keys.O) { OpenModel(); e.Handled = true; }
                else if (e.Control && e.KeyCode == Keys.N) { NewMachine(); e.Handled = true; }
                else if (e.Control && e.KeyCode == Keys.E) { e.Handled = true; await ExtractFromExport(); }
            };
            ShowUnit(null);
        }

        // ================================================================ gradniki – pomožno

        static DataGridView Grid(bool readOnly = false)
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                ReadOnly = readOnly, BackgroundColor = SystemColors.Window, BorderStyle = BorderStyle.None,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2, AllowUserToResizeRows = false,
            };
            g.RowTemplate.Height = 22;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(29, 83, 131);   // Hidria modra (standard 2.1)
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.DataError += (s, e) => e.ThrowException = false;
            return g;
        }

        static DataGridViewTextBoxColumn Col(DataGridView g, string header, string prop, int width, bool readOnly)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = prop, Width = width, ReadOnly = readOnly, SortMode = DataGridViewColumnSortMode.NotSortable };
            if (readOnly) c.DefaultCellStyle.ForeColor = Color.DimGray;
            g.Columns.Add(c);
            return c;
        }

        static void Check(DataGridView g, string header, string prop, int width) =>
            g.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = header, DataPropertyName = prop, Width = width });

        static void Combo(DataGridView g, string header, string prop, int width, params string[] items)
        {
            var c = new DataGridViewComboBoxColumn { HeaderText = header, DataPropertyName = prop, Width = width, FlatStyle = FlatStyle.Flat };
            c.Items.AddRange(items);
            g.Columns.Add(c);
        }

        /// <summary>Stolpci besedil SL/EN/DE – urejljivi (svetlo rumeno ozadje = vnos).</summary>
        static void TextCols(DataGridView g)
        {
            foreach (var (h, p) in new[] { ("Besedilo SL", "Sl"), ("EN", "En"), ("DE", "De") })
            {
                var c = Col(g, h, p, h.Length > 2 ? 230 : 180, false);
                c.DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 230);
            }
        }

        DataGridView AlarmGrid()
        {
            var g = Grid();
            Col(g, "Bit (DB_Error_Messages)", "Bit", 260, true);
            Combo(g, "Razred", "Class", 85, "Errors", "Warnings");
            Col(g, "Odmik", "Offset", 110, true);
            TextCols(g);
            Col(g, "Izvor", "Source", 80, true);
            return g;
        }

        static TextBox LogBox() => new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, BackColor = SystemColors.Window, Font = new Font("Consolas", 9f) };

        static void AddTab(TabControl tc, string text, Control content)
        {
            var p = new TabPage(text);
            p.Controls.Add(content);
            tc.TabPages.Add(p);
        }

        static void AddRow(TableLayoutPanel t, string label, Control c)
        {
            t.RowCount++;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, t.RowCount - 1);
            t.Controls.Add(c, 1, t.RowCount - 1);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, string lp);
        static void SetCue(TextBox tb, string cue) => tb.HandleCreated += (s, e) => SendMessage(tb.Handle, 0x1501, (IntPtr)1, cue);

        void Log(string s)
        {
            if (InvokeRequired) { BeginInvoke((Action)(() => Log(s))); return; }
            _txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + s + Environment.NewLine);
        }

        void Busy(bool on, string text)
        {
            _progress.Visible = on;
            _status.Text = text;
            _btnExport.Enabled = _btnOpen.Enabled = _btnNew.Enabled = !on;
            _btnSave.Enabled = _btnSaveAs.Enabled = _btnPlc.Enabled = _btnStructure.Enabled = _btnSkeleton.Enabled = !on && _model != null;
            UseWaitCursor = on;
        }

        // ================================================================ model: izdelava, odpiranje, shranjevanje

        public async Task ExtractFromExport(string preset = null)
        {
            if (!ConfirmDiscard()) return;
            string dir = preset;
            if (dir == null)
            using (var d = new FolderBrowserDialog { Description = "Izberi mapo exporta 'Export all' (vsebuje manifest.json in PLC\\)", ShowNewFolderButton = false })
            {
                var last = Settings.Get("lastExport");
                if (last != null && Directory.Exists(last)) d.SelectedPath = last;
                else if (Directory.Exists(@"D:\Claude\TiaFullExport")) d.SelectedPath = @"D:\Claude\TiaFullExport";
                if (d.ShowDialog(this) != DialogResult.OK) return;
                dir = d.SelectedPath;
            }
            var projects = TIA_ToolKit.Analyzer.ExportReader.FindProjects(dir);
            if (projects.Count == 0) { MessageBox.Show(this, "V mapi ni exporta (manifest.json ali PLC\\).", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (projects.Count > 1)
            {
                dir = PickProject(projects);
                if (dir == null) return;
            }
            Settings.Set("lastExport", dir);

            Busy(true, "Berem export " + dir + " ...");
            try
            {
                var extra = new List<string>();
                string local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "slovar_lokalni.json");
                if (File.Exists(local)) { extra.Add(local); Log("Lokalni slovar: " + local); }
                var model = await Task.Run(() => new ModelExtractor(TextDictionary.LoadDefault(extra), Log).Extract(dir));
                string parent = Path.GetDirectoryName(model.SourceExport) ?? dir;
                SetModel(model, Path.Combine(parent, SafeName(model.Machine.Name) + ".hmimodel.json"), dirty: true);
                Log("Model izdelan: " + model.Units.Count + " enot, " + model.Warnings.Count + " opozoril. Shrani ga s Ctrl+S.");
                _status.Text = "Model izdelan – še ni shranjen (" + _modelPath + ").";
            }
            catch (Exception ex)
            {
                Log("NAPAKA: " + ex);
                MessageBox.Show(this, ex.Message, "Napaka pri branju exporta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _status.Text = "Napaka – glej Dnevnik.";
            }
            finally { Busy(false, _status.Text); }
        }

        string PickProject(List<string> projects)
        {
            using (var f = new Form { Text = "Več exportov v mapi – izberi enega", Size = new Size(700, 320), StartPosition = FormStartPosition.CenterParent, Font = Font, MinimizeBox = false, MaximizeBox = false })
            {
                var lb = new ListBox { Dock = DockStyle.Fill };
                lb.Items.AddRange(projects.Cast<object>().ToArray());
                lb.SelectedIndex = 0;
                var ok = new Button { Text = "Izberi", Dock = DockStyle.Bottom, DialogResult = DialogResult.OK, Height = 32 };
                lb.DoubleClick += (s, e) => { f.DialogResult = DialogResult.OK; f.Close(); };
                f.Controls.Add(lb); f.Controls.Add(ok); f.AcceptButton = ok;
                return f.ShowDialog(this) == DialogResult.OK ? (string)lb.SelectedItem : null;
            }
        }

        void OpenModel()
        {
            if (!ConfirmDiscard()) return;
            using (var d = new OpenFileDialog { Filter = "Model stroja (*.hmimodel.json)|*.hmimodel.json|JSON (*.json)|*.json", Title = "Odpri model stroja" })
            {
                var last = Settings.Get("lastModel");
                if (last != null && File.Exists(last)) d.InitialDirectory = Path.GetDirectoryName(last);
                if (d.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    SetModel(ModelJson.Load(d.FileName), d.FileName, dirty: false);
                    Settings.Set("lastModel", d.FileName);
                    Log("Odprt model " + d.FileName);
                    _status.Text = "Odprt: " + d.FileName;
                }
                catch (Exception ex) { MessageBox.Show(this, ex.Message, "Napaka pri odpiranju", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        /// <summary>Odpre model brez dialoga (ukazna vrstica, dvoklik na .hmimodel.json).</summary>
        public void OpenModelFile(string path)
        {
            try
            {
                SetModel(ModelJson.Load(path), Path.GetFullPath(path), dirty: false);
                Settings.Set("lastModel", Path.GetFullPath(path));
                _status.Text = "Odprt: " + path;
                Log("Odprt model " + path);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Napaka pri odpiranju", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        public void SelectUnit(string id, int tab)
        {
            foreach (DataGridViewRow r in _gridUnits.Rows)
                if (string.Equals((r.DataBoundItem as UnitRow)?.Id, id, StringComparison.OrdinalIgnoreCase)) { _gridUnits.CurrentCell = r.Cells[0]; break; }
            if (tab >= 0 && tab < _unitTabs.TabCount) _unitTabs.SelectedIndex = tab;
        }

        /// <summary>Zapre brez vprašanja o shranjevanju (samo za --snapshot).</summary>
        public void CloseWithoutPrompt() { _dirty = false; Close(); }

        /// <summary>Razvoj/dokumentacija: izriše okno v PNG.</summary>
        public void Snapshot(string png)
        {
            Application.DoEvents();
            using (var bmp = new Bitmap(Width, Height))
            {
                DrawToBitmap(bmp, new Rectangle(0, 0, Width, Height));
                bmp.Save(png, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        bool Save(bool askPath)
        {
            if (_model == null) return false;
            CommitEdits();
            string path = _modelPath;
            if (askPath || path == null)
                using (var d = new SaveFileDialog { Filter = "Model stroja (*.hmimodel.json)|*.hmimodel.json", FileName = Path.GetFileName(path ?? "model.hmimodel.json"), InitialDirectory = path == null ? null : Path.GetDirectoryName(path) })
                {
                    if (d.ShowDialog(this) != DialogResult.OK) return false;
                    path = d.FileName;
                }
            try
            {
                ModelExtractor.RecalcStatistics(_model);
                ModelJson.Save(_model, path);
                _modelPath = path;
                Settings.Set("lastModel", path);
                SetDirty(false);
                RefreshSummary();
                _status.Text = "Shranjeno: " + path;
                Log("Shranjeno " + path);
                return true;
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Napaka pri shranjevanju", MessageBoxButtons.OK, MessageBoxIcon.Error); return false; }
        }

        bool ConfirmDiscard()
        {
            if (!_dirty) return true;
            var r = MessageBox.Show(this, "Model ima neshranjene spremembe. Shranim?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            return r == DialogResult.No || (r == DialogResult.Yes && Save(false));
        }

        void ExportPlcChanges()
        {
            if (_model == null) return;
            CommitEdits();
            var units = PlcChanges.UnitsNeedingChange(_model).ToList();
            if (units.Count == 0) { MessageBox.Show(this, "Vsi časovniki so že polje – sprememb PLC ni.", Text); return; }
            string def = _modelPath != null ? Path.Combine(Path.GetDirectoryName(_modelPath), Path.GetFileName(_modelPath).Replace(".hmimodel.json", "") + "_PLC_spremembe.csv") : "PLC_spremembe.csv";
            using (var d = new SaveFileDialog { Filter = "CSV za Excel (*.csv)|*.csv", FileName = Path.GetFileName(def), InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(def)) })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                File.WriteAllLines(d.FileName, PlcChanges.CsvLines(_model), new UTF8Encoding(true));
                int n = units.Sum(u => u.Timers.Count);
                _status.Text = "Spremembe PLC: " + units.Count + " DB-jev, " + n + " članov → " + d.FileName;
                Log(_status.Text);
            }
        }

        static string SafeName(string s) { foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_'); return s; }

        // ================================================================ prikaz modela

        void SetModel(MachineModelFile m, string path, bool dirty)
        {
            _model = m;
            _modelPath = path;
            _allUnitRows = m.Units.Select(u => new UnitRow(u, m.Warnings)).ToList();
            ApplyFilter();
            _gridWarnings.DataSource = new BindingList<WarningRow>(m.Warnings.OrderBy(w => w.Code).ThenBy(w => w.Unit).Select(w => new WarningRow(w)).ToList());
            _gridGlobalAlarms.DataSource = new BindingList<AlarmRow>(m.Global.Alarms.Where(a => a.Unit == null).Select(a => new AlarmRow(a)).ToList());
            _lblMachine.Text = m.Machine.Name + "  ·  " + (m.Machine.Type == "parts" ? "deli " + string.Join(",", m.Machine.Parts) : "en stroj") +
                               "  ·  " + string.Join(", ", m.Machine.HmiDevices.Select(d => d.Kind + " " + d.Target));
            SetDirty(dirty);
            RefreshSummary();
            Busy(false, _status.Text);
            if (_gridUnits.Rows.Count > 0) { _gridUnits.CurrentCell = _gridUnits.Rows[0].Cells[0]; ShowUnit(_allUnitRows[0].U); }
        }

        void ApplyFilter()
        {
            string f = _filter.Text.Trim();
            var rows = f.Length == 0 ? _allUnitRows
                : _allUnitRows.Where(r => r.Id.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0 || r.Text.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            _gridUnits.DataSource = new BindingList<UnitRow>(rows);
        }

        void RefreshSummary()
        {
            if (_model == null) { _txtSummary.Text = ""; return; }
            var m = _model; var s = m.Statistics; var sb = new StringBuilder();
            sb.AppendLine("Stroj:      " + m.Machine.Name + (m.Machine.Project != null ? "  (" + m.Machine.Project + ")" : ""));
            sb.AppendLine("Export:     " + m.SourceExport);
            sb.AppendLine("Model:      " + (_modelPath ?? "(ni shranjen)"));
            sb.AppendLine("Izdelan:    " + m.GeneratedAt + "  " + m.Generator);
            sb.AppendLine("Tip:        " + m.Machine.Type + (m.Machine.Parts.Count > 0 ? "  deli: " + string.Join(", ", m.Machine.Parts) : "") +
                          (m.Machine.Lines.Count > 0 ? "  linije: " + string.Join(", ", m.Machine.Lines) : ""));
            sb.AppendLine("PLC:        " + m.Machine.Plc?.Name);
            foreach (var d in m.Machine.HmiDevices) sb.AppendLine("HMI:        " + d.Kind + "  " + d.Target + "  " + d.Resolution + "  povezava " + d.Connection);
            sb.AppendLine("DB:         " + string.Join(",  ", m.Machine.DbMap.Names.Select(kv => kv.Key + " = " + kv.Value)));
            if (m.Global.CylinderControlArraySize.HasValue) sb.AppendLine("Cylinder_Control: potrebna dolžina HMI polja " + m.Global.CylinderControlArraySize + " (Array[0.." + (m.Global.CylinderControlArraySize - 1) + "] of Bool)");
            sb.AppendLine();
            if (s != null)
            {
                sb.AppendLine("Enot " + s.Units + ",  časovnikov " + s.Timers + " (prikazanih " + s.TimersShown + "),  parametrov " + s.Parameters +
                              ",  recept " + s.RecipeParameters + ",  cilindrov " + s.Cylinders + ",  alarmov " + s.Alarms);
                sb.AppendLine("Besedila:  slovar/ročno " + s.TextsFromDictionary + ",  sestavljena (preveri) " + s.TextsComposed + ",  manjka " + s.TextsMissing);
            }
            int plc = PlcChanges.UnitsNeedingChange(m).Count();
            if (plc > 0) sb.AppendLine("Spremembe PLC: " + plc + " Settings DB-jev potrebuje časovnike kot Delay : Array[0..9] of S5Time (gumb 'Spremembe PLC').");
            sb.AppendLine("Opozoril:  " + m.Warnings.Count + "  (" + string.Join(", ", m.Warnings.GroupBy(w => w.Code).Select(g => g.Key + " ×" + g.Count())) + ")");
            _txtSummary.Text = sb.ToString();
        }

        void ShowUnit(Unit u)
        {
            CommitEdits();
            _unit = u;
            _loadingUnit = true;
            try
            {
                bool has = u != null;
                foreach (Control c in new Control[] { _txtSl, _txtEn, _txtDe, _numPlc, _chkPc, _chkPanel }) c.Enabled = has;
                _txtSl.Text = has ? Get(u.Text, "sl-SI") : "";
                _txtEn.Text = has ? Get(u.Text, "en-US") : "";
                _txtDe.Text = has ? Get(u.Text, "de-DE") : "";
                _numPlc.Value = has && u.PlcIndex.HasValue ? Math.Max(0, Math.Min(99, u.PlcIndex.Value)) : 0;
                UpdateHmiLabel();
                _chkPc.Checked = has && u.ShowOn.Contains("PC");
                _chkPanel.Checked = has && u.ShowOn.Contains("Panel");
                _txtInfo.Text = has ? UnitInfo(u) : "";
                _gridTimers.DataSource = has ? new BindingList<TimerRow>(u.Timers.Select(t => new TimerRow(t)).ToList()) : null;
                _gridParams.DataSource = has ? new BindingList<ParamRow>(u.Parameters.Select(p => new ParamRow(p)).ToList()) : null;
                _gridRecipe.DataSource = has ? new BindingList<ParamRow>(u.RecipeParameters.Select(p => new ParamRow(p)).ToList()) : null;
                _gridCyl.DataSource = has ? new BindingList<CylinderRow>(u.Cylinders.Select(c => new CylinderRow(c)).ToList()) : null;
                var alarms = has ? _model.Global.Alarms.Where(a => a.Unit == u.Id).Select(a => new AlarmRow(a)).ToList() : new List<AlarmRow>();
                _gridAlarms.DataSource = new BindingList<AlarmRow>(alarms);
                if (has)
                {
                    SetTabText(1, "Časovniki (" + u.Timers.Count(t => t.Show) + "/" + u.Timers.Count + ")");
                    SetTabText(2, "Parametri (" + u.Parameters.Count + ")");
                    SetTabText(3, "Recept (" + u.RecipeParameters.Count + ")");
                    SetTabText(4, "Cilindri (" + u.Cylinders.Count + ")");
                    SetTabText(5, "Alarmi (" + alarms.Count + ")");
                }
            }
            finally { _loadingUnit = false; }
        }

        void SetTabText(int i, string t) { if (_unitTabs.TabPages.Count > i) _unitTabs.TabPages[i].Text = t; }

        static string Get(LangText t, string lang) => t != null && t.TryGetValue(lang, out var v) ? v : "";

        string UnitInfo(Unit u)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FB:            " + u.Fb + "     mapa: " + u.Folder);
            sb.AppendLine("Del / skupina: " + (u.Part ?? "-") + (u.Group != null ? "  /  " + u.Group : "") + (u.Line != null ? "     linija " + u.Line : "") +
                          (u.Parent != null ? "     nadrejena " + u.Parent + " (pod " + u.SubIndex + ")" : ""));
            sb.AppendLine("Indeks:        " + (u.IndexSource ?? "-"));
            if (u.InheritsOffFromIndex.HasValue) sb.AppendLine("               OFF/ENABLE_HMI_COMMAND bere z indeksom nadrejene postaje " + u.InheritsOffFromIndex);
            sb.AppendLine("Funkcije DB1010: " + string.Join(", ", u.StationFunctions));
            sb.AppendLine("Settings DB:   " + (u.SettingsDb != null ? u.SettingsDb.Name + " [DB" + u.SettingsDb.Number + ", " + u.SettingsDb.Layout + "]" : "-"));
            var t = u.HmiTags?.Timers;
            if (t != null)
            {
                sb.AppendLine("HMI tag časovnikov: PC " + t.Pc + ",  panel " + t.Panel + "  →  " + t.PlcTag + "  (" + t.DataType + ")" + (t.RequiresPlcChange ? "  ⚠ potrebna sprememba PLC" : ""));
                if (!string.IsNullOrEmpty(t.Note)) sb.AppendLine("               " + t.Note);
            }
            if (u.PartBadCount != null) sb.AppendLine("Zapored slabi kosi: " + u.PartBadCount.Member + " (" + u.PartBadCount.DataType + ")");
            var w = _model.Warnings.Where(x => x.Unit != null && x.Unit.Split(new[] { ", " }, StringSplitOptions.None).Contains(u.Id)).ToList();
            if (w.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Opozorila:");
                foreach (var x in w) sb.AppendLine("  " + x.Code + " – " + x.Message);
            }
            return sb.ToString().Replace("\n", Environment.NewLine).Replace("\r\r", "\r");
        }

        // ================================================================ urejanje

        void UnitText(string lang, string value)
        {
            if (_loadingUnit || _unit == null) return;
            if (_unit.Text == null) _unit.Text = new LangText();
            _unit.Text[lang] = value;
            _unit.TextSource = TextSource.Manual;
            SetDirty(true);
            _gridUnits.InvalidateRow(_gridUnits.CurrentRow?.Index ?? 0);
        }

        void UnitIndexChanged()
        {
            UpdateHmiLabel();
            if (_loadingUnit || _unit == null) return;
            int v = (int)_numPlc.Value;
            if (_unit.PlcIndex == v) return;
            _unit.PlcIndex = v;
            _unit.HmiIndex = v - 1;
            if (_unit.IndexSource == null || !_unit.IndexSource.Contains("(ročno)")) _unit.IndexSource = (_unit.IndexSource ?? "") + " (ročno)";
            SetDirty(true);
            _gridUnits.Invalidate();
        }

        void UpdateHmiLabel() => _lblHmi.Text = "HMI indeks = " + ((int)_numPlc.Value - 1) + "   (PLC − 1, standard 5.3)";

        void UnitShowOn(string kind, bool on)
        {
            if (_loadingUnit || _unit == null) return;
            if (on && !_unit.ShowOn.Contains(kind)) _unit.ShowOn.Add(kind);
            if (!on) _unit.ShowOn.Remove(kind);
            SetDirty(true);
        }

        /// <summary>Potrdi urejanje celice, ki je še odprta (preden menjamo enoto ali shranjujemo).</summary>
        void CommitEdits()
        {
            foreach (var g in new[] { _gridTimers, _gridParams, _gridRecipe, _gridCyl, _gridAlarms, _gridGlobalAlarms })
                if (g != null && g.IsCurrentCellInEditMode) g.EndEdit();
        }

        void SetDirty(bool d)
        {
            _dirty = d;
            Text = "TIA_Generator – " + (_model?.Machine.Name ?? "generator HMI") + (d ? " *" : "");
            _btnSave.Enabled = _btnSaveAs.Enabled = _btnPlc.Enabled = _btnStructure.Enabled = _btnSkeleton.Enabled = _model != null;
        }

        // ================================================================ struktura stroja, PLC ogrodje

        void NewMachine()
        {
            if (!ConfirmDiscard()) return;
            var d = TIA_Generator.Structure.StructureBuilder.Create("Nov stroj", 3, 5);
            using (var dlg = new StructureDialog(d, "Nov stroj – sektorji, postaje, podpostaje"))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var m = TIA_Generator.Structure.StructureBuilder.ApplyToModel(dlg.Result, null, Dict);
                SetModel(m, null, dirty: true);
                _status.Text = "Nov stroj: " + m.Units.Count + " postaj – shrani model (Ctrl+S), nato 'PLC ogrodje ...'.";
                Log(_status.Text);
            }
        }

        void EditStructure()
        {
            if (_model == null) return;
            CommitEdits();
            var d = TIA_Generator.Structure.StructureBuilder.FromModel(_model);
            using (var dlg = new StructureDialog(d, "Struktura stroja – " + _model.Machine.Name))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                int before = _model.Units.Count;
                TIA_Generator.Structure.StructureBuilder.ApplyToModel(dlg.Result, _model, Dict);
                string sel = _unit?.Id;
                SetModel(_model, _modelPath, dirty: true);
                if (sel != null) SelectUnit(sel, 0);
                int fresh = _model.Units.Count(u => u.Origin == "struktura");
                _status.Text = "Struktura uveljavljena: " + _model.Units.Count + " enot (prej " + before + "), novih postaj " + fresh + ".";
                Log(_status.Text);
            }
        }

        void GenerateSkeleton()
        {
            if (_model == null) return;
            CommitEdits();
            var fresh = _model.Units.Where(u => u.Origin == "struktura").ToList();
            if (fresh.Count == 0)
            {
                MessageBox.Show(this, "V modelu ni novih postaj. PLC ogrodje se ustvari samo za postaje, dodane v 'Struktura ...' ali 'Nov stroj ...' (obstoječih iz PLC ne prepiše).", Text);
                return;
            }
            string defOut = _modelPath != null ? Path.Combine(Path.GetDirectoryName(_modelPath), SafeName(_model.Machine.Name) + "_PLC_ogrodje")
                                               : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SafeName(_model.Machine.Name) + "_PLC_ogrodje");
            using (var f = new Form { Text = "PLC ogrodje novih postaj (" + fresh.Count + ")", Size = new Size(760, 260), StartPosition = FormStartPosition.CenterParent, Font = Font, MinimizeBox = false, MaximizeBox = false, FormBorderStyle = FormBorderStyle.FixedDialog })
            {
                var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(10) };
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
                var tplDir = new TextBox { Dock = DockStyle.Fill, Text = Settings.Get("skeletonTemplate") ?? @"D:\Claude\TiaFullExport\Purem support pin\Project1_V20" };
                var tplUnit = new TextBox { Width = 90, Text = Settings.Get("skeletonUnit") ?? "A05" };
                var outDir = new TextBox { Dock = DockStyle.Fill, Text = defOut };
                Button Browse(TextBox tb) { var b = new Button { Text = "…", Width = 32 }; b.Click += (s, e) => { using (var d = new FolderBrowserDialog { SelectedPath = tb.Text }) if (d.ShowDialog(f) == DialogResult.OK) tb.Text = d.SelectedPath; }; return b; }
                void Row(string l, Control c, Control b) { t.RowCount++; t.Controls.Add(new Label { Text = l, AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, t.RowCount - 1); t.Controls.Add(c, 1, t.RowCount - 1); if (b != null) t.Controls.Add(b, 2, t.RowCount - 1); }
                Row("Vzorčni export", tplDir, Browse(tplDir));
                Row("Vzorčna enota", tplUnit, null);
                Row("Izhodna mapa", outDir, Browse(outDir));
                t.RowCount++;
                t.Controls.Add(new Label { Text = "Vzorec = standardno ogrodje postaje (Purem Unit_A05 »Spare«). Zamenjajo se oznaka, indeks postaje in časovniki → Delay[i]; številke FB/DB = PLC indeks, Settings = 100 + indeks.", AutoSize = false, Dock = DockStyle.Fill, Height = 40, ForeColor = Color.DimGray }, 0, t.RowCount - 1);
                t.SetColumnSpan(t.GetControlFromPosition(0, t.RowCount - 1), 3);
                var ok = new Button { Text = "Generiraj", DialogResult = DialogResult.OK, Width = 110, Height = 30 };
                var bp = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 4, 10, 0) };
                bp.Controls.Add(ok);
                f.Controls.Add(t); f.Controls.Add(bp); f.AcceptButton = ok;
                if (f.ShowDialog(this) != DialogResult.OK) return;

                Settings.Set("skeletonTemplate", tplDir.Text); Settings.Set("skeletonUnit", tplUnit.Text.Trim());
                try
                {
                    UseWaitCursor = true;
                    var r = new PlcSkeleton(Log).Generate(_model, new PlcSkeleton.Template { ExportDir = tplDir.Text, UnitId = tplUnit.Text.Trim() }, outDir.Text);
                    foreach (var l in r.Report) Log(l);
                    _status.Text = "PLC ogrodje: " + r.Units + " postaj, " + r.Files.Count + " datotek → " + outDir.Text;
                    if (MessageBox.Show(this, _status.Text + "\n\nPoročilo (potrebni simboli, trki številk) je v zavihku Dnevnik in v PLC_ogrodje_porocilo.txt.\nOdprem mapo?",
                                        Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        System.Diagnostics.Process.Start("explorer.exe", "\"" + outDir.Text + "\"");
                }
                catch (Exception ex) { Log("NAPAKA: " + ex); MessageBox.Show(this, ex.Message, "PLC ogrodje", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                finally { UseWaitCursor = false; }
            }
        }

        void SelectUnitFromWarning()
        {
            var w = (_gridWarnings.CurrentRow?.DataBoundItem as WarningRow)?.W;
            string id = w?.Unit?.Split(new[] { ", " }, StringSplitOptions.None).FirstOrDefault();
            if (id == null) return;
            _filter.Text = "";
            foreach (DataGridViewRow r in _gridUnits.Rows)
                if ((r.DataBoundItem as UnitRow)?.Id == id) { _gridUnits.CurrentCell = r.Cells[0]; _unitTabs.SelectedIndex = 0; break; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // vsaka sprememba v mrežah (besedila, prikaz, polje, razred) označi model kot spremenjen
            foreach (var g in new[] { _gridTimers, _gridParams, _gridRecipe, _gridCyl, _gridAlarms, _gridGlobalAlarms })
            {
                g.CellValueChanged += (s, ev) => { if (!_loadingUnit && ev.RowIndex >= 0) SetDirty(true); };
                g.CurrentCellDirtyStateChanged += (s, ev) => { if (g.CurrentCell is DataGridViewCheckBoxCell || g.CurrentCell is DataGridViewComboBoxCell) g.CommitEdit(DataGridViewDataErrorContexts.Commit); };
            }
        }
    }
}
