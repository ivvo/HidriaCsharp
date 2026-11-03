using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIA_ToolKit
{
    sealed class MainForm : Form
    {
        readonly StaWorker _worker = new StaWorker();
        readonly TiaService _tia = new TiaService();
        readonly string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");

        // Povezava
        ComboBox _cbProcess;
        Button _btnRefresh, _btnConnect, _btnOpenProtected;
        Label _lblStatus;
        TextBox _tbOutDir;

        // Drevo: target -> Screen templates / Screens -> mape -> zasloni
        TreeView _tree;
        Label _lblSelection;
        bool _checking;

        // Mapiranje
        DataGridView _gridMap;

        // Analiza
        DataGridView _gridUsed;

        // Akcije
        Button _btnExport, _btnDryRun, _btnReplace, _btnImport, _btnAnalyze, _btnExportAll, _btnAnalyzeExport, _btnCancel;

        TextBox _tbLog;

        const int ColOldSwatch = 0, ColOldRgb = 1, ColArrow = 2, ColNewSwatch = 3, ColNewRgb = 4;
        static readonly Color PartialColor = Color.RoyalBlue;

        public MainForm()
        {
            Text = "TIA ToolKit - Hidria  [TIA Portal " + Program.TiaVersion + "]";
            Size = new Size(1450, 900);
            MinimumSize = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9f);

            BuildUi();
            LoadMapping(ColorMapping.DefaultFile);
            _tbOutDir.Text = @"C:\TIA_ColorExport";

            Shown += async (s, e) => await RefreshProcesses();
            FormClosing += (s, e) =>
            {
                TrySaveMapping(ColorMapping.DefaultFile, quiet: true);
                // Odklop na Openness threadu, nato zaustavi thread. Projekt se NE shrani.
                try { _worker.Run(() => _tia.Disconnect()).Wait(3000); } catch { }
                _worker.Dispose();
            };
        }

        // ================================================================== UI

        void BuildUi()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(6) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            Controls.Add(root);

            // --- vrstica 1: TIA proces
            var conn = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            conn.Controls.Add(MakeLabel("TIA Portal proces:"));
            _cbProcess = new ComboBox { Width = 480, DropDownStyle = ComboBoxStyle.DropDownList };
            conn.Controls.Add(_cbProcess);
            _btnRefresh = MakeButton("Osveži", async (s, e) => await RefreshProcesses());
            _btnConnect = MakeButton("Poveži / osveži drevo", async (s, e) => await Connect());
            _btnOpenProtected = MakeButton("Odpri zaščiten projekt ...", async (s, e) => await OpenProtectedProject());
            conn.Controls.Add(_btnRefresh);
            conn.Controls.Add(_btnConnect);
            conn.Controls.Add(_btnOpenProtected);
            _lblStatus = MakeLabel("Ni povezave.");
            _lblStatus.ForeColor = Color.DarkRed;
            conn.Controls.Add(_lblStatus);
            root.Controls.Add(conn, 0, 0);

            // --- vrstica 2: mapa za export
            var dirRow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
            dirRow.Controls.Add(MakeLabel("Mapa za XML export:"));
            _tbOutDir = new TextBox { Width = 480 };
            dirRow.Controls.Add(_tbOutDir);
            dirRow.Controls.Add(MakeButton("...", (s, e) => BrowseOutDir()));
            dirRow.Controls.Add(MakeButton("Odpri mapo", (s, e) =>
            {
                if (Directory.Exists(_tbOutDir.Text)) System.Diagnostics.Process.Start("explorer.exe", "\"" + _tbOutDir.Text + "\"");
            }));
            root.Controls.Add(dirRow, 0, 1);

            // --- vrstica 3: drevo | mapiranje | analiza
            var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
            root.Controls.Add(main, 0, 2);

            main.Controls.Add(BuildTreeGroup(), 0, 0);
            main.Controls.Add(BuildMappingGroup(), 1, 0);
            main.Controls.Add(BuildAnalysisGroup(), 2, 0);

            // --- vrstica 4: akcije
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, Padding = new Padding(0, 4, 0, 4) };
            _btnExport = MakeBigButton("1. Export izbranih", async (s, e) => await ExportSelected());
            _btnDryRun = MakeBigButton("2a. Zamenjaj barve (dry-run)", async (s, e) => await ReplaceColors(true));
            _btnReplace = MakeBigButton("2b. Zamenjaj barve (dejansko)", async (s, e) => await ReplaceColors(false));
            _btnImport = MakeBigButton("3. Import izbranih", async (s, e) => await ImportSelected());
            _btnReplace.BackColor = Color.FromArgb(255, 235, 200);
            _btnImport.BackColor = Color.FromArgb(255, 220, 220);
            _btnExportAll = MakeBigButton("Export ALL (analiza) ...", async (s, e) => await ExportAll());
            _btnExportAll.BackColor = Color.FromArgb(215, 235, 255);
            _btnAnalyzeExport = MakeBigButton("Analiza exporta (napake) ...", async (s, e) => await AnalyzeExport());
            _btnAnalyzeExport.BackColor = Color.FromArgb(220, 245, 220);
            _btnCancel = MakeButton("Prekini", (s, e) =>
            {
                _tia.CancelRequested = true;
                Log("Prekinjam po trenutnem objektu ...");
            });
            _btnCancel.Enabled = false;
            _btnCancel.Margin = new Padding(3, 8, 3, 3);
            actions.Controls.AddRange(new Control[] { _btnExport, _btnDryRun, _btnReplace, _btnImport, _btnExportAll, _btnAnalyzeExport, _btnCancel });
            var note = MakeLabel("Vse faze delajo samo na IZBRANIH elementih v drevesu. Projekt se NIKOLI ne shrani samodejno (Ctrl+S v TIA).");
            note.ForeColor = Color.DarkRed;
            note.Padding = new Padding(12, 10, 0, 0);
            actions.Controls.Add(note);
            root.Controls.Add(actions, 0, 3);

            // --- vrstica 5: log
            _tbLog = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both,
                WordWrap = false, Font = new Font("Consolas", 9f), BackColor = Color.White
            };
            root.Controls.Add(_tbLog, 0, 4);
        }

        Control BuildTreeGroup()
        {
            var gb = new GroupBox { Text = "HMI targeti / mape / zasloni (privzeto nič izbrano)", Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            gb.Controls.Add(layout);

            _tree = new TreeView { Dock = DockStyle.Fill, CheckBoxes = true, HideSelection = false, ShowNodeToolTips = true };
            _tree.AfterCheck += TreeAfterCheck;
            layout.Controls.Add(_tree, 0, 0);

            var btns = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            btns.Controls.Add(MakeButton("Izberi vse", (s, e) => CheckAll(true)));
            btns.Controls.Add(MakeButton("Počisti izbor", (s, e) => CheckAll(false)));
            btns.Controls.Add(MakeButton("Razširi", (s, e) => _tree.ExpandAll()));
            btns.Controls.Add(MakeButton("Skrči", (s, e) =>
            {
                _tree.CollapseAll();
                foreach (TreeNode n in _tree.Nodes) n.Expand();
            }));
            layout.Controls.Add(btns, 0, 1);

            _lblSelection = MakeLabel("Izbrano: nič");
            _lblSelection.Padding = new Padding(3, 2, 0, 4);
            layout.Controls.Add(_lblSelection, 0, 2);
            return gb;
        }

        Control BuildMappingGroup()
        {
            var gb = new GroupBox { Text = "Mapiranje barv (stara -> nova)", Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            gb.Controls.Add(layout);

            _gridMap = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToResizeRows = false,
                RowHeadersWidth = 24, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = SystemColors.Window
            };
            _gridMap.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stara", ReadOnly = true, FillWeight = 18 });
            _gridMap.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stara RGB", FillWeight = 30 });
            _gridMap.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", ReadOnly = true, FillWeight = 6 });
            _gridMap.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nova", ReadOnly = true, FillWeight = 18 });
            _gridMap.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nova RGB", FillWeight = 30 });
            foreach (DataGridViewColumn c in _gridMap.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            _gridMap.CellValueChanged += (s, e) => { if (e.RowIndex >= 0) UpdateSwatches(_gridMap.Rows[e.RowIndex]); };
            _gridMap.CellClick += MapCellClick;
            layout.Controls.Add(_gridMap, 0, 0);

            var btns = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            btns.Controls.Add(MakeButton("Dodaj par", (s, e) => AddMapRow("", "")));
            btns.Controls.Add(MakeButton("Briši izbrane", (s, e) =>
            {
                foreach (DataGridViewRow r in _gridMap.SelectedRows.Cast<DataGridViewRow>().ToList()) _gridMap.Rows.Remove(r);
            }));
            btns.Controls.Add(MakeButton("Shrani mapiranje ...", (s, e) =>
            {
                using (var d = new SaveFileDialog { Filter = "Mapiranje (*.xml)|*.xml", FileName = "mapping.xml" })
                    if (d.ShowDialog(this) == DialogResult.OK) TrySaveMapping(d.FileName, quiet: false);
            }));
            btns.Controls.Add(MakeButton("Naloži mapiranje ...", (s, e) =>
            {
                using (var d = new OpenFileDialog { Filter = "Mapiranje (*.xml)|*.xml" })
                    if (d.ShowDialog(this) == DialogResult.OK) LoadMapping(d.FileName);
            }));
            layout.Controls.Add(btns, 0, 1);

            var palette = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            palette.Controls.Add(MakeLabel("Odobrena paleta -> NOVA barva izbranih vrstic:"));
            foreach (var c in ColorMapping.ApprovedPalette)
            {
                var col = c;
                var b = new Button
                {
                    Text = col.ToString(), Width = 92, Height = 30, BackColor = col.ToColor(),
                    ForeColor = Brightness(col) < 140 ? Color.White : Color.Black, FlatStyle = FlatStyle.Flat
                };
                b.Click += (s, e) => SetNewColorOnSelected(col);
                palette.Controls.Add(b);
            }
            layout.Controls.Add(palette, 0, 2);
            return gb;
        }

        Control BuildAnalysisGroup()
        {
            var gb = new GroupBox { Text = "Uporabljene barve v exportu (dvoklik = dodaj v mapiranje)", Dock = DockStyle.Fill };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            gb.Controls.Add(layout);

            _btnAnalyze = MakeButton("Analiziraj export (izbrani)", async (s, e) => await Analyze());
            layout.Controls.Add(_btnAnalyze, 0, 0);

            _gridUsed = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = SystemColors.Window
            };
            _gridUsed.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", FillWeight = 12 });
            _gridUsed.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RGB", FillWeight = 28 });
            _gridUsed.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Vloga", FillWeight = 28 });
            _gridUsed.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Aktivno", FillWeight = 16, ValueType = typeof(int) });
            _gridUsed.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Neaktivno", FillWeight = 16, ValueType = typeof(int) });
            _gridUsed.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Datotek", FillWeight = 14, ValueType = typeof(int) });
            _gridUsed.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                string rgb = (string)_gridUsed.Rows[e.RowIndex].Cells[1].Value;
                bool exists = _gridMap.Rows.Cast<DataGridViewRow>().Any(r =>
                {
                    RgbColor a, b;
                    return RgbColor.TryParse(Convert.ToString(r.Cells[ColOldRgb].Value), out a) && RgbColor.TryParse(rgb, out b) && a.Equals(b);
                });
                if (exists) { Log("Barva " + rgb + " je že v mapiranju."); return; }
                var row = AddMapRow(rgb, "");
                _gridMap.ClearSelection();
                row.Selected = true;
                Log("Dodano v mapiranje: " + rgb + " -> (izberi novo barvo iz palete)");
            };
            layout.Controls.Add(_gridUsed, 0, 1);
            return gb;
        }

        static Label MakeLabel(string text)
        {
            return new Label { Text = text, AutoSize = true, Padding = new Padding(0, 7, 0, 0) };
        }

        static Button MakeButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 28 };
            b.Click += click;
            return b;
        }

        static Button MakeBigButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, Width = 210, Height = 38, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            b.Click += click;
            return b;
        }

        static int Brightness(RgbColor c) { return (c.R * 299 + c.G * 587 + c.B * 114) / 1000; }

        // ================================================================== Drevo

        /// <summary>Zgradi drevo: target -> "Screen templates"/"Screens" -> mape -> zasloni (vse neoznačeno).</summary>
        void BuildTree(List<Tuple<HmiTargetInfo, List<HmiItem>>> data)
        {
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            foreach (var d in data)
            {
                var tn = new TreeNode(d.Item1.ToString()) { Tag = d.Item1, Name = d.Item1.Name };
                AddGroup(tn, "Screen templates", d.Item2.Where(i => i.Kind == HmiItemKind.Template));
                AddGroup(tn, "Screens", d.Item2.Where(i => i.Kind == HmiItemKind.Screen));
                _tree.Nodes.Add(tn);
                tn.Expand();
                foreach (TreeNode g in tn.Nodes) g.Expand();
            }
            foreach (TreeNode tn in _tree.Nodes) ApplyCounts(tn);
            _tree.EndUpdate();
            UpdateSelectionLabel();
        }

        static void AddGroup(TreeNode targetNode, string title, IEnumerable<HmiItem> items)
        {
            var list = items.ToList();
            if (list.Count == 0) return;
            var group = new TreeNode(title) { Name = title };
            var folders = new Dictionary<string, TreeNode>();   // TIA pot -> vozlišče mape

            foreach (var item in list)
            {
                var parent = group;
                string path = "";
                if (item.TiaPath.Length > 0)
                    foreach (var seg in item.TiaPath.Split('/'))
                    {
                        path = path.Length == 0 ? seg : path + "/" + seg;
                        TreeNode f;
                        if (!folders.TryGetValue(path, out f))
                        {
                            f = new TreeNode(seg) { Name = seg, ToolTipText = path };
                            // mape pred zasloni (kot v TIA)
                            int idx = parent.Nodes.Cast<TreeNode>().Count(n => !(n.Tag is HmiItem));
                            parent.Nodes.Insert(idx, f);
                            folders[path] = f;
                        }
                        parent = f;
                    }
                parent.Nodes.Add(new TreeNode(item.Name)
                {
                    Tag = item, Name = item.Name,
                    ToolTipText = (item.TiaPath.Length > 0 ? item.TiaPath + "/" : "") + item.Name
                });
            }
            targetNode.Nodes.Add(group);
        }

        /// <summary>Mapam doda število vsebovanih zaslonov: "Buttons (12)".</summary>
        static int ApplyCounts(TreeNode node)
        {
            if (node.Tag is HmiItem) return 1;
            int n = 0;
            foreach (TreeNode c in node.Nodes) n += ApplyCounts(c);
            if (!(node.Tag is HmiTargetInfo)) node.Text = node.Name + " (" + n + ")";
            return n;
        }

        void TreeAfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_checking) return;
            _checking = true;
            try
            {
                SetChecked(e.Node.Nodes, e.Node.Checked);
                e.Node.ForeColor = Color.Empty;
                UpdateParents(e.Node.Parent);
            }
            finally { _checking = false; }
            UpdateSelectionLabel();
        }

        static void SetChecked(TreeNodeCollection nodes, bool value)
        {
            foreach (TreeNode n in nodes)
            {
                n.Checked = value;
                n.ForeColor = Color.Empty;
                SetChecked(n.Nodes, value);
            }
        }

        /// <summary>Starš je označen, če so označeni vsi otroci; delno izbran starš je obarvan modro.</summary>
        static void UpdateParents(TreeNode parent)
        {
            while (parent != null)
            {
                var children = parent.Nodes.Cast<TreeNode>().ToList();
                bool all = children.All(c => c.Checked);
                bool any = children.Any(c => c.Checked || c.ForeColor == PartialColor);
                parent.Checked = all;
                parent.ForeColor = !all && any ? PartialColor : Color.Empty;
                parent = parent.Parent;
            }
        }

        void CheckAll(bool value)
        {
            _checking = true;
            try { SetChecked(_tree.Nodes, value); }
            finally { _checking = false; }
            UpdateSelectionLabel();
        }

        List<HmiItem> SelectedItems()
        {
            var list = new List<HmiItem>();
            CollectChecked(_tree.Nodes, list);
            return list;
        }

        static void CollectChecked(TreeNodeCollection nodes, List<HmiItem> list)
        {
            foreach (TreeNode n in nodes)
            {
                var item = n.Tag as HmiItem;
                if (item != null) { if (n.Checked) list.Add(item); }
                else CollectChecked(n.Nodes, list);
            }
        }

        void UpdateSelectionLabel()
        {
            var sel = SelectedItems();
            if (sel.Count == 0) { _lblSelection.Text = "Izbrano: nič"; return; }
            _lblSelection.Text = "Izbrano: " + sel.Count(i => i.Kind == HmiItemKind.Screen) + " zaslonov, " +
                                 sel.Count(i => i.Kind == HmiItemKind.Template) + " predlog na " +
                                 sel.Select(i => i.Target).Distinct().Count() + " targetih";
        }

        // ================================================================== Mapiranje

        DataGridViewRow AddMapRow(string oldRgb, string newRgb)
        {
            int i = _gridMap.Rows.Add("", oldRgb, "->", "", newRgb);
            var row = _gridMap.Rows[i];
            UpdateSwatches(row);
            return row;
        }

        void UpdateSwatches(DataGridViewRow row)
        {
            UpdateSwatch(row.Cells[ColOldRgb], row.Cells[ColOldSwatch]);
            UpdateSwatch(row.Cells[ColNewRgb], row.Cells[ColNewSwatch]);
        }

        static void UpdateSwatch(DataGridViewCell rgbCell, DataGridViewCell swatch)
        {
            RgbColor c;
            string text = Convert.ToString(rgbCell.Value);
            if (RgbColor.TryParse(text, out c))
            {
                swatch.Style.BackColor = c.ToColor();
                swatch.Style.SelectionBackColor = c.ToColor();
                rgbCell.Style.ForeColor = Color.Black;
            }
            else
            {
                swatch.Style.BackColor = Color.White;
                swatch.Style.SelectionBackColor = Color.White;
                rgbCell.Style.ForeColor = string.IsNullOrWhiteSpace(text) ? Color.Black : Color.Red;
            }
        }

        void MapCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || (e.ColumnIndex != ColOldSwatch && e.ColumnIndex != ColNewSwatch)) return;
            var rgbCell = _gridMap.Rows[e.RowIndex].Cells[e.ColumnIndex == ColOldSwatch ? ColOldRgb : ColNewRgb];
            using (var dlg = new ColorDialog { FullOpen = true, AnyColor = true })
            {
                RgbColor cur;
                if (RgbColor.TryParse(Convert.ToString(rgbCell.Value), out cur)) dlg.Color = cur.ToColor();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    rgbCell.Value = RgbColor.FromColor(dlg.Color).ToString();
            }
        }

        void SetNewColorOnSelected(RgbColor c)
        {
            var rows = _gridMap.SelectedRows.Cast<DataGridViewRow>().ToList();
            if (rows.Count == 0 && _gridMap.CurrentRow != null) rows.Add(_gridMap.CurrentRow);
            if (rows.Count == 0) { Log("Najprej izberi vrstico v mapiranju."); return; }
            foreach (var r in rows) r.Cells[ColNewRgb].Value = c.ToString();
        }

        List<ColorPair> CurrentPairs()
        {
            _gridMap.EndEdit();
            return _gridMap.Rows.Cast<DataGridViewRow>()
                .Select(r => new ColorPair { Old = Convert.ToString(r.Cells[ColOldRgb].Value), New = Convert.ToString(r.Cells[ColNewRgb].Value) })
                .Where(p => !string.IsNullOrWhiteSpace(p.Old) || !string.IsNullOrWhiteSpace(p.New))
                .ToList();
        }

        void LoadMapping(string file)
        {
            try
            {
                var pairs = ColorMapping.Load(file);
                _gridMap.Rows.Clear();
                foreach (var p in pairs) AddMapRow(p.Old, p.New);
                Log("Mapiranje naloženo (" + pairs.Count + " parov)" + (File.Exists(file) ? ": " + file : " - privzeto."));
            }
            catch (Exception ex) { Log("NAPAKA pri nalaganju mapiranja: " + ex.Message); }
        }

        void TrySaveMapping(string file, bool quiet)
        {
            try
            {
                ColorMapping.Save(file, CurrentPairs());
                if (!quiet) Log("Mapiranje shranjeno: " + file);
            }
            catch (Exception ex) { if (!quiet) Log("NAPAKA pri shranjevanju mapiranja: " + ex.Message); }
        }

        // ================================================================== TIA

        async Task RefreshProcesses()
        {
            int found = -1;
            await RunBusy(async () =>
            {
                var list = await _worker.Run(() => TiaService.ListProcesses());
                _cbProcess.Items.Clear();
                foreach (var p in list) _cbProcess.Items.Add(p);
                if (list.Count > 0) _cbProcess.SelectedIndex = 0;
                found = list.Count;
                Log("Najdenih odprtih TIA Portal " + Program.TiaVersion + " procesov: " + list.Count);
            });
            if (found == 0) OfferOtherVersion();
        }

        /// <summary>
        /// Ta exe vidi samo TIA Portal svoje verzije. Če je odprt TIA druge verzije, to pove in ponudi zagon ustreznega exe-ja.
        /// </summary>
        void OfferOtherVersion()
        {
            var others = Program.RunningTiaVersions().Where(v => v != Program.TiaVersion).ToList();
            if (others.Count == 0)
            {
                Log("Ni odprtega TIA Portala. Odpri projekt v TIA Portal " + Program.TiaVersion + " in klikni Osveži.");
                return;
            }

            Log("Odprt je TIA Portal " + string.Join(", ", others) + " - ta exe je za " + Program.TiaVersion + " in ga ne vidi.");
            foreach (var v in others)
            {
                string exe = Program.ExeForVersion(v);
                if (!File.Exists(exe))
                {
                    Log("  Exe za " + v + " ni zgrajen (" + exe + "). Zgradi ga z: msbuild TIA_ToolKit.sln /p:TiaVersion=" + v);
                    continue;
                }
                var r = MessageBox.Show(this,
                    "Odprt je TIA Portal " + v + ", to orodje pa je zgrajeno za " + Program.TiaVersion + ".\n\n" +
                    "Zaženem različico orodja za " + v + "?\n" + exe,
                    "Druga verzija TIA Portala", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe) { WorkingDirectory = Path.GetDirectoryName(exe) });
                Close();
                return;
            }
        }

        async Task Connect()
        {
            var proc = _cbProcess.SelectedItem as TiaProcessInfo;
            if (proc == null) { Log("Izberi TIA Portal proces (Osveži, če seznam ni aktualen)."); return; }
            await LoadProject(() => _tia.Connect(proc.Id, LogFromWorker));
        }

        /// <summary>
        /// Zaščiten projekt (UMAC): ga orodje odpre samo, s prijavo, v izbranem TIA Portalu brez odprtega projekta.
        /// Geslo gre neposredno TIA-ju (SecureString), ne shrani se in se ne izpiše v log.
        /// </summary>
        async Task OpenProtectedProject()
        {
            var proc = _cbProcess.SelectedItem as TiaProcessInfo;
            if (proc == null) { Log("Izberi TIA Portal proces (Osveži, če seznam ni aktualen)."); return; }

            string file;
            using (var d = new OpenFileDialog
            {
                Title = "Izberi TIA Portal projekt (" + Program.TiaVersion + ")",
                Filter = "TIA Portal projekt (*.ap*)|*.ap*|Vse datoteke (*.*)|*.*"
            })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                file = d.FileName;
            }

            string user; bool global; System.Security.SecureString pwd;
            if (!LoginDialog.Ask(this, Path.GetFileName(file), out user, out pwd, out global)) return;

            try
            {
                await LoadProject(() => _tia.OpenProtectedProject(proc.Id, file, user, pwd, global, LogFromWorker));
            }
            finally
            {
                pwd.Dispose();
            }
        }

        async Task LoadProject(Action connect)
        {
            bool processGone = false;
            await RunBusy(async () =>
            {
                try { await LoadProjectCore(connect); }
                catch (TiaProcessGoneException) { processGone = true; throw; }
            });
            if (processGone) await RefreshProcesses();   // izbran TIA je bil zaprt - osveži seznam
        }

        async Task LoadProjectCore(Action connect)
        {
            _tree.Nodes.Clear();
            UpdateSelectionLabel();
            SetStatus("Povezujem ...", Color.DarkOrange);
            try
            {
                var data = await _worker.Run(() =>
                {
                    connect();
                    return _tia.FindAllHmiTargets()
                               .Select(t => Tuple.Create(t, _tia.ListItems(t)))
                               .ToList();
                });
                BuildTree(data);
                Log("Najdenih HMI targetov: " + data.Count);
                foreach (var d in data)
                    Log("  - " + d.Item1 + ": " + d.Item2.Count(i => i.Kind == HmiItemKind.Screen) + " zaslonov, " +
                        d.Item2.Count(i => i.Kind == HmiItemKind.Template) + " predlog");
                SetStatus("Povezan: " + await _worker.Run(() => _tia.ProjectName), Color.DarkGreen);
            }
            catch
            {
                SetStatus("Ni povezave.", Color.DarkRed);
                throw;
            }
        }

        bool RequireSelection(out List<HmiItem> items)
        {
            items = SelectedItems();
            if (!_tia.IsConnected) { Log("Najprej se poveži s TIA Portalom."); return false; }
            if (items.Count == 0) { Log("V drevesu označi vsaj en zaslon, mapo ali target."); return false; }
            if (string.IsNullOrWhiteSpace(_tbOutDir.Text)) { Log("Določi mapo za export."); return false; }
            return true;
        }

        async Task ExportSelected()
        {
            List<HmiItem> items;
            if (!RequireSelection(out items)) return;
            string outDir = _tbOutDir.Text.Trim();

            await RunBusy(async () =>
            {
                int okAll = 0, failAll = 0;
                foreach (var grp in items.GroupBy(i => i.Target))
                {
                    var t = grp.Key;
                    var list = grp.ToList();
                    var res = await _worker.Run(() =>
                    {
                        int ok, failed;
                        _tia.ExportItems(t, list, outDir, LogFromWorker, out ok, out failed);
                        return Tuple.Create(ok, failed);
                    });
                    okAll += res.Item1; failAll += res.Item2;
                }
                Log("EXPORT KONČAN: " + okAll + " datotek, " + failAll + " napak.");
            });
        }

        async Task ImportSelected()
        {
            List<HmiItem> items;
            if (!RequireSelection(out items)) return;
            string outDir = _tbOutDir.Text.Trim();

            int withFile = items.Count(i => File.Exists(TiaService.ItemFile(outDir, i)));
            if (withFile == 0) { Log("Za izbrane elemente ni exportiranih datotek v " + outDir + " - najprej Export."); return; }

            var r = MessageBox.Show(this,
                "Import bo PREPISAL (Override) " + withFile + " zaslonov/predlog v targetih:\n\n" +
                string.Join("\n", items.Select(i => i.Target.Name).Distinct()) +
                (withFile < items.Count ? "\n\n(" + (items.Count - withFile) + " izbranih nima exporta - preskočeni.)" : "") +
                "\n\nProjekt se NE bo shranil - po pregledu shrani ročno v TIA Portalu.\nNadaljujem?",
                "Import", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;

            await RunBusy(async () =>
            {
                int okAll = 0, failAll = 0, skipAll = 0;
                foreach (var grp in items.GroupBy(i => i.Target))
                {
                    var t = grp.Key;
                    var list = grp.ToList();
                    var res = await _worker.Run(() =>
                    {
                        int ok, failed, skipped;
                        _tia.ImportItems(t, list, outDir, LogFromWorker, out ok, out failed, out skipped);
                        return Tuple.Create(ok, failed, skipped);
                    });
                    okAll += res.Item1; failAll += res.Item2; skipAll += res.Item3;
                }
                Log("IMPORT KONČAN: " + okAll + " uvoženih, " + failAll + " napak, " + skipAll +
                    " preskočenih. Projekt NI shranjen - preglej in shrani ročno (Ctrl+S).");
            });
        }

        /// <summary>
        /// Izvoz vsega, kar Openness zna izvoziti, v &lt;mapa&gt;\&lt;projekt&gt;\ (+ export_log.txt, manifest.json).
        /// Obstoječa mapa se izbriše samo, če je prejšnji Export all (vsebuje manifest.json) - zaradi čistega git diffa.
        /// </summary>
        async Task ExportAll()
        {
            if (!_tia.IsConnected) { Log("Najprej se poveži s TIA Portalom."); return; }
            if (string.IsNullOrWhiteSpace(_tbOutDir.Text)) { Log("Določi mapo za export."); return; }
            string outDir = _tbOutDir.Text.Trim();
            string root = await _worker.Run(() => _tia.ExportAllRoot(outDir));

            List<ExportCategory> categories;
            using (var dlg = new ExportAllDialog(root))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                categories = dlg.SelectedCategories;
            }
            if (categories.Count == 0) { Log("Ni izbrane nobene kategorije."); return; }

            if (Directory.Exists(root) && Directory.EnumerateFileSystemEntries(root).Any())
            {
                if (!File.Exists(Path.Combine(root, TiaService.ManifestName)))
                {
                    Log("Mapa " + root + " obstaja in NI prejšnji Export all (ni " + TiaService.ManifestName + ") - izberi drugo mapo za export.");
                    return;
                }
                var r = MessageBox.Show(this,
                    "Mapa prejšnjega izvoza bo v celoti izbrisana in ustvarjena na novo:\n\n" + root +
                    "\n\n(Tako izbrisani objekti v TIA ne ostanejo kot stare datoteke.) Nadaljujem?",
                    "Export all", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                try { Directory.Delete(root, true); }
                catch (Exception ex) { Log("NAPAKA pri brisanju " + root + ": " + ex.Message + " (je kakšna datoteka odprta?)"); return; }
            }

            await RunBusy(async () =>
            {
                _btnCancel.Enabled = true;
                try
                {
                    await _worker.Run(() =>
                    {
                        int exported, skipped, failed;
                        _tia.ExportAll(outDir, categories, LogFromWorker, out exported, out skipped, out failed);
                    });
                }
                finally { _btnCancel.Enabled = false; }
            });
        }

        // ================================================================== Barve (brez TIA)

        /// <summary>
        /// Datoteke za analizo/zamenjavo: exportirane datoteke IZBRANIH elementov v drevesu.
        /// Brez povezave (prazno drevo) se vzamejo vse XML datoteke v izhodni mapi.
        /// </summary>
        List<string> WorkFiles(out string baseDir)
        {
            baseDir = _tbOutDir.Text.Trim();
            if (!Directory.Exists(baseDir)) { Log("Mapa ne obstaja: " + baseDir); return new List<string>(); }

            if (_tree.Nodes.Count == 0)
            {
                var all = ColorReplacer.GetXmlFiles(baseDir).ToList();
                Log("Ni povezave s TIA - obdelam vse XML datoteke v " + baseDir + " (" + all.Count + ").");
                return all;
            }

            var items = SelectedItems();
            if (items.Count == 0) { Log("V drevesu označi elemente, ki jih želiš obdelati."); return new List<string>(); }
            var dir = baseDir;
            var files = items.Select(i => TiaService.ItemFile(dir, i)).Where(File.Exists).ToList();
            if (files.Count < items.Count)
                Log("Opozorilo: " + (items.Count - files.Count) + " izbranih elementov nima exporta (preskočeni).");
            return files;
        }

        async Task Analyze()
        {
            string baseDir;
            var files = WorkFiles(out baseDir);
            if (files.Count == 0) return;
            await RunBusy(async () =>
            {
                var used = await Task.Run(() => ColorReplacer.Analyze(files, LogFromWorker));
                _gridUsed.Rows.Clear();
                foreach (var u in used)
                {
                    int i = _gridUsed.Rows.Add("", u.Color.ToString(), u.Role, u.ActiveCount, u.InactiveCount, u.Files.Count);
                    var sw = _gridUsed.Rows[i].Cells[0];
                    sw.Style.BackColor = sw.Style.SelectionBackColor = u.Color.ToColor();
                    if (u.ActiveCount == 0) _gridUsed.Rows[i].DefaultCellStyle.ForeColor = Color.Gray;
                }
                Log("Analiza: " + used.Select(u => u.Color).Distinct().Count() + " različnih barv, " + used.Count + " kombinacij barva/vloga. " +
                    "Sivo = samo neaktivne uporabe (Transparent ozadje / obroba 0).");
            });
        }

        async Task ReplaceColors(bool dryRun)
        {
            Dictionary<RgbColor, RgbColor> map;
            List<string> warnings;
            var errors = ColorMapping.Validate(CurrentPairs(), out map, out warnings);
            foreach (var w in warnings) Log("Opozorilo: " + w);
            if (errors.Count > 0) { foreach (var e in errors) Log("NAPAKA: " + e); return; }
            if (map.Count == 0) { Log("Mapiranje je prazno."); return; }

            string baseDir;
            var files = WorkFiles(out baseDir);
            if (files.Count == 0) return;

            if (!dryRun)
            {
                var r = MessageBox.Show(this,
                    "Barve bodo zamenjane v " + files.Count + " XML datotekah v:\n" + baseDir +
                    "\n\nSpremenjene datoteke se prej kopirajo v <mapa>_backup_<čas>. Nadaljujem?",
                    "Zamenjava barv", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                TrySaveMapping(ColorMapping.DefaultFile, quiet: true);
            }

            await RunBusy(async () =>
            {
                await Task.Run(() => ColorReplacer.Replace(baseDir, files, map, dryRun, LogFromWorker));
            });
        }

        // ================================================================== Analiza exporta

        /// <summary>
        /// Statična analiza izvoza "Export all" (TIA_ToolKit.Analyzer). Ne potrebuje povezave s TIA.
        /// Poročilo gre v NADREJENO mapo projekta, ker Export all ob ponovnem izvozu svojo mapo v celoti izbriše.
        /// </summary>
        async Task AnalyzeExport()
        {
            string dir;
            using (var d = new FolderBrowserDialog
            {
                Description = "Izberi mapo exporta (projekt z manifest.json ali nadrejeno mapo z več projekti)",
                SelectedPath = _tbOutDir.Text, ShowNewFolderButton = false
            })
            {
                if (d.ShowDialog(this) != DialogResult.OK) return;
                dir = d.SelectedPath;
            }
            var projects = Analyzer.ExportReader.FindProjects(dir);
            if (projects.Count == 0) { Log("V mapi " + dir + " ni exporta (manifest.json ali PLC\\)."); return; }

            await RunBusy(async () =>
            {
                foreach (var p in projects)
                {
                    Log("Analiza: " + p);
                    string report = null;
                    await Task.Run(() =>
                    {
                        var res = Analyzer.ExportAnalyzer.Analyze(p, LogFromWorker);
                        report = Path.Combine(Path.GetDirectoryName(p.TrimEnd('\\', '/')), "ANALIZA_TIA_ToolKit_" + res.Project.Name + ".xlsx");
                        int kept = Analyzer.ExcelReport.Write(res, report);
                        LogFromWorker("Ugotovitev: " + res.Findings.Count + " – " +
                            string.Join(", ", res.Findings.GroupBy(f => f.Category).Select(g => g.Key + " " + g.Count())) +
                            (kept > 0 ? "  (ohranjenih vnosov Status/Opomba: " + kept + ")" : ""));
                    });
                    Log("Poročilo: " + report);
                    if (projects.Count == 1 && report != null && File.Exists(report))
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(report) { UseShellExecute = true });
                }
            });
        }

        // ================================================================== Pomožno

        async Task RunBusy(Func<Task> work)
        {
            SetBusy(true);
            try { await work(); }
            catch (Exception ex)
            {
                Log("=== NAPAKA === " + ex.GetType().Name + ": " + ex.Message);
                if (ex.InnerException != null) Log("  Inner: " + ex.InnerException.Message);
            }
            finally { SetBusy(false); }
        }

        void SetBusy(bool busy)
        {
            foreach (var b in new Control[] { _btnRefresh, _btnConnect, _btnOpenProtected,_btnExport, _btnDryRun, _btnReplace, _btnImport, _btnAnalyze, _btnExportAll, _btnAnalyzeExport, _cbProcess, _tree })
                b.Enabled = !busy;
            UseWaitCursor = busy;
        }

        void SetStatus(string text, Color color)
        {
            _lblStatus.Text = text;
            _lblStatus.ForeColor = color;
        }

        void BrowseOutDir()
        {
            using (var d = new FolderBrowserDialog { SelectedPath = _tbOutDir.Text, ShowNewFolderButton = true })
                if (d.ShowDialog(this) == DialogResult.OK) _tbOutDir.Text = d.SelectedPath;
        }

        void LogFromWorker(string msg)
        {
            if (IsDisposed) return;
            BeginInvoke(new Action(() => Log(msg)));
        }

        void Log(string msg)
        {
            // večvrstična sporočila (npr. napake z naštetimi vzroki) zamaknjena pod časom
            string line = DateTime.Now.ToString("HH:mm:ss") + "  " +
                          (msg ?? "").Replace("\r\n", "\n").Replace("\n", Environment.NewLine + "          ");
            _tbLog.AppendText(line + Environment.NewLine);
            try { File.AppendAllText(_logFile, line + Environment.NewLine); } catch { }
        }
    }
}
