using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TIA_Generator.Extract;
using TIA_Generator.Model;

namespace TIA_Generator.Cli
{
    /// <summary>
    /// TIA_Generator – generator HMI po HMI Standardu 2.0.
    ///   model &lt;exportDir&gt; [-o &lt;x.hmimodel.json&gt;] [-s &lt;slovar.json&gt;]...   faza 1: model stroja iz exporta "Export all"
    ///   show  &lt;x.hmimodel.json&gt; [enota]                                    pregled modela (enote ali ena enota)
    /// </summary>
    static class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help") { Usage(); return args.Length == 0 ? 1 : 0; }
            try
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "model": return CmdModel(args.Skip(1).ToList());
                    case "show": return CmdShow(args.Skip(1).ToList());
                    case "plcchanges": return CmdPlcChanges(args.Skip(1).ToList());
                    case "newmachine": return CmdNewMachine(args.Skip(1).ToList());
                    case "plcskeleton": return CmdPlcSkeleton(args.Skip(1).ToList());
                    default: Console.Error.WriteLine("Neznan ukaz: " + args[0]); Usage(); return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("NAPAKA: " + ex.Message);
                return 2;
            }
        }

        static void Usage()
        {
            Console.WriteLine("TIA_Generator – generator HMI (HMI Standard 2.0)");
            Console.WriteLine();
            Console.WriteLine("  TIA_Generator model <exportDir> [-o <x.hmimodel.json>] [-s <slovar.json>]...");
            Console.WriteLine("      Iz exporta 'Export all' izdela model stroja (enote, indeksi, časovniki, parametri, cilindri, alarmi).");
            Console.WriteLine("      Privzeti izhod: <nadrejena mapa exporta>\\<projekt>.hmimodel.json (Export all briše svojo mapo).");
            Console.WriteLine("      -s: dodatni slovar besedil (dopolni vgrajenega).");
            Console.WriteLine("  TIA_Generator show <x.hmimodel.json> [<enota>]");
            Console.WriteLine("      Pregled modela: tabela enot ali podrobnosti ene enote.");
            Console.WriteLine("  TIA_Generator plcchanges <x.hmimodel.json> [-o <spremembe.csv>]");
            Console.WriteLine("      Potrebne spremembe PLC za standard 2.0 (časovniki → Delay[0..9]) z bloki, kjer so člani uporabljeni.");
        }

        static string Opt(List<string> a, string name, string def = null)
        {
            int i = a.IndexOf(name);
            return i >= 0 && i + 1 < a.Count ? a[i + 1] : def;
        }

        /// <summary>newmachine &lt;ime&gt; --sectors N (0 = en sektor) --stations M [--sub A01=2,B03=1] [--pc 0|1] [--panel 0|1] -o model.json</summary>
        static int CmdNewMachine(List<string> a)
        {
            if (a.Count == 0 || a[0].StartsWith("-")) throw new ArgumentException("Manjka ime stroja.");
            int sectors = int.Parse(Opt(a, "--sectors", "1")), stations = int.Parse(Opt(a, "--stations", "5"));
            var d = TIA_Generator.Structure.StructureBuilder.Create(a[0], sectors, stations);
            d.Pc = Opt(a, "--pc", "1") != "0"; d.Panel = Opt(a, "--panel", "1") != "0";
            foreach (var kv in (Opt(a, "--sub", "") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var p = kv.Split('=');
                var st = d.Stations.FirstOrDefault(s => s.Id == p[0]) ?? throw new ArgumentException("Ni postaje " + p[0]);
                st.SubCount = int.Parse(p[1]);
            }
            TIA_Generator.Structure.StructureBuilder.Rebuild(d);
            var m = TIA_Generator.Structure.StructureBuilder.ApplyToModel(d, null, TextDictionary.LoadDefault());
            string o = Opt(a, "-o", Safe(a[0]) + ".hmimodel.json");
            ModelJson.Save(m, o);
            Console.WriteLine("Model: " + Path.GetFullPath(o));
            foreach (var s in d.Stations)
                Console.WriteLine(string.Format("  {0,-8} {1,-3} PLC {2,3}  HMI {3,3}  {4}", s.Id, s.Sector, s.PlcIndex, s.HmiIndex, s.IsSub ? "podpostaja " + s.Parent : ""));
            foreach (var w in TIA_Generator.Structure.StructureBuilder.Validate(d)) Console.WriteLine("  ⚠ " + w);
            return 0;
        }

        /// <summary>plcskeleton &lt;model&gt; --template &lt;exportDir&gt; [--unit A05] -o &lt;outDir&gt;</summary>
        static int CmdPlcSkeleton(List<string> a)
        {
            if (a.Count == 0) throw new ArgumentException("Manjka pot do .hmimodel.json.");
            var m = ModelJson.Load(a[0]);
            var tpl = new TIA_Generator.Generate.PlcSkeleton.Template
            {
                ExportDir = Opt(a, "--template", @"D:\Claude\TiaFullExport\Purem support pin\Project1_V20"),
                UnitId = Opt(a, "--unit", "A05"),
            };
            string o = Opt(a, "-o") ?? throw new ArgumentException("Manjka -o <izhodna mapa>.");
            var r = new TIA_Generator.Generate.PlcSkeleton(Console.WriteLine).Generate(m, tpl, o);
            foreach (var l in r.Report) Console.WriteLine(l);
            return 0;
        }

        static int CmdPlcChanges(List<string> a)
        {
            if (a.Count == 0) throw new ArgumentException("Manjka pot do .hmimodel.json.");
            var m = ModelJson.Load(a[0]);
            string csv = null;
            for (int i = 1; i < a.Count; i++) if (a[i] == "-o" && i + 1 < a.Count) csv = a[++i];

            var rows = new List<string> { "Enota;DB;Stari član;Novi element;Prikaz na HMI;Uporabljen v (blok, networki)" };
            int units = 0, members = 0, uses = 0;
            foreach (var u in m.Units.Where(x => x.HmiTags?.Timers?.RequiresPlcChange == true))
            {
                units++;
                Console.WriteLine();
                Console.WriteLine(u.Id + "  " + u.SettingsDb?.Name + " [DB" + u.SettingsDb?.Number + "]  →  " + Model.TimerArrayNameOf(u) + " : Array[0..9] of S5Time");
                foreach (var t in u.Timers)
                {
                    members++;
                    uses += t.UsedIn?.Count ?? 0;
                    Console.WriteLine(string.Format("   {0,-28} → {1,-9} {2}", t.Member, t.ArrayElement, t.UsedIn == null || t.UsedIn.Count == 0 ? "(ni uporabljen)" : string.Join("; ", t.UsedIn)));
                    rows.Add(string.Join(";", u.Id, u.SettingsDb?.Name, t.Member, t.ArrayElement, t.Show ? "da" : "ne",
                                         t.UsedIn == null ? "" : string.Join(" | ", t.UsedIn)));
                }
            }
            Console.WriteLine();
            Console.WriteLine("Skupaj: " + units + " DB-jev, " + members + " članov, " + uses + " mest uporabe v blokih.");
            if (units > 0) Console.WriteLine("Po spremembi v TIA: Export all → ponovno 'model' (opozorilo TIMERS-NOT-ARRAY mora izginiti).");
            if (csv != null)
            {
                File.WriteAllLines(csv, TIA_Generator.Generate.PlcChanges.CsvLines(m), new UTF8Encoding(true));
                Console.WriteLine("CSV: " + Path.GetFullPath(csv));
            }
            return 0;
        }

        static class Model
        {
            public static string TimerArrayNameOf(TIA_Generator.Model.Unit u)
            {
                var p = u.HmiTags?.Timers?.PlcTag ?? "";
                int dot = p.LastIndexOf('.');
                return dot >= 0 ? p.Substring(dot + 1) : ModelExtractor.TimerArrayName;
            }
        }

        static int CmdModel(List<string> a)
        {
            string export = null, output = null;
            var dicts = new List<string>();
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] == "-o" && i + 1 < a.Count) output = a[++i];
                else if (a[i] == "-s" && i + 1 < a.Count) dicts.Add(a[++i]);
                else if (export == null) export = a[i];
                else throw new ArgumentException("Odvečen argument: " + a[i]);
            }
            if (export == null) throw new ArgumentException("Manjka mapa exporta.");
            export = Path.GetFullPath(export.TrimEnd('\\', '/'));
            if (!Directory.Exists(export)) throw new DirectoryNotFoundException(export);

            var dict = TextDictionary.LoadDefault(dicts);
            var m = new ModelExtractor(dict, s => Console.WriteLine(s)).Extract(export);
            if (output == null)
            {
                string parent = Path.GetDirectoryName(m.SourceExport) ?? export;
                output = Path.Combine(parent, Safe(m.Machine.Name) + ".hmimodel.json");
            }
            ModelJson.Save(m, output);

            var s2 = m.Statistics;
            Console.WriteLine();
            Console.WriteLine("Model: " + output);
            Console.WriteLine("  stroj " + m.Machine.Name + " (" + m.Machine.Type + (m.Machine.Parts.Count > 0 ? ", deli " + string.Join(",", m.Machine.Parts) : "") +
                              "), PLC " + m.Machine.Plc.Name + ", HMI: " + string.Join(", ", m.Machine.HmiDevices.Select(d => d.Kind + " " + d.Target)));
            Console.WriteLine("  DB: " + string.Join(", ", m.Machine.DbMap.Names.Select(kv => kv.Key + "=" + kv.Value)));
            Console.WriteLine("  enot " + s2.Units + ", časovnikov " + s2.Timers + " (prikazanih " + s2.TimersShown + "), parametrov " + s2.Parameters +
                              ", recept " + s2.RecipeParameters + ", cilindrov " + s2.Cylinders + ", alarmov " + s2.Alarms);
            Console.WriteLine("  besedila: slovar " + s2.TextsFromDictionary + ", sestavljena " + s2.TextsComposed + ", manjka " + s2.TextsMissing);
            Console.WriteLine("  opozoril " + m.Warnings.Count + (m.Warnings.Count > 0 ? ":" : ""));
            foreach (var g in m.Warnings.GroupBy(w => w.Code))
            {
                Console.WriteLine("    " + g.Key + " ×" + g.Count());
                foreach (var w in g.Take(4)) Console.WriteLine("      " + w);
                if (g.Count() > 4) Console.WriteLine("      …");
            }
            return 0;
        }

        static int CmdShow(List<string> a)
        {
            if (a.Count == 0) throw new ArgumentException("Manjka pot do .hmimodel.json.");
            var m = ModelJson.Load(a[0]);
            if (a.Count == 1)
            {
                Console.WriteLine(string.Format("{0,-10} {1,-3} {2,5} {3,5} {4,-8} {5,7} {6,6} {7,6} {8,6} {9,6}  {10}",
                    "Enota", "Del", "PLC", "HMI", "Settings", "Čas.", "Param", "Recept", "Cil.", "Alarm", "Opis"));
                foreach (var u in m.Units)
                    Console.WriteLine(string.Format("{0,-10} {1,-3} {2,5} {3,5} {4,-8} {5,7} {6,6} {7,6} {8,6} {9,6}  {10}",
                        u.Id, u.Part ?? (u.Group != null ? u.Group.Substring(0, Math.Min(3, u.Group.Length)) : ""), u.PlcIndex?.ToString() ?? "?", u.HmiIndex?.ToString() ?? "?",
                        u.SettingsDb != null ? "DB" + u.SettingsDb.Number : "-",
                        u.Timers.Count(t => t.Show) + "/" + u.Timers.Count, u.Parameters.Count, u.RecipeParameters.Count,
                        u.Cylinders.Count, u.Alarms.Count, u.Text.TryGetValue("sl-SI", out var d) ? d : ""));
                return 0;
            }
            var unit = m.Units.FirstOrDefault(x => string.Equals(x.Id, a[1], StringComparison.OrdinalIgnoreCase));
            if (unit == null) throw new ArgumentException("Enote " + a[1] + " ni v modelu.");
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(unit, Newtonsoft.Json.Formatting.Indented,
                new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore }));
            return 0;
        }

        static string Safe(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }
    }
}
