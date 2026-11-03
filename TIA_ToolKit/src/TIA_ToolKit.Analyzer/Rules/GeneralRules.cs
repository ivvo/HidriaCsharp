using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// PLC-DOUBLE: globalni bit je zapisan z "=" (Assign) v več networkih - zadnji zapis prepiše prejšnje.
    /// Primeri: warning_Robot_Calibratin_ON v 6 FB-jih, DB_Service_Settings.SERVICE_ON 3× v Safety.
    /// </summary>
    public sealed class DoubleAssignRule : IRule
    {
        public string Id => "PLC-DOUBLE";
        public string Title => "Globalni bit zapisan v več networkih (dvojni zapis)";

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            var writers = new Dictionary<string, List<(Block b, Network n)>>();
            foreach (var b in plc.Blocks.Where(x => !x.IsDb))
                foreach (var nw in b.Networks)
                {
                    foreach (var s in nw.Stl.Where(s => s.Op == "Assign" && s.Operand != null && s.Operand.StartsWith("\"")))
                        Add(writers, s.Operand, b, nw);
                    if (nw.Lad != null)
                        foreach (var p in nw.Lad.Parts.Values.Where(p => p.Name == "Coil" && p.Operand != null && p.Operand.StartsWith("\"")))
                            Add(writers, p.Operand, b, nw);
                }
            foreach (var kv in writers.Where(k => k.Value.Count > 1).OrderBy(k => k.Key))
            {
                var blocks = kv.Value.Select(x => x.b).Distinct().ToList();
                var f = new Finding
                {
                    RuleId = Id, Severity = Severity.Medium, Confidence = "Preveriti",
                    Category = blocks.Count > 1 ? Categories.CopyIndex : Categories.Functional,
                    Title = "Dvojni zapis: " + kv.Key,
                    Description = kv.Key + " je zapisan z \"=\" " + kv.Value.Count + "× (" + string.Join(", ", blocks.Select(x => x.Name)) +
                                  "). Velja zadnji zapis v ciklu, prejšnji se prepišejo. Preveri, ali gre za namerno prioritetno shemo (JCN) ali za napako kopiranja (pravilno: OR vseh pogojev ali en bit na enoto).",
                };
                foreach (var g in kv.Value.GroupBy(x => x.b))
                    f.Locations.Add(FindingLocation.Of(g.Key, g.Select(x => x.n).ToArray()));
                yield return f;
            }
        }

        static void Add(Dictionary<string, List<(Block, Network)>> d, string op, Block b, Network n)
        {
            if (!d.TryGetValue(op, out var l)) d[op] = l = new List<(Block, Network)>();
            if (!l.Any(x => x.Item2 == n)) l.Add((b, n));
        }
    }

    /// <summary>OB-ERROR: napakovni OB-ji (OB82/OB121/OB122) prazni ali manjkajo, OB86 piše v DB_ERRORS brez meje.</summary>
    public sealed class ErrorObRule : IRule
    {
        public string Id => "OB-ERROR";
        public string Title => "Napakovni OB-ji prazni ali manjkajo";
        static readonly (int nr, string name)[] Wanted = { (82, "diagnostika I/O"), (86, "izpad naprave"), (121, "programska napaka"), (122, "napaka dostopa do I/O") };

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            var obs = plc.Blocks.Where(b => b.Kind == BlockKind.OB).ToList();
            var empty = new List<Block>(); var missing = new List<string>();
            foreach (var (nr, name) in Wanted)
            {
                var ob = obs.FirstOrDefault(o => o.Number == nr);
                if (ob == null) missing.Add("OB" + nr + " (" + name + ")");
                else if (ob.Networks.All(n => !n.Stl.Any(s => s.Op != "COMMENT" && s.Op != "EMPTY_LINE") && (n.Lad == null || n.Lad.Parts.Count == 0) && string.IsNullOrWhiteSpace(n.Scl)))
                    empty.Add(ob);
            }
            if (empty.Count == 0 && missing.Count == 0) yield break;
            var f = new Finding
            {
                RuleId = Id, Category = Categories.Helpers, Severity = Severity.Medium,
                Title = Title,
                Description = (empty.Count > 0 ? "Prazni: " + string.Join(", ", empty.Select(o => o.Name + " [OB" + o.Number + "]")) + ". " : "") +
                              (missing.Count > 0 ? "Manjkajo: " + string.Join(", ", missing) + ". " : "") +
                              "Napake I/O in programske napake (indeks izven meja, napaka dostopa) se tiho spregledajo. Priporočilo: zaklenjena zastavica + števec + alarm.",
            };
            foreach (var o in empty) f.Locations.Add(FindingLocation.Of(o));
            if (f.Locations.Count == 0) f.Locations.Add(new FindingLocation { Path = "PLC " + plc.Name + " – OB-ji" });
            yield return f;
        }
    }

    /// <summary>SCL-OEE: kakovost izračunana kot 1 - Bad/Good (pravilno Good/(Good+Bad)).</summary>
    public sealed class OeeQualityRule : IRule
    {
        public string Id => "SCL-OEE";
        public string Title => "OEE: napačna formula kakovosti (1 − Bad/Good)";
        static readonly Regex Rx = new Regex(@"1(\.0)?\s*-\s*\(?\s*#?\w*Bad\w*\s*/\s*#?\w*Good\w*", RegexOptions.IgnoreCase);

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            foreach (var b in plc.Blocks.Where(x => !x.IsDb))
            {
                var nws = b.Networks.Where(n => n.Scl != null && Rx.IsMatch(n.Scl)).ToList();
                bool inSource = nws.Count == 0 && b.SclSource != null && Rx.IsMatch(b.SclSource);
                if (nws.Count == 0 && !inSource) continue;
                var f = new Finding
                {
                    RuleId = Id, Category = Categories.Helpers, Severity = Severity.Low,
                    Title = Title,
                    Description = "Kakovost je izračunana kot 1 − Bad/Good. Pravilno je Good/(Good+Bad); pri Bad ≥ Good je rezultat 0 ali negativen, pri Good = 0 pa deljenje z 0.",
                };
                f.Locations.Add(FindingLocation.Of(b, nws.ToArray()));
                yield return f;
            }
        }
    }
}
