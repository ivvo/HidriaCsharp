using System;
using System.IO;
using System.Linq;
using TIA_ToolKit.Analyzer;

namespace TIA_ToolKit.Cli
{
    /// <summary>
    /// TIA_ToolKit.Cli analyze &lt;exportDir&gt; [-o &lt;poročilo.xlsx&gt;]
    /// exportDir je mapa enega projekta (z manifest.json) ali nadrejena mapa z več projekti (vsak dobi svoje poročilo).
    /// </summary>
    static class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (args.Length < 2 || args[0] != "analyze")
            {
                Console.WriteLine("Uporaba: TIA_ToolKit.Cli analyze <exportDir> [-o <porocilo.xlsx>]");
                return 1;
            }
            string dir = Path.GetFullPath(args[1]);
            string outFile = null;
            for (int i = 2; i + 1 < args.Length; i++) if (args[i] == "-o") outFile = args[i + 1];
            if (!Directory.Exists(dir)) { Console.WriteLine("Mapa ne obstaja: " + dir); return 1; }

            var projects = ExportReader.FindProjects(dir);
            if (projects.Count == 0) { Console.WriteLine("V mapi ni exporta (manifest.json ali PLC\\)."); return 1; }
            foreach (var p in projects)
            {
                Console.WriteLine("== " + p);
                var res = ExportAnalyzer.Analyze(p, Console.WriteLine);
                string target = outFile != null && projects.Count == 1 ? outFile
                    // v nadrejeno mapo - Export all ob ponovnem izvozu svojo mapo v celoti izbriše
                    : Path.Combine(Path.GetDirectoryName(p.TrimEnd('\\', '/')), "ANALIZA_TIA_ToolKit_" + res.Project.Name + ".xlsx");
                int kept = ExcelReport.Write(res, target);
                Console.WriteLine("Ugotovitev: " + res.Findings.Count + "  (" +
                    string.Join(", ", res.Findings.GroupBy(f => f.Category).Select(g => g.Key + " " + g.Count())) + ")");
                Console.WriteLine("Poročilo: " + target + (kept > 0 ? "  (ohranjenih vnosov: " + kept + ")" : ""));
            }
            return 0;
        }
    }
}
