using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TIA_Generator.Gui
{
    static class Program
    {
        const string OpennessDir = @"C:\Program Files\Siemens\Automation\Portal V20\PublicAPI\V20";

        [STAThread]
        static void Main(string[] args)
        {
            // Openness DLL-ji niso v bin – naložimo jih iz namestitve TIA, preden se dotaknemo katerega koli Siemens tipa.
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                string name = new System.Reflection.AssemblyName(e.Name).Name;
                if (!name.StartsWith("Siemens.Engineering", StringComparison.OrdinalIgnoreCase)) return null;
                string path = Path.Combine(OpennessDir, name + ".dll");
                return File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
            };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // TIA_GeneratorGui.exe [<model.hmimodel.json>] [--unit <oznaka>] [--tab <n>] [--snapshot <png>]
            // --snapshot je za razvoj: izriše okno v PNG in zapre program.
            string model = null, unit = null, snap = null, export = null, snapDialog = null; int tab = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--snapdialog" && i + 1 < args.Length) snapDialog = args[++i];
                else if (args[i] == "--export" && i + 1 < args.Length) export = args[++i];
                else if (args[i] == "--unit" && i + 1 < args.Length) unit = args[++i];
                else if (args[i] == "--tab" && i + 1 < args.Length) int.TryParse(args[++i], out tab);
                else if (args[i] == "--snapshot" && i + 1 < args.Length) snap = args[++i];
                else if (File.Exists(args[i])) model = args[i];
            }
            int si = Array.IndexOf(args, "--snapimport");
            if (si >= 0 && si + 1 < args.Length)
            {
                // razvoj: dialog Uvozi v TIA (naloži Openness, prebere procese) izrisan v PNG
                using (var dlg = new ImportDialog(@"C:\temp", "DB_Error_Messages"))
                {
                    dlg.Shown += async (s, e) =>
                    {
                        await System.Threading.Tasks.Task.Delay(4000);
                        using (var bmp = new System.Drawing.Bitmap(dlg.Width, dlg.Height))
                        {
                            dlg.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, dlg.Width, dlg.Height));
                            bmp.Save(args[si + 1], System.Drawing.Imaging.ImageFormat.Png);
                        }
                        dlg.Close();
                    };
                    Application.Run(dlg);
                }
                return;
            }
            if (snapDialog != null)
            {
                // razvoj: dialog Struktura (iz modela ali nov stroj 3×5) izrisan v PNG
                var d = model != null ? TIA_Generator.Structure.StructureBuilder.FromModel(TIA_Generator.Model.ModelJson.Load(model))
                                      : TIA_Generator.Structure.StructureBuilder.Create("Nov stroj", 3, 5);
                using (var dlg = new StructureDialog(d, "Struktura stroja"))
                {
                    dlg.Shown += (s, e) => dlg.BeginInvoke((Action)(() =>
                    {
                        Application.DoEvents();
                        using (var bmp = new System.Drawing.Bitmap(dlg.Width, dlg.Height))
                        {
                            dlg.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, dlg.Width, dlg.Height));
                            bmp.Save(snapDialog, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        dlg.Close();
                    }));
                    Application.Run(dlg);
                }
                return;
            }
            var form = new MainForm();
            form.Shown += async (s, e) =>
            {
                if (export != null) await form.ExtractFromExport(export);
                else if (model != null) form.OpenModelFile(model);
                if (unit != null) form.SelectUnit(unit, tab);
                if (snap != null) form.BeginInvoke((Action)(() => { form.Snapshot(snap); form.CloseWithoutPrompt(); }));
            };
            Application.Run(form);
        }
    }

    /// <summary>Majhne uporabniške nastavitve (zadnji export/model) v %AppData%\TIA_Generator\settings.txt (ključ=vrednost).</summary>
    static class Settings
    {
        static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TIA_Generator", "settings.txt");

        static Dictionary<string, string> Read()
        {
            try
            {
                if (!File.Exists(FilePath)) return new Dictionary<string, string>();
                return File.ReadAllLines(FilePath).Where(l => l.Contains('=')).Select(l => l.Split(new[] { '=' }, 2))
                           .GroupBy(p => p[0]).ToDictionary(g => g.Key, g => g.Last()[1]);
            }
            catch { return new Dictionary<string, string>(); }
        }

        public static string Get(string key) => Read().TryGetValue(key, out var v) ? v : null;

        public static void Set(string key, string value)
        {
            try
            {
                var d = Read();
                d[key] = value;
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllLines(FilePath, d.Select(kv => kv.Key + "=" + kv.Value));
            }
            catch { /* nastavitve niso kritične */ }
        }
    }
}
