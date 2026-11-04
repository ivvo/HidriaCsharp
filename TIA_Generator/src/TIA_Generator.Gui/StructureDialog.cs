using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TIA_Generator.Structure;

namespace TIA_Generator.Gui
{
    /// <summary>
    /// Struktura stroja: sektorji (A–E) ali en sektor, število postaj v sektorju, podpostaje, opisi, PLC indeksi.
    /// Za nov stroj ali za obstoječ model (predizpolnjeno iz exporta).
    /// </summary>
    sealed class StructureDialog : Form
    {
        public StructureDef Result { get; private set; }
        readonly StructureDef _d;
        readonly HashSet<string> _existingAtStart;
        readonly TextBox _name;
        readonly RadioButton _rbSectors, _rbSingle;
        readonly CheckBox _pc, _panel;
        readonly DataGridView _gSectors, _gStations;
        readonly Button _addSector, _delSector;
        readonly TextBox _warnings;
        bool _busy;

        sealed class SectorRow
        {
            readonly SectorDef S; readonly Action _changed;
            public SectorRow(SectorDef s, Action changed) { S = s; _changed = changed; }
            public string Letter => S.Letter.Length == 0 ? "–" : S.Letter;
            public string Text { get => S.Text; set => S.Text = value ?? ""; }
            public int Count { get => S.StationCount; set { if (value < 0 || value > 99 || value == S.StationCount) return; S.StationCount = value; _changed(); } }
            public string Range => StructureBuilder.Ranges.TryGetValue(S.Letter, out var r) ? r.lo + "–" + r.hi : "1–99";
        }

        sealed class StationRow
        {
            readonly StationDef S; readonly Action _rebuild, _validate;
            public StationRow(StationDef s, Action rebuild, Action validate) { S = s; _rebuild = rebuild; _validate = validate; }
            public string Unit => (S.IsSub ? "    " : "") + "Unit_" + S.Id;
            public string Text { get => S.Text; set => S.Text = (value ?? "").Trim(); }
            public string Subs { get => S.IsSub ? "" : S.SubCount.ToString(); set { if (S.IsSub || !int.TryParse(value, out int v) || v < 0 || v > 9 || v == S.SubCount) return; S.SubCount = v; _rebuild(); } }
            public string Plc { get => S.Virtual ? "" : S.PlcIndex.ToString(); set { if (S.Virtual || !int.TryParse(value, out int v) || v < 0 || v > 99) return; S.PlcIndex = v; _validate(); } }
            public string Hmi => S.Virtual ? "" : S.HmiIndex.ToString();
            public string State => S.Virtual ? "samo nosilec podpostaj" : S.Existing ? "obstaja v PLC" : "nova";
            public StationDef Def => S;
        }

        public StructureDialog(StructureDef d, string title)
        {
            _d = d;
            _existingAtStart = new HashSet<string>(d.Stations.Where(s => s.Existing).Select(s => s.Id));
            Text = title;
            Font = new Font("Segoe UI", 9f);
            Size = new Size(1150, 760);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;

            // --- zgoraj: stroj
            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 74, ColumnCount = 6, Padding = new Padding(10, 8, 10, 0) };
            _name = new TextBox { Width = 320, Text = d.MachineName };
            _rbSectors = new RadioButton { Text = "Sektorji A, B, C …  (Unit_A01 …)", AutoSize = true, Checked = d.Sectored };
            _rbSingle = new RadioButton { Text = "En sektor  (Unit_01 …)", AutoSize = true, Checked = !d.Sectored };
            _pc = new CheckBox { Text = "PC (WinCC RT Advanced 1920×1080)", AutoSize = true, Checked = d.Pc };
            _panel = new CheckBox { Text = "Panel (Comfort 800×480)", AutoSize = true, Checked = d.Panel };
            top.Controls.Add(new Label { Text = "Ime stroja", AutoSize = true, Padding = new Padding(0, 5, 0, 0) }, 0, 0);
            top.Controls.Add(_name, 1, 0);
            top.Controls.Add(_rbSectors, 2, 0);
            top.Controls.Add(_rbSingle, 3, 0);
            top.Controls.Add(new Label { Text = "HMI naprave", AutoSize = true, Padding = new Padding(0, 5, 0, 0) }, 0, 1);
            var dev = new FlowLayoutPanel { AutoSize = true };
            dev.Controls.Add(_pc); dev.Controls.Add(_panel);
            top.Controls.Add(dev, 1, 1);
            top.SetColumnSpan(dev, 3);

