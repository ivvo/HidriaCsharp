using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>Skupne pomožne funkcije za HMI pravila (raba tagov na zaslonih, razrešitev absolutnega naslova v PLC člana).</summary>
    static class HmiUse
    {
        /// <summary>Kje je tag uporabljen: zaslon → objekti.</summary>
        public static Dictionary<string, List<(HmiScreen screen, HmiItem item)>> Usage(HmiTarget h)
        {
            var d = new Dictionary<string, List<(HmiScreen, HmiItem)>>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in h.Screens)
                foreach (var i in s.Items)
                    foreach (var t in i.Bindings.Select(b => b.Tag).Concat(i.Actions.SelectMany(a => a.Tags)).Concat(i.AnimationTags).Select(Base).Distinct())
                    {
                        if (!d.TryGetValue(t, out var l)) d[t] = l = new List<(HmiScreen, HmiItem)>();
                        l.Add((s, i));
                    }
            return d;
        }

        /// <summary>"U60_Timers[3]" → "U60_Timers".</summary>
        public static string Base(string tag) { int i = tag.IndexOf('['); return i < 0 ? tag : tag.Substring(0, i); }

        public static FindingLocation TagLocation(HmiTarget h, HmiTag t) =>
            new FindingLocation { Path = "HMI/" + h.Name + "/" + t.File + "  (tag " + t.Name + ")" };

        public static IEnumerable<FindingLocation> ScreenLocations(HmiTarget h, IEnumerable<(HmiScreen screen, HmiItem item)> uses) =>
            uses.GroupBy(u => u.screen).Select(g => new FindingLocation
            {
                Path = "HMI/" + h.Name + "/" + g.Key.File,
                NetworkTitle = string.Join(", ", g.Select(u => u.item.Name).Distinct().Take(6)),
            });

        public static int Width(string dataType, int length)
        {
            switch ((dataType ?? "").ToLowerInvariant())
            {
                case "bool": return 0;
                case "byte": case "char": case "sint": case "usint": return 1;
                case "int": case "word": case "uint": case "s5time": case "date": case "wchar": return 2;
                case "dint": case "dword": case "udint": case "real": case "time": case "time_of_day": return 4;
                case "lreal": case "lint": case "lword": return 8;
                default: return Math.Max(length, 1);
            }
        }

        static readonly HashSet<string> Stop = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "on", "off", "req", "in", "out", "db", "settings", "setting", "unit", "sig", "isig", "osig", "the", "of", "to", "x", "hmi", "plc", "set", "reset", "value" };
        static readonly Regex TokRx = new Regex(@"[A-Z]?[a-z]+|[A-Z]+(?![a-z])|\d+");

        /// <summary>Pomenski deli imena (brez U20/T10, številk in splošnih besed).</summary>
        public static HashSet<string> Tokens(string s)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in TokRx.Matches(s ?? ""))
            {
                string t = m.Value.ToLowerInvariant();
                if (t.Length < 3 || Stop.Contains(t) || Regex.IsMatch(t, @"^\d+$")) continue;
                set.Add(t);
            }
            return set;
        }

        public static bool Similar(HashSet<string> a, HashSet<string> b) =>
            a.Count == 0 || b.Count == 0 ||
            a.Any(x => b.Any(y => x == y || x.Length >= 4 && y.Length >= 4 && (x.StartsWith(y.Substring(0, 4)) || y.StartsWith(x.Substring(0, 4)))
                                  || x.Length >= 5 && y.Length >= 5 && Distance(x, y) <= 2));   // tipkarske: Qantity/Quantity, Comfirm/Confirm

        static int Distance(string s, string t)
        {
            var d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;
            for (int i = 1; i <= s.Length; i++)
                for (int j = 1; j <= t.Length; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + (s[i - 1] == t[j - 1] ? 0 : 1));
            return d[s.Length, t.Length];
        }
    }

    /// <summary>
    /// HMI-ADDR: absolutni HMI tag se razreši v člana PLC (DB s standardno postavitvijo - DbLayout, ali %M/%I/%Q tag).
    /// Napaka, če DB ne obstaja, je naslov izven DB, se tip ne ujema (bit v Int, beseda čez String), tag kaže na rezervni člen
    /// (Empty_*, TEST[n]) ali se ime HMI taga z imenom PLC člana ne ujema v nobenem pomenskem delu.
    /// Primeri 122A: T20_Robot_Stop_REQ → DB_Service_Settings.TEST[49], U60_Dosing_EMPTY → bit v Part_n_BAD_Count, ERROR_ON → Empty_17.
    /// </summary>
    public sealed class HmiAddressRule : IRule
    {
        public string Id => "HMI-ADDR";
        public string Title => "HMI tag kaže na napačen PLC naslov";
        static readonly Regex DbRx = new Regex(@"^%DB(\d+)\.DB([XBWD])(\d+)(?:\.(\d))?$", RegexOptions.IgnoreCase);
        static readonly Regex AreaRx = new Regex(@"^%([IQM])([BWD]?)(\d+)(?:\.(\d))?$", RegexOptions.IgnoreCase);
        static readonly Regex SpareRx = new Regex(@"(^|[._])(Empty|Spare|Reserve|Rezerva|Dummy)_?\d+$|^TEST\[\d+\]$", RegexOptions.IgnoreCase);

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            if (plc != project.Plcs.FirstOrDefault()) yield break;   // HMI pravila enkrat na projekt
            foreach (var h in project.Hmis)
            {
                var usage = HmiUse.Usage(h);
                var nameMismatch = new List<(HmiTag tag, string plcName)>();
                foreach (var t in h.Tags.Where(x => x.Address != null))
                {
                    usage.TryGetValue(t.Name, out var uses);
                    uses = uses ?? new List<(HmiScreen, HmiItem)>();
                    bool isArray = (t.DataType ?? "").StartsWith("Array", StringComparison.OrdinalIgnoreCase);
                    string problem = null, plcName = null; Severity sev = Severity.High;

                    var m = DbRx.Match(t.Address);
                    if (m.Success)
                    {
                        int nr = int.Parse(m.Groups[1].Value), byteOff = int.Parse(m.Groups[3].Value);
                        int bit = m.Groups[4].Success && !isArray ? int.Parse(m.Groups[4].Value) : -1;
                        int width = isArray ? Math.Max(t.Length, 1) : HmiUse.Width(t.DataType, t.Length);
                        var db = plc.Blocks.FirstOrDefault(b => b.IsDb && b.Number == nr);
                        if (db == null) problem = "DB" + nr + " v PLC ne obstaja";
                        else
                        {
                            var lay = DbLayout.For(plc, db);
                            if (lay == null || !lay.IsCertain(byteOff)) continue;   // optimiziran DB ali neumerjen del postavitve
                            int end = byteOff + (width == 0 ? 1 : width);
                            if (lay.Complete && end > lay.Size)
                                problem = t.Address + " je izven " + db.Name + " (velikost " + lay.Size + " B)";
                            else if (isArray)
                            {
                                if (!lay.Leaves.Any(x => x.Byte == byteOff && (x.Bit <= 0)))
                                    problem = "polje se začne sredi člana: " + string.Join(", ", lay.At(byteOff, -1, 1).Take(2).Select(x => x.Path + " (" + x.Type + ")"));
                            }
                            else if (width == 0)
                            {
                                var leaf = lay.Leaves.FirstOrDefault(x => x.IsBool && x.Byte == byteOff && x.Bit == bit);
                                if (leaf == null)
                                    problem = "bit " + t.Address + " leži znotraj " + string.Join(", ", lay.At(byteOff, -1, 1).Take(2).Select(x => db.Name + "." + x.Path + " (" + x.Type + ")"));
                                else plcName = db.Name + "." + leaf.Path;
                            }
                            else
                            {
                                var leaf = lay.Leaves.FirstOrDefault(x => !x.IsBool && x.Byte == byteOff && (x.Size == width || x.Type.StartsWith("String", StringComparison.OrdinalIgnoreCase)));
                                var covered = lay.At(byteOff, -1, width);
                                // Word/DWord čez same Bool-e = običajen dostop do bitov (SetBitInTag), ne napaka
                                if (leaf == null && covered.Count > 0 && covered.All(x => x.IsBool)) continue;
                                if (leaf == null)
                                    problem = t.Address + " (" + t.DataType + ") ne ustreza nobenemu članu - pokriva " +
                                              string.Join(", ", lay.At(byteOff, -1, width).Take(3).Select(x => db.Name + "." + x.Path + " (" + x.Type + ")"));
                                else plcName = db.Name + "." + leaf.Path;
                            }
                        }
                    }
                    else
                    {
                        var a = AreaRx.Match(t.Address);
                        if (!a.Success) continue;
                        var pt = plc.Tags.FirstOrDefault(x => string.Equals(x.Address, t.Address, StringComparison.OrdinalIgnoreCase));
                        if (pt == null) continue;
                        plcName = pt.Name;
                    }

                    if (problem == null && plcName != null)
                    {
                        string last = plcName.Substring(plcName.IndexOf('.') + 1);
                        if (SpareRx.IsMatch(last) || SpareRx.IsMatch(plcName.Substring(plcName.LastIndexOf('.') + 1)))
                        {
                            problem = t.Address + " je rezervni/testni člen " + plcName + ", ki ga PLC ne uporablja za ta namen";
                            // HMI vanj piše (gumb SetBit, vnosno polje) → ukaz ne pride do PLC
                            sev = uses.Any(u => u.item.IsInput || u.item.Actions.Any(a => a.Tags.Any(x => HmiUse.Base(x) == t.Name))) ? Severity.High : Severity.Medium;
                        }
                        else if (!isArray && !plcName.EndsWith("]") && !HmiUse.Similar(HmiUse.Tokens(t.Name), HmiUse.Tokens(plcName)))
                        { nameMismatch.Add((t, plcName)); continue; }
                    }
                    if (problem == null) continue;
                    if (uses.Count == 0) sev = Severity.Low;   // tag na zaslonih ni uporabljen - nevaren šele ob ponovni uporabi

                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.Hmi, Severity = sev,
                        Title = "HMI tag " + t.Name + ": napačen naslov",
                        Description = "HMI tag " + t.Name + " (" + t.Address + ", " + t.DataType + "): " + problem + ". " +
                                      (uses.Count > 0 ? "Uporabljen na: " + string.Join(", ", uses.Select(u => u.screen.Name + "/" + u.item.Name).Distinct().Take(5)) + "." : "Na zaslonih ni uporabljen."),
                    };
                    f.Locations.Add(HmiUse.TagLocation(h, t));
                    f.Locations.AddRange(HmiUse.ScreenLocations(h, uses));
                    yield return f;
                }

                // neujemanje imen: samo uporabljeni tagi, po en zapis na tabelo tagov
                foreach (var g in nameMismatch.Where(x => usage.ContainsKey(x.tag.Name)).GroupBy(x => x.tag.File))
                {
                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.Hmi, Severity = Severity.Low, Confidence = "Preveriti",
                        Title = "Ime HMI taga se ne ujema s PLC naslovom (" + g.Key + ")",
                        Description = string.Join("; ", g.Take(10).Select(x => x.tag.Name + " (" + x.tag.Address + ") → " + x.plcName)) + (g.Count() > 10 ? "; …" : "") +
                                      ". HMI tag kaže na PLC člana z drugačnim pomenom - verjetno napačen ali zastarel absoluten naslov (kopiranje z druge enote, premaknjen DB).",
                    };
                    foreach (var x in g) f.Locations.Add(HmiUse.TagLocation(h, x.tag));
                    yield return f;
                }
            }
        }
    }
}
