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
            if (args.Length >= 3 && args[0] == "dump") return Dump(args);
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

        /// <summary>dump &lt;exportDir&gt; &lt;blok&gt; [network]: izpiše, kako je analizator prebral networke bloka (za razvoj pravil).</summary>
        static int Dump(string[] args)
        {
            var p = ExportReader.Read(ExportReader.FindProjects(Path.GetFullPath(args[1])).First(), s => { });
            int only = args.Length > 3 ? int.Parse(args[3]) : 0;
            foreach (var b in p.Plcs.SelectMany(x => x.Blocks).Where(x => string.Equals(x.Name, args[2], StringComparison.OrdinalIgnoreCase)))
                foreach (var nw in b.Networks.Where(n => only == 0 || n.Index == only))
                {
                    Console.WriteLine("--- " + b.DisplayPath + " NW" + nw.Index + " [" + nw.Language + "] " + nw.Title);
                    foreach (var s in nw.Stl) Console.WriteLine("  " + s);
                    if (nw.Lad != null)
                    {
                        foreach (var part in nw.Lad.Parts.Values)
                            Console.WriteLine("  part " + part.Uid + " " + part.Name + (part.Negated ? " NEG" : "") + " " + part.Operand);
                        foreach (var w in nw.Lad.Wires)
                            Console.WriteLine("  wire " + (w.Source.Powerrail ? "PWR" : w.Source.Uid + "." + w.Source.Pin) + " -> " +
                                              string.Join(", ", w.Targets.Select(t => t.Uid + "." + t.Pin)));
                    }
                    if (nw.Scl != null) Console.WriteLine(nw.Scl);
                }
            return 0;
        }
    }
}