            // --- levo: sektorji
            _gSectors = Grid();
            AddCol(_gSectors, "Sektor", "Letter", 55, true);
            AddCol(_gSectors, "Opis sektorja", "Text", 150, false);
            AddCol(_gSectors, "Št. postaj", "Count", 70, false);
            AddCol(_gSectors, "PLC razpon", "Range", 75, true);
            var secButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(0, 4, 0, 0) };
            _addSector = new Button { Text = "+ sektor", Width = 90 };
            _delSector = new Button { Text = "− zadnji sektor", Width = 110 };
            secButtons.Controls.Add(_addSector); secButtons.Controls.Add(_delSector);
            var left = new GroupBox { Text = "Sektorji", Dock = DockStyle.Fill, Padding = new Padding(8) };
            left.Controls.Add(_gSectors); left.Controls.Add(secButtons);

            // --- desno: postaje
            _gStations = Grid();
            AddCol(_gStations, "FB", "Unit", 120, true);
            AddCol(_gStations, "Opis postaje (SL)", "Text", 230, false);
            AddCol(_gStations, "Podpostaj", "Subs", 70, false);
            AddCol(_gStations, "PLC indeks", "Plc", 75, false);
            AddCol(_gStations, "HMI indeks", "Hmi", 75, true);
            AddCol(_gStations, "Stanje", "State", 150, true);
            _gStations.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var r = _gStations.Rows[e.RowIndex].DataBoundItem as StationRow;
                if (r?.Def.IsSub == true) e.CellStyle.ForeColor = Color.FromArgb(29, 83, 131);
                if (r?.Def.Existing == false && !r.Def.Virtual && _gStations.Columns[e.ColumnIndex].DataPropertyName == "State") e.CellStyle.ForeColor = Color.ForestGreen;
            };
            var right = new GroupBox { Text = "Postaje in podpostaje  (opis, število podpostaj in PLC indeks so urejljivi)", Dock = DockStyle.Fill, Padding = new Padding(8) };
            right.Controls.Add(_gStations);

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
            Load += (s, e) => split.SplitterDistance = 400;   // šele ko je velikost okna znana
            split.Panel1.Controls.Add(left);
            split.Panel2.Controls.Add(right);

            // --- spodaj: opozorila + gumbi
            _warnings = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, ForeColor = Color.DarkOrange, BackColor = SystemColors.Window };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 420, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 6, 6, 0) };
            var ok = new Button { Text = "Uveljavi", Width = 110, Height = 30 };
            var cancel = new Button { Text = "Prekliči", Width = 90, Height = 30, DialogResult = DialogResult.Cancel };
            var realloc = new Button { Text = "Indekse novih postaj dodeli znova", Width = 200, Height = 30 };
            buttons.Controls.Add(ok); buttons.Controls.Add(cancel); buttons.Controls.Add(realloc);
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 90, Padding = new Padding(10, 4, 4, 6) };
            bottom.Controls.Add(_warnings); bottom.Controls.Add(buttons);
            CancelButton = cancel;

            Controls.Add(split);
            Controls.Add(bottom);
            Controls.Add(top);

            _rbSectors.CheckedChanged += (s, e) => { if (_rbSectors.Checked) ChangeMode(true); };
            _rbSingle.CheckedChanged += (s, e) => { if (_rbSingle.Checked) ChangeMode(false); };
            _addSector.Click += (s, e) => AddSector();
            _delSector.Click += (s, e) => RemoveSector();
            realloc.Click += (s, e) => { foreach (var st in _d.Stations.Where(x => !x.Existing)) st.PlcIndex = 0; StructureBuilder.AllocateIndices(_d); Refresh2(); };
            ok.Click += (s, e) => Accept();
            foreach (var g in new[] { _gSectors, _gStations })
                g.CellEndEdit += (s, e) => BeginInvoke((Action)Refresh2);

            Refresh2();
        }

        static DataGridView Grid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false,
                BackgroundColor = SystemColors.Window, BorderStyle = BorderStyle.None, AllowUserToResizeRows = false,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2,
            };
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(29, 83, 131);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            g.DataError += (s, e) => e.ThrowException = false;
            return g;
        }

        static void AddCol(DataGridView g, string h, string p, int w, bool ro)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = h, DataPropertyName = p, Width = w, ReadOnly = ro, SortMode = DataGridViewColumnSortMode.NotSortable };
            if (ro) c.DefaultCellStyle.ForeColor = Color.DimGray; else c.DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 230);
            g.Columns.Add(c);
        }

        void ChangeMode(bool sectored)
        {
            if (_busy || _d.Sectored == sectored) return;
            int total = Math.Max(1, _d.Stations.Count(s => !s.IsSub));
            if (_d.Stations.Any(s => s.Existing) &&
                MessageBox.Show(this, "Sprememba načina preimenuje vse postaje (A01 ↔ 01). Obstoječe postaje bodo iz modela odstranjene. Nadaljujem?", Text,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                _busy = true; _rbSectors.Checked = _d.Sectored; _rbSingle.Checked = !_d.Sectored; _busy = false; return;
            }
            _d.Sectored = sectored;
            _d.Sectors.Clear();
            _d.Stations.Clear();
            if (sectored) _d.Sectors.Add(new SectorDef { Letter = "A", StationCount = total });
            else _d.Sectors.Add(new SectorDef { Letter = "", StationCount = total });
            Rebuild();
        }

        void AddSector()
        {
            if (!_d.Sectored) return;
            var next = StructureBuilder.SectorLetters.FirstOrDefault(l => _d.Sectors.All(s => s.Letter != l));
            if (next == null) { MessageBox.Show(this, "Standard 5.3 pozna sektorje A–E.", Text); return; }
            _d.Sectors.Add(new SectorDef { Letter = next, StationCount = 1 });
            Rebuild();
        }

        void RemoveSector()
        {
            if (!_d.Sectored || _d.Sectors.Count <= 1) return;
            var last = _d.Sectors.Last();
            if (_d.Stations.Any(s => s.Sector == last.Letter && s.Existing) &&
                MessageBox.Show(this, "Sektor " + last.Letter + " ima postaje, ki obstajajo v PLC. Odstranim jih iz modela?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            _d.Sectors.Remove(last);
            Rebuild();
        }

        void Rebuild()
        {
            StructureBuilder.Rebuild(_d);
            Refresh2();
        }

        void Refresh2()
        {
            _busy = true;
            try
            {
                _gSectors.DataSource = new BindingList<SectorRow>(_d.Sectors.Select(s => new SectorRow(s, () => BeginInvoke((Action)Rebuild))).ToList());
                int first = _gStations.FirstDisplayedScrollingRowIndex;
                _gStations.DataSource = new BindingList<StationRow>(_d.Stations.Select(s => new StationRow(s, () => BeginInvoke((Action)Rebuild), () => { })).ToList());
                if (first >= 0 && first < _gStations.RowCount) _gStations.FirstDisplayedScrollingRowIndex = first;
                _addSector.Enabled = _delSector.Enabled = _d.Sectored;
                var w = StructureBuilder.Validate(_d);
                int main = _d.Stations.Count(s => !s.IsSub && !s.Virtual), subs = _d.Stations.Count(s => s.IsSub);
                _warnings.ForeColor = w.Count > 0 ? Color.DarkOrange : Color.ForestGreen;
                _warnings.Text = (_d.Sectored ? _d.Sectors.Count + " sektorjev, " : "en sektor, ") + main + " postaj, " + subs + " podpostaj, novih " +
                                 _d.Stations.Count(s => !s.Existing && !s.Virtual) + "." + Environment.NewLine +
                                 (w.Count == 0 ? "Struktura je skladna s standardom 5.3." : string.Join(Environment.NewLine, w));
            }
            finally { _busy = false; }
        }

        void Accept()
        {
            _gStations.EndEdit(); _gSectors.EndEdit();
            _d.MachineName = string.IsNullOrWhiteSpace(_name.Text) ? "Nov stroj" : _name.Text.Trim();
            _d.Pc = _pc.Checked; _d.Panel = _panel.Checked;
            if (!_d.Pc && !_d.Panel) { MessageBox.Show(this, "Izberi vsaj eno HMI napravo.", Text); return; }
            var removed = _existingAtStart.Where(id => _d.Stations.All(s => s.Id != id)).ToList();
            if (removed.Count > 0 && MessageBox.Show(this, "Iz modela bodo odstranjene postaje, ki obstajajo v PLC:\n" + string.Join(", ", removed) + "\n\nNadaljujem?",
                                                     Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var w = StructureBuilder.Validate(_d);
            if (w.Count > 0 && MessageBox.Show(this, "Struktura ima opozorila:\n\n" + string.Join("\n", w.Take(10)) + "\n\nVseeno uveljavim?",
                                               Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Result = _d;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
