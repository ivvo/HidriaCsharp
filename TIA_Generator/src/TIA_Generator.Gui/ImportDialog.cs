using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TIA_Generator.Openness;

namespace TIA_Generator.Gui
{
    /// <summary>
    /// Uvoz PLC ogrodja v odprt TIA Portal V20: izbira procesa in PLC-ja → Preveri (načrt, nič se ne spremeni) → Uvozi.
    /// Projekta ne shrani.
    /// </summary>
    sealed class ImportDialog : Form
    {
        readonly TiaImporter _tia = new TiaImporter();
        readonly ComboBox _procs, _plcs;
        readonly TextBox _dir, _out;
        readonly CheckBox _alarms, _overwrite;
        readonly Button _connect, _plan, _import;
        readonly string _alarmDb;
        ImportPlan _currentPlan;

        public ImportDialog(string skeletonDir, string alarmDb)
        {
            _alarmDb = alarmDb ?? "DB_Error_Messages";
            Text = "Uvoz PLC ogrodja v TIA Portal V20 (Openness)";
            Font = new Font("Segoe UI", 9f);
            Size = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;

            var top = new TableLayoutPanel { Dock = DockStyle.Top, Height = 150, ColumnCount = 4, Padding = new Padding(10) };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            _procs = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            var refresh = new Button { Text = "Osveži", Dock = DockStyle.Fill };
            _connect = new Button { Text = "Priklopi", Dock = DockStyle.Fill };
            _plcs = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Enabled = false };
            _dir = new TextBox { Dock = DockStyle.Fill, Text = skeletonDir ?? "" };
            var browse = new Button { Text = "…", Dock = DockStyle.Fill };
            _alarms = new CheckBox { Text = "Manjkajoče bite (Warning_OFF_<oznaka> …) dodaj na konec " + _alarmDb, Checked = true, AutoSize = true };
            _overwrite = new CheckBox { Text = "Obstoječe bloke z istim imenom prepiši (sicer se preskočijo)", AutoSize = true };
            void Row(int r, string label, Control c, Control b1 = null, Control b2 = null)
            {
                top.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, r);
                top.Controls.Add(c, 1, r);
                if (b1 != null) top.Controls.Add(b1, 2, r);
                if (b2 != null) top.Controls.Add(b2, 3, r);
            }
            Row(0, "TIA Portal", _procs, refresh, _connect);
            Row(1, "PLC", _plcs);
            Row(2, "Mapa PLC ogrodja", _dir, browse);
            var opts = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            opts.Controls.Add(_alarms); opts.Controls.Add(_overwrite);
            top.Controls.Add(opts, 1, 3); top.SetColumnSpan(opts, 3);

