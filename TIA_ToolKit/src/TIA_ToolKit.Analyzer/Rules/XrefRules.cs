using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// XREF-WRITEONLY: globalni signal z "rezultatskim" imenom (BAD/NOK/ERROR/ALARM/FAULT/CANCEL/ABORT) se v PLC zapisuje (S/R/=/T, tuljava, SCL :=),
    /// a se nikjer ne bere - rezultat nima posledic (npr. Etalon_All_BAD: NOK etalon ne ustavi proizvodnje).
    /// Bere = katerakoli druga referenca (pogoj, L, parameter klica, desna stran v SCL), tudi prek nadrejene strukture ali spremenljivega indeksa.
    /// Podatkovni bloki za zunanje odjemalce (SQL/SCADA/HMI/sporočila) so izvzeti - te bere SCADA/HMI, ne PLC.
    /// </summary>
    public sealed class WriteOnlyRule : IRule
    {
        public string Id => "XREF-WRITEONLY";
        public string Title => "Rezultat se zapisuje, a se v PLC nikjer ne bere";
        // po žetonih imena (Reset_Default ne sme zadeti FAULT)
        static readonly Regex NameRx = new Regex(@"(^|_)(BAD|NOK|NG|ERROR|ERR|ALARM|FAULT|CANCELL?|ABORT)(_|\d|$)", RegexOptions.IgnoreCase);
        static readonly Regex StrongRx = new Regex(@"(^|_)(BAD|NOK|NG|CANCELL?|ABORT)(_|\d|$)", RegexOptions.IgnoreCase);
        static readonly Regex ExternalRx = new Regex(@"^""[^"".]*(SQL|SCADA|HMI|Error_Messages|Messages|Alarm|Ignition|OPC)", RegexOptions.IgnoreCase);
        static readonly Regex SclLhsRx = new Regex("(\"[^\"]+\")\\s*:=");
        static readonly Regex VarIdxRx = new Regex(@"\[[^\]]*[#""][^\]]*\]");
        static readonly Regex ConstIdxRx = new Regex(@"\[\d+(,\d+)*\]");

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            var refs = new Dictionary<string, int>();
            var writes = new Dictionary<string, List<(Block b, Network n)>>();
            foreach (var b in plc.Blocks.Where(x => !x.IsDb))
                foreach (var nw in b.Networks)
                {
                    foreach (var r in nw.Refs) refs[r] = refs.TryGetValue(r, out var c) ? c + 1 : 1;
                    foreach (var s in nw.Stl.Where(s => (s.Op == "S" || s.Op == "R" || s.Op == "Assign" || s.Op == "T") && s.Operand != null && s.Operand.StartsWith("\"")))
                        AddWrite(writes, s.Operand, b, nw);
                    if (nw.Lad != null)
                        foreach (var p in nw.Lad.Parts.Values.Where(p => p.IsCoil && p.Operand != null && p.Operand.StartsWith("\"")))
                            AddWrite(writes, p.Operand, b, nw);
                    if (nw.Scl != null)
                        foreach (Match m in SclLhsRx.Matches(nw.Scl)) AddWrite(writes, m.Groups[1].Value, b, nw);
                }

            // prebrano prek nadrejene strukture ("DB.ETALONS" kot parameter) ali prek spremenljivega indeksa ("PIECE[#i].X")
            var readPrefixes = new HashSet<string>();
            var readWildcard = new HashSet<string>();
            foreach (var kv in refs)
            {
                int w = writes.TryGetValue(kv.Key, out var wl) ? wl.Count : 0;
                if (kv.Value <= w) continue;
                string t = kv.Key.Trim('"');
                readPrefixes.Add(t);
                if (VarIdxRx.IsMatch(t)) readWildcard.Add(VarIdxRx.Replace(t, "[*]"));
            }

            // izhodi %Q so namenjeni napravi (laser, robot) - njih PLC ne bere
            var outputs = new HashSet<string>(plc.Tags.Where(t => t.Area == 'Q').Select(t => "\"" + t.Name + "\""));
            var cand = new List<string>();
            foreach (var kv in writes)
            {
                string t = kv.Key.Trim('"');
                string member = ConstIdxRx.Replace(t.Substring(t.LastIndexOf('.') + 1), "");
                if (!NameRx.IsMatch(member) || ExternalRx.IsMatch(kv.Key) || outputs.Contains(kv.Key)) continue;
                if (VarIdxRx.IsMatch(t)) continue;   // brisanje v zanki (PIECE[#Clear].BAD_PART)
                if (IsRead(t, readPrefixes, readWildcard)) continue;
                cand.Add(kv.Key);
            }
            // statusno polje za HMI (ERROR[1..10]: vsaka enota zapiše svoj indeks, bere ga HMI) - ≥ 3 indeksi iste družine
            var statusFamilies = new HashSet<string>(cand.Where(c => ConstIdxRx.IsMatch(c)).GroupBy(c => ConstIdxRx.Replace(c, "[]"))
                .Where(g => g.Count() >= 3).Select(g => g.Key));
            var hits = cand.Where(c => !statusFamilies.Contains(ConstIdxRx.Replace(c, "[]")))
                           .SelectMany(c => writes[c].Select(w => (sym: c, w.b, w.n))).ToList();

            foreach (var g in hits.GroupBy(h => h.b))
            {
                var syms = g.Select(h => h.sym).Distinct().OrderBy(s => s).ToList();
                var f = new Finding
                {
                    RuleId = Id, Category = Categories.Functional, Confidence = "Preveriti",
                    Severity = syms.Any(s => StrongRx.IsMatch(s.Trim('"').Substring(s.Trim('"').LastIndexOf('.') + 1))) ? Severity.Medium : Severity.Low,
                    Title = Title,
                    Description = string.Join(", ", syms.Take(6)) + (syms.Count > 6 ? ", … (" + syms.Count + ")" : "") +
                                  ": se v bloku zapisuje, v PLC pa ga noben pogoj ne bere. Rezultat (slab kos, napaka, preklic) zato nima posledic - ni alarma ne blokade. " +
                                  "Če ga bere samo HMI/SCADA, je to namerno; sicer manjka logika, ki se nanj odzove.",
                };
                f.Locations.Add(FindingLocation.Of(g.Key, g.Select(h => h.n).Distinct().ToArray()));
                yield return f;
            }
        }

        static bool IsRead(string t, HashSet<string> prefixes, HashSet<string> wildcard)
        {
            if (prefixes.Contains(t)) return true;
            for (int i = 0; i < t.Length; i++)
                if ((t[i] == '.' || t[i] == '[') && prefixes.Contains(t.Substring(0, i))) return true;
            string w = ConstIdxRx.Replace(t, "[*]");
            if (w == t) return false;
            if (wildcard.Contains(w)) return true;
            for (int i = 0; i < w.Length; i++)
                if (w[i] == '.' && wildcard.Contains(w.Substring(0, i))) return true;
            return false;
        }

        static void AddWrite(Dictionary<string, List<(Block, Network)>> d, string op, Block b, Network n)
        {
            if (!d.TryGetValue(op, out var l)) d[op] = l = new List<(Block, Network)>();
            l.Add((b, n));
        }
    }
}
