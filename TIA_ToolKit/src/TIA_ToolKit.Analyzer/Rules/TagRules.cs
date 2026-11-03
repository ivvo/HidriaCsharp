using System.Collections.Generic;
using System.Linq;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>TAG-OVERLAP: tagi različne širine, ki se prekrivajo (npr. %IW1822 znotraj %ID1820). Taktni/sistemski bajt sta izvzeta.</summary>
    public sealed class TagOverlapRule : IRule
    {
        public string Id => "TAG-OVERLAP";
        public string Title => "Prekrivanje naslovov PLC tagov";

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            // uporabljeni globalni simboli (prvi del operanda brez narekovajev)
            var used = new HashSet<string>(plc.Blocks.SelectMany(b => b.Networks).SelectMany(n => n.AllOperands())
                .Where(o => o.StartsWith("\"")).Select(o => o.Substring(1).Split('"')[0]));
            var placeholders = new List<(PlcTag tag, List<PlcTag> over)>();

            foreach (var area in plc.Tags.Where(t => t.BitLo >= 0).GroupBy(t => t.Area))
            {
                var list = area.OrderBy(t => t.BitLo).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var a = list[i];
                    if (IsContainerByte(a)) continue;
                    var overlaps = new List<PlcTag>();
                    for (int j = i + 1; j < list.Count && list[j].BitLo <= a.BitHi; j++)
                        if (list[j].Address != a.Address && !IsContainerByte(list[j])) overlaps.Add(list[j]);
                    if (overlaps.Count == 0) continue;
                    if (IsPlaceholder(a) && !used.Contains(a.Name)) { placeholders.Add((a, overlaps)); continue; }
                    yield return new Finding
                    {
                        RuleId = Id, Category = Categories.Tags, Severity = Severity.Medium, Confidence = "Preveriti",
                        Title = "Prekrivanje: " + a.Address + " " + a.Name,
                        Description = a.Address + " " + a.Name + " (" + a.DataType + ") se prekriva z: " +
                                      string.Join(", ", overlaps.Take(6).Select(o => o.Address + " " + o.Name + " (" + o.DataType + ")")) + (overlaps.Count > 6 ? ", …" : "") +
                                      ". Zapis v en tag spremeni drugega; preveri IO mapo naprave ali odstrani zastarele tage.",
                        Locations = { new FindingLocation { Path = "Tags/" + a.Table.Replace('\\', '/') } },
                    };
                }
            }

            // neuporabljeni tagi s privzetim imenom (Tag_1 ...) čez bite: ena skupna, nizka ugotovitev namesto ene na tag
            if (placeholders.Count > 0)
            {
                var f = new Finding
                {
                    RuleId = Id, Category = Categories.Tags, Severity = Severity.Low, Confidence = "Preveriti",
                    Title = "Neuporabljeni tagi s privzetim imenom čez bite",
                    Description = string.Join(", ", placeholders.Select(p => p.tag.Address + " " + p.tag.Name)) +
                                  ": tagi s privzetim imenom se prekrivajo z biti (" +
                                  string.Join(", ", placeholders.SelectMany(p => p.over).Take(4).Select(o => o.Address + " " + o.Name)) + ", …), program pa jih ne uporablja. " +
                                  "Verjetno ostanek – odstrani jih, da kdo prek njih ne zapiše cele vrste vhodov/izhodov.",
                };
                foreach (var t in placeholders.Select(p => p.tag.Table).Distinct())
                    f.Locations.Add(new FindingLocation { Path = "Tags/" + t.Replace('\\', '/') });
                yield return f;
            }
        }

        static bool IsContainerByte(PlcTag t) => t.Name == "Clock_Byte" || t.Name == "System_Byte";
        static bool IsPlaceholder(PlcTag t) => System.Text.RegularExpressions.Regex.IsMatch(t.Name ?? "", @"^Tag_\d+$");
    }

    /// <summary>TAG-SYSBYTE: System_Byte na %MB1 + biti M1.x, ki jih program postavlja/briše (StartUp = M1.2 je AlwaysTRUE, če je sistemski bajt vklopljen).</summary>
    public sealed class SystemByteRule : IRule
    {
        public string Id => "TAG-SYSBYTE";
        public string Title => "Sistemski bajt %MB1: program piše v sistemske bite";

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            var sys = plc.Tags.FirstOrDefault(t => t.Name == "System_Byte" && t.Address == "%MB1");
            if (sys == null) yield break;
            var bits = plc.Tags.Where(t => t.Area == 'M' && t.BitLo >= 8 && t.BitHi <= 11 && t.BitLo == t.BitHi).ToList();   // M1.0..M1.3
            var written = new List<string>(); var locs = new List<FindingLocation>();
            foreach (var t in bits)
            {
                string op = "\"" + t.Name + "\"";
                foreach (var b in plc.Blocks.Where(x => !x.IsDb))
                {
                    var nws = b.Networks.Where(n => n.Stl.Any(s => (s.Op == "S" || s.Op == "R" || s.Op == "Assign") && s.Operand == op)).ToList();
                    if (nws.Count == 0) continue;
                    if (!written.Contains(t.Address + " " + t.Name)) written.Add(t.Address + " " + t.Name);
                    locs.Add(FindingLocation.Of(b, nws.ToArray()));
                }
            }
            if (written.Count == 0) yield break;
            var f = new Finding
            {
                RuleId = Id, Category = Categories.Tags, Severity = Severity.Low, Confidence = "Preveriti",
                Title = Title,
                Description = "Tag System_Byte je na %MB1, program pa piše " + string.Join(", ", written) + ". Če je v lastnostih CPU vklopljen sistemski bajt na MB1, " +
                              "so M1.0 FirstScan, M1.1 DiagStatusUpdate, M1.2 AlwaysTRUE, M1.3 AlwaysFALSE – zapisi se vsak cikel prepišejo (npr. StartUp = M1.2 ostane TRUE). Preveri HW konfiguracijo.",
            };
            f.Locations.AddRange(locs);
            yield return f;
        }
    }
}