            _out = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9f), BackColor = SystemColors.Window };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 6, 10, 0) };
            var close = new Button { Text = "Zapri", Width = 90, Height = 30, DialogResult = DialogResult.Cancel };
            _import = new Button { Text = "Uvozi", Width = 110, Height = 30, Enabled = false };
            _plan = new Button { Text = "Preveri (brez sprememb)", Width = 170, Height = 30, Enabled = false };
            buttons.Controls.Add(close); buttons.Controls.Add(_import); buttons.Controls.Add(_plan);
            CancelButton = close;

            Controls.Add(_out); Controls.Add(buttons); Controls.Add(top);

            refresh.Click += async (s, e) => await Guard(RefreshProcesses);
            _connect.Click += async (s, e) => await Guard(Connect);
            browse.Click += (s, e) => { using (var d = new FolderBrowserDialog { SelectedPath = _dir.Text }) if (d.ShowDialog(this) == DialogResult.OK) _dir.Text = d.SelectedPath; };
            _plan.Click += async (s, e) => await Guard(MakePlan);
            _import.Click += async (s, e) => await Guard(DoImport);
            _plcs.SelectedIndexChanged += (s, e) => { _plan.Enabled = _plcs.SelectedItem != null; _import.Enabled = false; };
            _overwrite.CheckedChanged += (s, e) => _import.Enabled = false;
            FormClosed += (s, e) => _tia.Dispose();
            Shown += async (s, e) =>
            {
                Log("1) V TIA Portal V20 odpri projekt (kopijo osnovnega projekta).  2) Priklopi.  3) Preveri.  4) Uvozi.  Projekt se ne shrani.");
                Log("   Uporabnik mora biti v skupini Windows 'Siemens TIA Openness'.");
                await Guard(RefreshProcesses);
            };
        }

        void Log(string s)
        {
            if (InvokeRequired) { BeginInvoke((Action)(() => Log(s))); return; }
            _out.AppendText(s + Environment.NewLine);
        }

        async Task Guard(Func<Task> f)
        {
            UseWaitCursor = true; Enabled = false;
            try { await f(); }
            catch (Exception ex)
            {
                var msg = ex is AggregateException ae ? ae.InnerException?.Message : ex.Message;
                if (ex is System.IO.FileNotFoundException || (msg ?? "").Contains("Siemens.Engineering"))
                    msg += "\n\nOpenness ni na voljo: TIA Portal V20 z Openness mora biti nameščen.";
                else if ((msg ?? "").IndexOf("access", StringComparison.OrdinalIgnoreCase) >= 0 || (msg ?? "").Contains("Openness"))
                    msg += "\n\nPreveri, da je uporabnik v skupini 'Siemens TIA Openness' (odjava/prijava po dodajanju).";
                Log("NAPAKA: " + msg);
                MessageBox.Show(this, msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Enabled = true; UseWaitCursor = false; }
        }

        async Task RefreshProcesses()
        {
            var list = await _tia.ListProcesses();
            _procs.Items.Clear();
            foreach (var p in list) _procs.Items.Add(p);
            if (_procs.Items.Count > 0) _procs.SelectedIndex = 0;
            Log(list.Count == 0 ? "Ni odprtega TIA Portala V20." : "TIA Portal procesi: " + string.Join(", ", list));
        }

        async Task Connect()
        {
            if (!(_procs.SelectedItem is TiaProcess p)) { Log("Izberi TIA Portal proces."); return; }
            var plcs = await _tia.Connect(p.Id, Log);
            _plcs.Items.Clear();
            foreach (var n in plcs) _plcs.Items.Add(n);
            _plcs.Enabled = plcs.Count > 0;
            if (plcs.Count > 0) _plcs.SelectedIndex = 0;
            Log(plcs.Count == 0 ? "V projektu ni PLC-ja." : "PLC: " + string.Join(", ", plcs));
        }

        async Task MakePlan()
        {
            if (!Directory.Exists(Path.Combine(_dir.Text, "PLC"))) { Log("Mapa nima podmape PLC – izberi izhodno mapo 'PLC ogrodje …'."); return; }
            Settings.Set("importDir", _dir.Text);
            _currentPlan = await _tia.Plan((string)_plcs.SelectedItem, _dir.Text, _alarms.Checked ? _alarmDb : null, _overwrite.Checked);
            Log("");
            Log("=== NAČRT UVOZA v " + _currentPlan.Plc + " (nič še ni spremenjeno) ===");
            foreach (var b in _currentPlan.Blocks) Log("  " + b);
            if (_currentPlan.AlarmBitsToAdd.Count > 0) Log("  " + _currentPlan.AlarmDb + ": dodati " + _currentPlan.AlarmBitsToAdd.Count + " bitov: " + string.Join(", ", _currentPlan.AlarmBitsToAdd));
            foreach (var n in _currentPlan.Notes) Log("  opomba: " + n);
            int n2 = _currentPlan.Blocks.Count(b => b.Do), conflicts = _currentPlan.Blocks.Count(b => b.Action.StartsWith("KONFLIKT"));
            Log("Za uvoz: " + n2 + " blokov" + (conflicts > 0 ? ", konfliktov številk: " + conflicts + " (spremeni PLC indeks v strukturi ali številko v projektu)" : "") + ".");
            _import.Enabled = n2 > 0 || (_alarms.Checked && _currentPlan.AlarmBitsToAdd.Count > 0);
        }

        async Task DoImport()
        {
            if (_currentPlan == null) return;
            int n = _currentPlan.Blocks.Count(b => b.Do);
            if (MessageBox.Show(this, "Uvozim " + n + " blokov v " + _currentPlan.Plc +
                                (_alarms.Checked && _currentPlan.AlarmBitsToAdd.Count > 0 ? " in dodam " + _currentPlan.AlarmBitsToAdd.Count + " bitov v " + _currentPlan.AlarmDb : "") +
                                "?\n\nProjekt se ne shrani – po uvozu ga v TIA prevedi, preglej in shrani.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Log("");
            Log("=== UVOZ ===");
            await _tia.Execute(_currentPlan, _alarms.Checked, Log);
            _import.Enabled = false;
        }
    }
}
