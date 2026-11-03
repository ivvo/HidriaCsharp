using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// IDX-UNIT: v bloku je indeks globalnega polja uporabljen le redko (≤ 2×), medtem ko blok isto polje sicer uporablja z drugim
    /// (glavnim) indeksom. Tipična napaka kopiranja iz sosednje enote: Unit_20 "DB_Service_Settings.OFF[3]" (sicer OFF[2] 8×),
    /// "TRACKING.PIECE[13].BAD_PART" (sicer PIECE[12] 88×), Unit_40 "TRACKING.PIECE[4]" (sicer PIECE[14]).
    /// Networki, ki naštevajo ≥ 4 različne indekse istega polja (pregled vseh mest), so izvzeti.
    /// </summary>
    public sealed class UnitIndexRule : IRule
    {
        public string Id => "IDX-UNIT";
        public string Title => "Redek indeks polja v bloku enote (morda indeks druge enote)";
        static readonly Regex IdxRx = new Regex("^\"([^\"\\[]+)\\[(\\d+)\\]");
        const int MaxRare = 2, Ratio = 4, Enumeration = 4;

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            var perBlock = plc.Blocks.Where(x => !x.IsDb).Select(b => (b, hits: RareUses(b))).Where(x => x.hits.Count > 0).ToList();
            // redek indeks, ki se enako pojavi v ≥ 2 blokih, je skupen vzorec (npr. predaja s prijemala robota PIECE[1]/[2] v Unit_20 in Unit_30)
            var shared = new HashSet<string>(perBlock.SelectMany(x => x.hits.Select(h => h.key).Distinct())
                .GroupBy(k => k).Where(g => g.Count() >= 2).Select(g => g.Key));

            foreach (var (b, all) in perBlock)
            {
                var hits = all.Where(h => !shared.Contains(h.key)).ToList();
                foreach (var g in hits.GroupBy(h => h.nw).OrderBy(g => g.Key.Index))
                {
                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.CopyIndex, Severity = Severity.Medium, Confidence = "Preveriti",
                        Title = Title,
                        Description = string.Join("; ", g.Select(h => h.text).Distinct().Take(6)) + (g.Count() > 6 ? "; …" : "") +
                                      ". Blok to polje sicer uporablja z drugim indeksom; redek indeks je pogosto ostanek kopiranja iz sosednje enote. " +
                                      "Preveri, ali je dostop do tega mesta namenski (npr. predaja kosa sosednji postaji).",
                    };
                    f.Locations.Add(FindingLocation.Of(b, g.Key));
                    yield return f;
                }
            }
        }

        static List<(string key, Network nw, string text)> RareUses(Block b)
        {
            {
                // (družina, indeks, network) za vsako pojavitev
                var uses = new List<(string fam, int idx, Network nw, string op)>();
                foreach (var nw in b.Networks)
                    foreach (var op in nw.AllOperands())
                    {
                        var m = IdxRx.Match(op);
                        if (m.Success) uses.Add((m.Groups[1].Value, int.Parse(m.Groups[2].Value), nw, op));
                    }

                var hits = new List<(string key, Network nw, string text)>();
                foreach (var fam in uses.GroupBy(u => u.fam))
                {
                    var counts = fam.GroupBy(u => u.idx).ToDictionary(g => g.Key, g => g.Count());
                    if (counts.Count < 2) continue;
                    var main = counts.OrderByDescending(k => k.Value).First();
                    var enumNws = new HashSet<Network>(fam.GroupBy(u => u.nw).Where(g => g.Select(u => u.idx).Distinct().Count() >= Enumeration).Select(g => g.Key));
                    foreach (var rare in counts.Where(k => k.Key != main.Key && k.Value <= MaxRare && main.Value >= Ratio * k.Value))
                        foreach (var u in fam.Where(u => u.idx == rare.Key && !enumNws.Contains(u.nw)))
                            hits.Add((fam.Key + "[" + rare.Key + "]", u.nw, u.op + " (sicer " + fam.Key + "[" + main.Key + "] " + main.Value + "×)"));
                }
                return hits;
            }
        }
    }
}
