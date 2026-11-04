using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TIA_Generator.Gui
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // TIA_GeneratorGui.exe [<model.hmimodel.json>] [--unit <oznaka>] [--tab <n>] [--snapshot <png>]
            // --snapshot je za razvoj: izriše okno v PNG in zapre program.
            string model = null, unit = null, snap = null, export = null; int tab = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--export" && i + 1 < args.Length) export = args[++i];
                else if (args[i] == "--unit" && i + 1 < args.Length) unit = args[++i];
                else if (args[i] == "--tab" && i + 1 < args.Length) int.TryParse(args[++i], out tab);
                else if (args[i] == "--snapshot" && i + 1 < args.Length) snap = args[++i];
                else if (File.Exists(args[i])) model = args[i];
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
