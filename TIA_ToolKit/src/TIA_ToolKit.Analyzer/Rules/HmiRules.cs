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
    /// HMI-SCRIPT: VB skripte z gesli v besedilu, absolutnimi potmi (osebna mapa razvijalca), neobstoječimi SmartTags in skripte, ki jih nihče ne kliče;
    /// v PLC (SCL) primerjava gesla s konstanto ('Default.').
    /// </summary>
    public sealed class HmiScriptRule : IRule
    {
        public string Id => "HMI-SCRIPT";
        public string Title => "HMI skripta / geslo: trdo zapisane vrednosti ali mrtva koda";
        static readonly Regex PwdTagRx = new Regex(@"SmartTags\(""([^""]*(Pass|Pwd|Geslo|Passwort)[^""]*)""\)\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase);
        static readonly Regex LogonRx = new Regex(@"\bLogon\s+""([^""]+)""", RegexOptions.IgnoreCase);
        static readonly Regex PathRx = new Regex(@"""([A-Za-z]:\\[^""]+)""");
        static readonly Regex SmartRx = new Regex(@"SmartTags\(""([^""]+)""\)", RegexOptions.IgnoreCase);
        static readonly Regex SclPwdRx = new Regex(@"(""[^""]*(Pass|Pwd|Geslo|Passwort)[^""]*"")\s*=\s*'([^']+)'", RegexOptions.IgnoreCase);

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            // PLC: geslo primerjano s konstanto
            foreach (var b in plc.Blocks.Where(x => !x.IsDb))
                foreach (var nw in b.Networks.Where(n => n.Scl != null))
                    foreach (Match m in SclPwdRx.Matches(nw.Scl))
                    {
                        var f = new Finding
                        {
                            RuleId = Id, Category = Categories.Hmi, Severity = Severity.Low, Title = "Geslo trdo zapisano v PLC",
                            Description = m.Groups[1].Value + " se primerja s konstanto '" + m.Groups[3].Value + "'. Geslo je vidno vsakomur, ki odpre projekt, " +
                                          "vnos pa ostane v DB-ju; za zaščito uporabi upravljanje uporabnikov na HMI (Authorization).",
                        };
                        f.Locations.Add(FindingLocation.Of(b, nw));
                        yield return f;
                    }

            if (plc != project.Plcs.FirstOrDefault()) yield break;
            foreach (var h in project.Hmis)
            {
                var called = new HashSet<string>(h.Screens.SelectMany(s => s.Items).SelectMany(i => i.Actions).Select(a => a.Function).Where(x => x != null)
                                                  .Concat(h.TagEventFunctions), StringComparer.OrdinalIgnoreCase);
                foreach (var sc in h.Scripts)
                {
                    var issues = new List<string>(); var sev = Severity.Low;
                    var code = string.Join("\n", sc.Code.Split('\n').Where(l => !l.TrimStart().StartsWith("'")));   // brez komentarjev
                    var pwds = PwdTagRx.Matches(code).Cast<Match>().Select(m => "\"" + m.Groups[3].Value + "\"")
                               .Concat(LogonRx.Matches(code).Cast<Match>().Select(m => "\"" + m.Groups[1].Value + "\"")).Distinct().ToList();
                    if (pwds.Count > 0) { issues.Add("gesla v čistem besedilu: " + string.Join(", ", pwds)); sev = Severity.High; }
                    var paths = PathRx.Matches(code).Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();
                    if (paths.Count > 0) issues.Add("absolutna pot " + string.Join(", ", paths) + (paths.Any(p => p.IndexOf(@"\Users\", StringComparison.OrdinalIgnoreCase) >= 0) ? " (osebna mapa uporabnika)" : ""));
                    var missing = SmartRx.Matches(code).Cast<Match>().Select(m => m.Groups[1].Value)
                                  .Where(t => !t.StartsWith("@") && !h.TagByName.ContainsKey(t)).Distinct().ToList();
                    if (missing.Count > 0) issues.Add("neobstoječi tagi " + string.Join(", ", missing.Take(6)) + (missing.Count > 6 ? ", …" : ""));
                    bool unused = !called.Contains(sc.Name) && !h.Scripts.Any(o => o != sc && Regex.IsMatch(o.Code, @"\b" + Regex.Escape(sc.Name) + @"\b"));
                    if (issues.Count == 0) continue;   // neklicana skripta brez drugih težav ni vredna vrstice
                    if (unused) issues.Add("skripte ne kliče noben objekt ali skripta (morda časovni razpored - preveri)");
                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.Hmi, Severity = sev, Confidence = sev == Severity.High ? "Preverjeno" : "Preveriti", Title = Title,
                        Description = "Skripta " + sc.Name + ": " + string.Join("; ", issues) + ".",
                    };
                    f.Locations.Add(new FindingLocation { Path = "HMI/" + h.Name + "/" + sc.File });
                    yield return f;
                }
            }
        }
    }

    /// <summary>
    /// HMI-AUTH: nevaren ukaz (premostitev Disable/OFF/Bypass, ročni način, kalibracija, brisanje, zagon etalona, StopRuntime ...) brez avtorizacije
    /// na zaslonu, ki je dosegljiv brez prijave (pot od začetnih zaslonov prek navigacije brez avtorizacije, vklj. gumbe predlog).
    /// </summary>
    public sealed class HmiAuthorizationRule : IRule
    {
        public string Id => "HMI-AUTH";
        public string Title => "Nevaren ukaz na HMI brez avtorizacije";
        static readonly Regex DangerRx = new Regex(
            @"(Disable|(^|(?<!RESET)_)OFF(\[|$|_)|Bypass|Manual|Delete|Clear|Calib\w*_(START|ON)|Etalon\w*_?Start|Robot_Stop|SafetyOff|Safety_Off|Reset_Default|Jog|Force|Password)",
            RegexOptions.IgnoreCase);
        static readonly HashSet<string> WriteFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "SetBit", "ResetBit", "InvertBit", "SetTag", "SetBitWhileKeyPressed", "SetBitInTag", "ResetBitInTag", "InvertBitInTag", "IncreaseTag", "DecreaseTag" };

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            if (plc != project.Plcs.FirstOrDefault()) yield break;
            foreach (var h in project.Hmis)
            {
                var reachable = ReachableWithoutLogin(h);
                var all = new List<(HmiScreen s, HmiItem item, string what)>();
                foreach (var s in h.Screens.Where(x => !x.IsTemplate && reachable.Contains(x.Name)))
                {
                    var hits = new List<(HmiItem item, string what)>();
                    foreach (var i in s.Items.Where(x => string.IsNullOrEmpty(x.Authorization)))
                    {
                        if (i.IsInput)
                            foreach (var b in i.Bindings.Where(b => b.Property == "ProcessValue" && DangerRx.IsMatch(b.Tag))) hits.Add((i, b.Tag));
                        foreach (var a in i.Actions)
                        {
                            if (string.Equals(a.Function, "StopRuntime", StringComparison.OrdinalIgnoreCase)) hits.Add((i, "StopRuntime"));
                            else if (WriteFunctions.Contains(a.Function ?? ""))
                                foreach (var t in a.Tags.Where(t => DangerRx.IsMatch(t))) hits.Add((i, a.Function + " " + t));
                        }
                    }
                    all.AddRange(hits.Select(x => (s, x.item, x.what)));
                }

                const string Advice = ". Premostitve kontrol, ročno krmiljenje, kalibracija ali brisanje podatkov naj zahtevajo prijavo (Authorization na objektu ali na navigaciji do zaslona).";
                // isti objekt + ukaz na ≥ 3 zaslonih (kopiran gumb, npr. Exit → StopRuntime) = ena ugotovitev
                var common = all.GroupBy(x => x.item.Name + "|" + x.what).Where(g => g.Select(x => x.s).Distinct().Count() >= 3).ToList();
                var commonKeys = new HashSet<string>(common.Select(g => g.Key));
                foreach (var g in common)
                {
                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.Hmi, Severity = Severity.Medium, Confidence = "Preveriti", Title = Title,
                        Description = g.First().item.Name + " → " + g.First().what + " brez avtorizacije na " + g.Select(x => x.s).Distinct().Count() +
                                      " zaslonih, dosegljivih brez prijave (" + string.Join(", ", g.Select(x => x.s.Name).Distinct()) + ")" + Advice,
                    };
                    foreach (var x in g) f.Locations.Add(new FindingLocation { Path = "HMI/" + h.Name + "/" + x.s.File, NetworkTitle = x.item.Name });
                    yield return f;
                }
                foreach (var g in all.Where(x => !commonKeys.Contains(x.item.Name + "|" + x.what)).GroupBy(x => x.s))
                {
                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.Hmi, Severity = Severity.Medium, Confidence = "Preveriti", Title = Title,
                        Description = "Zaslon " + g.Key.Name + " je dosegljiv brez prijave, objekti brez avtorizacije pa izvedejo: " +
                                      string.Join("; ", g.Select(x => x.item.Name + " → " + x.what).Distinct().Take(10)) + (g.Count() > 10 ? "; …" : "") + Advice,
                    };
                    f.Locations.Add(new FindingLocation { Path = "HMI/" + h.Name + "/" + g.Key.File, NetworkTitle = string.Join(", ", g.Select(x => x.item.Name).Distinct().Take(8)) });
                    yield return f;
                }
            }
        }

        /// <summary>Zasloni, dosegljivi brez prijave: od zaslonov brez vhodne navigacije (začetni) prek ActivateScreen na objektih brez avtorizacije.</summary>
        static HashSet<string> ReachableWithoutLogin(HmiTarget h)
        {
            var templates = h.Screens.Where(s => s.IsTemplate).ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            var edges = new Dictionary<string, List<(string to, bool open)>>(StringComparer.OrdinalIgnoreCase);
            var inbound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in h.Screens.Where(x => !x.IsTemplate))
            {
                var items = s.Items.AsEnumerable();
                if (s.Template != null && templates.TryGetValue(s.Template, out var t)) items = items.Concat(t.Items);
                var list = new List<(string, bool)>();
                foreach (var i in items)
                    foreach (var a in i.Actions.Where(a => (a.Function ?? "").StartsWith("ActivateScreen", StringComparison.OrdinalIgnoreCase)))
                    {
                        string to = a.Tags.FirstOrDefault() ?? a.Values.FirstOrDefault();
                        if (to == null || to == s.Name) continue;
                        list.Add((to, string.IsNullOrEmpty(i.Authorization)));
                        inbound.Add(to);
                    }
                edges[s.Name] = list;
            }
            var seen = new HashSet<string>(h.Screens.Where(x => !x.IsTemplate && !inbound.Contains(x.Name)).Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>(seen);
            while (queue.Count > 0)
                if (edges.TryGetValue(queue.Dequeue(), out var outs))
                    foreach (var e in outs.Where(e => e.open && seen.Add(e.to))) queue.Enqueue(e.to);
            return seen;
        }
    }

    /// <summary>
    /// HMI-UNIT: (1) na zaslonu enote (ime vsebuje številko enote, npr. Postaja_50) je vnosno polje vezano na tag druge enote (U60_*);
    /// (2) isti tag (z indeksom) je na enem zaslonu vezan na več vnosnih polj - vnos v eno vrstico spremeni drugo (U60_Timers[1] 3×).
    /// </summary>
    public sealed class HmiUnitRule : IRule
    {
        public string Id => "HMI-UNIT";
        public string Title => "Vnosno polje vezano na tag druge enote ali podvojeno";
        static readonly Regex UnitRx = new Regex(@"^(?:U|T|UT)_?(\d{2,3})_", RegexOptions.IgnoreCase);

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            if (plc != project.Plcs.FirstOrDefault()) yield break;
            foreach (var h in project.Hmis)
            {
                var foreignAll = new List<(HmiScreen s, string own, HmiItem item, string tag)>();
                foreach (var s in h.Screens.Where(x => !x.IsTemplate))
                {
                    var inputs = s.Items.Where(i => i.IsInput).SelectMany(i => i.Bindings.Where(b => b.Property == "ProcessValue").Select(b => (item: i, tag: b.Tag))).ToList();
                    if (inputs.Count == 0) continue;

                    // (1) tuja enota - zbere za vse zaslone, poroča spodaj
                    var units = s.Items.SelectMany(i => i.Bindings).Select(b => UnitRx.Match(b.Tag)).Where(m => m.Success).Select(m => m.Groups[1].Value).ToList();
                    string own = units.GroupBy(u => u).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                    if (own != null && Regex.IsMatch(s.Name, @"(^|\D)" + own + @"(\D|$)"))
                        foreach (var x in inputs) { var m = UnitRx.Match(x.tag); if (m.Success && m.Groups[1].Value != own) foreignAll.Add((s, own, x.item, x.tag)); }

                    // (2) isti tag na več vnosnih poljih; zaslon-knjižnica (en tag na ≥ 8 objektih, npr. Nabor_Stikal) ni napaka
                    var dup = inputs.GroupBy(x => x.tag, StringComparer.OrdinalIgnoreCase)
                                    .Where(g => g.Select(x => x.item).Distinct().Count() > 1 && g.Select(x => x.item).Distinct().Count() < 8).ToList();
                    if (dup.Count > 0)
                        yield return Make(h, s, dup.SelectMany(g => g).ToList(), Severity.Medium,
                            "Zaslon " + s.Name + ": " + string.Join("; ", dup.Take(8).Select(g => g.Key + " na " + string.Join(", ", g.Select(x => x.item.Name).Distinct()))) +
                            ". Isti tag je vezan na več vnosnih polj (verjetno kopirane vrstice brez popravljenega indeksa) - vnos v eno polje spremeni tudi druga, prave vrednosti pa niso dosegljive.");
                }

                // isti tuji objekt (ime + tag) na ≥ 3 zaslonih = kopiran (pogosto prekrit) objekt → ena ugotovitev
                var common = foreignAll.GroupBy(x => x.item.Name + "→" + x.tag).Where(g => g.Select(x => x.s).Distinct().Count() >= 3).ToList();
                var commonKeys = new HashSet<string>(common.Select(g => g.Key));
                foreach (var g in common)
                {
                    var first = g.First();
                    var f = new Finding
                    {
                        RuleId = Id, Category = Categories.CopyIndex, Severity = Severity.Medium, Confidence = "Preveriti", Title = Title,
                        Description = "Objekt " + first.item.Name + " (vnos, tag " + first.tag + ") je na " + g.Select(x => x.s).Distinct().Count() + " zaslonih drugih enot (" +
                                      string.Join(", ", g.Select(x => x.s.Name).Distinct()) + "). Kopiran objekt (pogosto skrit pod drugim poljem) - prek njega se na tujem zaslonu spremeni vrednost enote " +
                                      UnitRx.Match(first.tag).Groups[1].Value + " (npr. s tipko Tab).",
                    };
                    foreach (var x in g) f.Locations.Add(new FindingLocation { Path = "HMI/" + h.Name + "/" + x.s.File, NetworkTitle = x.item.Name });
                    if (h.TagByName.TryGetValue(HmiUse.Base(first.tag), out var tag)) f.Locations.Add(HmiUse.TagLocation(h, tag));
                    yield return f;
                }
                foreach (var g in foreignAll.Where(x => !commonKeys.Contains(x.item.Name + "→" + x.tag)).GroupBy(x => x.s))
                    yield return Make(h, g.Key, g.Select(x => (x.item, x.tag)).ToList(), Severity.High,
                        "Zaslon " + g.Key.Name + " (enota " + g.First().own + "): vnosna polja " + string.Join(", ", g.Select(x => x.item.Name + " → " + x.tag).Distinct().Take(8)) +
                        " so vezana na tage druge enote. Vrednosti te enote se zato s tega zaslona ne dajo nastaviti, vnos pa spremeni nastavitve tuje enote.");
            }
        }

        Finding Make(HmiTarget h, HmiScreen s, List<(HmiItem item, string tag)> hits, Severity sev, string desc)
        {
            var f = new Finding { RuleId = Id, Category = Categories.CopyIndex, Severity = sev, Confidence = "Preveriti", Title = Title, Description = desc };
            f.Locations.Add(new FindingLocation { Path = "HMI/" + h.Name + "/" + s.File, NetworkTitle = string.Join(", ", hits.Select(x => x.item.Name).Distinct().Take(8)) });
            foreach (var t in hits.Select(x => HmiUse.Base(x.tag)).Distinct().Take(4))
                if (h.TagByName.TryGetValue(t, out var tag)) f.Locations.Add(HmiUse.TagLocation(h, tag));
            return f;
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
