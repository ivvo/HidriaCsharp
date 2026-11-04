using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TIA_Generator.Extract;
using TIA_Generator.Model;

namespace TIA_Generator.Generate
{
    /// <summary>
    /// PLC ogrodje novih postaj (Origin = "struktura"): klon vzorčne enote iz exporta (privzeto Purem Unit_A05 – »Spare«, standardno ogrodje
    /// postaje s 40 networki) v SimaticML XML za uvoz:
    ///   Unit_&lt;oznaka&gt;.xml (FB, številka = PLC indeks), Unit_&lt;oznaka&gt;_DB.xml (instanca, številka = PLC indeks),
    ///   Unit_&lt;oznaka&gt;_DB_Settings.xml (DB 100 + indeks; Delay : Array[0..9] of S5Time + Part_n_BAD_Count, standard 4.3).
    /// Zamenjave v FB: ime enote (A05 → oznaka) v imenih simbolov, indeks postaje v poljih DB_* (OFF[15] → OFF[n]),
    /// člani časovnikov vzorca (Delay_Position_Error …) → Delay[i]. Poročilo našteje simbole, ki jih je treba v PLC še ustvariti.
    /// </summary>
    public sealed class PlcSkeleton
    {
        public sealed class Template
        {
            public string ExportDir;
            public string UnitId = "A05";
        }

        public sealed class Result
        {
            public List<string> Files = new List<string>();
            public List<string> Report = new List<string>();
            public int Units;
        }

        readonly Action<string> _log;
        public PlcSkeleton(Action<string> log = null) { _log = log ?? (s => { }); }

        static string L(XElement e) => e.Name.LocalName;

        public Result Generate(MachineModelFile m, Template tpl, string outDir, IEnumerable<string> onlyUnits = null)
        {
            var res = new Result();
            var units = m.Units.Where(u => u.Origin == "struktura").ToList();
            if (onlyUnits != null) { var set = new HashSet<string>(onlyUnits); units = m.Units.Where(u => set.Contains(u.Id)).ToList(); }
            if (units.Count == 0) { res.Report.Add("Ni novih postaj (Origin = struktura) – nič za generirati."); return res; }

            // --- vzorec
            var blocksDir = Directory.GetDirectories(Path.Combine(tpl.ExportDir, "PLC")).Select(d => Path.Combine(d, "Blocks")).FirstOrDefault(Directory.Exists)
                            ?? throw new DirectoryNotFoundException("Vzorčni export nima PLC\\<plc>\\Blocks: " + tpl.ExportDir);
            string Find(string name) => Directory.EnumerateFiles(blocksDir, name + ".xml", SearchOption.AllDirectories).FirstOrDefault()
                                        ?? throw new FileNotFoundException("V vzorčnem exportu ni bloka " + name + ".xml");
            var fbFile = Find("Unit_" + tpl.UnitId);
            var idbFile = Find("Unit_" + tpl.UnitId + "_DB");
            var setFile = Find("Unit_" + tpl.UnitId + "_DB_Settings");
            var fbTpl = XDocument.Load(fbFile, LoadOptions.PreserveWhitespace);
            var idbTpl = XDocument.Load(idbFile, LoadOptions.PreserveWhitespace);
            var setTpl = XDocument.Load(setFile, LoadOptions.PreserveWhitespace);

            int oldIdx = TemplateIndex(fbTpl);
            var tplTimers = TemplateTimers(setTpl);
            res.Report.Add("Vzorec: Unit_" + tpl.UnitId + " iz " + tpl.ExportDir + " (indeks " + oldIdx + ", časovniki vzorca: " + string.Join(", ", tplTimers.Keys) + ")");
            res.Report.Add("Izhod: " + outDir);
            res.Report.Add("");

            var existingNumbers = ExistingNumbers(m.SourceExport);
            var alarmBits = new HashSet<string>(m.Global.Alarms.Select(a => a.Bit));

            foreach (var u in units.OrderBy(x => x.PlcIndex))
            {
                if (!u.PlcIndex.HasValue || u.PlcIndex <= 0) { res.Report.Add(u.Id + ": PRESKOČENO – ni PLC indeksa."); continue; }
                int idx = u.PlcIndex.Value;
                string dir = Path.Combine(outDir, "PLC", (u.Folder ?? ("Machine/" + u.Id)).Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(dir);
                var renamed = new SortedSet<string>(StringComparer.Ordinal);

                // FB
                var fb = new XDocument(fbTpl);
                SetAttr(fb, "Name", "Unit_" + u.Id);
                SetAttr(fb, "Number", idx.ToString());
                int idxRepl = 0, timerRepl = 0;
                foreach (var sym in fb.Descendants().Where(e => L(e) == "Symbol").ToList())
                {
                    var comps = sym.Elements().Where(e => L(e) == "Component").ToList();
                    if (comps.Count == 0) continue;
                    foreach (var c in comps)
                    {
                        string n = (string)c.Attribute("Name");
                        string nn = ReplaceToken(n, tpl.UnitId, u.Id);
                        if (nn != n) { c.SetAttributeValue("Name", nn); renamed.Add(SymbolText(comps)); }
                    }
                    string first = (string)comps[0].Attribute("Name");
                    // indeks postaje v globalnih poljih DB_*
                    if (first.StartsWith("DB_", StringComparison.OrdinalIgnoreCase))
                        foreach (var cv in comps.Skip(1).Where(c => (string)c.Attribute("AccessModifier") == "Array")
                                                   .SelectMany(c => c.Elements().Where(e => L(e) == "Access"))
                                                   .SelectMany(a => a.Descendants().Where(e => L(e) == "ConstantValue")))
                            if (cv.Value.Trim() == oldIdx.ToString()) { cv.Value = idx.ToString(); idxRepl++; }
                    // časovniki: "Unit_<id>_DB_Settings".<član vzorca> → .Delay[i]
                    if (first == "Unit_" + u.Id + "_DB_Settings" && comps.Count >= 2 && tplTimers.TryGetValue((string)comps[1].Attribute("Name"), out int ti))
                    {
                        var c = comps[1];
                        var ns = c.Name.Namespace;
                        c.SetAttributeValue("Name", ModelExtractor.TimerArrayName);
                        c.SetAttributeValue("AccessModifier", "Array");
                        c.RemoveNodes();
                        c.Add(new XElement(ns + "Access", new XAttribute("Scope", "LiteralConstant"),
                                  new XElement(ns + "Constant",
                                      new XElement(ns + "ConstantType", "DInt"),
                                      new XElement(ns + "ConstantValue", ti))));
                        timerRepl++;
                    }
                }
                // člani Settings, ki jih FB še uporablja, nov Settings DB pa jih nima (vzorec z dodatnimi parametri)
                var missingSettings = fb.Descendants().Where(e => L(e) == "Symbol")
                    .Select(s => s.Elements().Where(e => L(e) == "Component").Select(c => (string)c.Attribute("Name")).ToList())
                    .Where(c => c.Count >= 2 && c[0] == "Unit_" + u.Id + "_DB_Settings" && c[1] != ModelExtractor.TimerArrayName && c[1] != "Part_n_BAD_Count")
                    .Select(c => c[1]).Distinct().ToList();
                ReplaceTexts(fb, tpl.UnitId, u.Id);
                string fbPath = Path.Combine(dir, "Unit_" + u.Id + ".xml");
                Save(fb, fbPath); res.Files.Add(fbPath);

                // instanca
                var idb = new XDocument(idbTpl);
                SetAttr(idb, "Name", "Unit_" + u.Id + "_DB");
                SetAttr(idb, "Number", idx.ToString());
                SetAttr(idb, "InstanceOfName", "Unit_" + u.Id);
                ReplaceTexts(idb, tpl.UnitId, u.Id);
                string idbPath = Path.Combine(dir, "Unit_" + u.Id + "_DB.xml");
                Save(idb, idbPath); res.Files.Add(idbPath);

                // Settings DB
                var set = BuildSettings(setTpl, "Unit_" + u.Id + "_DB_Settings", u.SettingsDb?.Number ?? 100 + idx);
                ReplaceTexts(set, tpl.UnitId, u.Id);
                string setPath = Path.Combine(dir, "Unit_" + u.Id + "_DB_Settings.xml");
                Save(set, setPath); res.Files.Add(setPath);
                res.Units++;

                // poročilo
                res.Report.Add("== " + u.Id + "  (" + Txt(u) + ")   PLC indeks " + idx + ", HMI " + (idx - 1));
                res.Report.Add("   FB" + idx + " Unit_" + u.Id + ",  DB" + idx + " Unit_" + u.Id + "_DB,  DB" + (u.SettingsDb?.Number ?? 100 + idx) + " " + u.SettingsDb?.Name);
                res.Report.Add("   zamenjav indeksa " + oldIdx + " → " + idx + ": " + idxRepl + ",  časovnikov → Delay[i]: " + timerRepl);
                foreach (var (kind, num) in new[] { ("FB", idx), ("DB", idx), ("DB", u.SettingsDb?.Number ?? 100 + idx) })
                    if (existingNumbers.TryGetValue(kind + num, out var who) && who != "Unit_" + u.Id && who != "Unit_" + u.Id + "_DB" && who != u.SettingsDb?.Name)
                        res.Report.Add("   ⚠ " + kind + num + " v projektu že obstaja: " + who + " – pred uvozom spremeni številko.");
                foreach (var ms in missingSettings)
                    res.Report.Add("   ⚠ FB uporablja Unit_" + u.Id + "_DB_Settings." + ms + ", ki ga nov Settings DB nima – dodaj člana ali popravi FB.");
                var need = renamed.Where(r => !r.StartsWith("Unit_" + u.Id)).ToList();
                foreach (var r in need)
                {
                    bool isAlarm = r.StartsWith(m.Machine.DbMap.Names.TryGetValue("errorMessages", out var em) ? em + "." : "DB_Error_Messages.");
                    string bit = r.Contains('.') ? r.Substring(r.IndexOf('.') + 1) : r;
                    string state = isAlarm ? (alarmBits.Contains(bit) ? "obstaja" : "DODAJ v DB_Error_Messages (+ HMI alarm)")
                                 : r.Contains('.') ? "preveri, da obstaja" : "DODAJ PLC tag (naslov izberi ročno)";
                    res.Report.Add("   potreben simbol: " + r + "  – " + state);
                }
                _log("PLC ogrodje " + u.Id + ": " + dir);
            }

            res.Report.Add("");
            res.Report.Add("Enot: " + res.Units + ", datotek: " + res.Files.Count + ".");
            res.Report.Add("Simboli, ki jih vzorec uporablja nespremenjene (sektorske/strojne, npr. A00_ELM_*, START_ZONE_1), so v FB ostali kot v vzorcu – preveri jih za ciljni sektor.");
            res.Report.Add("Uvoz: SimaticML (V20) – prek TIA Openness (Import v PlcBlockGroup) ali Version Control Interface; mape ustrezajo Program blocks/Machine/…");
            File.WriteAllLines(Path.Combine(outDir, "PLC_ogrodje_porocilo.txt"), res.Report, new UTF8Encoding(true));
            return res;
        }

        static string Txt(Unit u) => u.Text != null && u.Text.TryGetValue("sl-SI", out var s) && s.Length > 0 ? s : "brez opisa";

        static string SymbolText(List<XElement> comps) => string.Join(".", comps.Select(c => (string)c.Attribute("Name") +
            ((string)c.Attribute("AccessModifier") == "Array" ? "[" + string.Join(",", c.Descendants().Where(e => L(e) == "ConstantValue").Select(e => e.Value)) + "]" : "")));

        /// <summary>Zamenja oznako enote kot samostojno besedo, ločeno s '_' ali robom ("Warning_OFF_A05" → "Warning_OFF_B03", "Unit_A05_DB" → "Unit_B03_DB").</summary>
        public static string ReplaceToken(string s, string oldId, string newId) =>
            s == null ? null : Regex.Replace(s, "(?<=^|_)" + Regex.Escape(oldId) + "(?=_|$)", newId);

        static void ReplaceTexts(XDocument doc, string oldId, string newId)
        {
            foreach (var t in doc.Descendants().Where(e => L(e) == "Text" && !e.HasElements))
                t.Value = Regex.Replace(t.Value, "(?<![A-Za-z0-9])" + Regex.Escape(oldId) + "(?![0-9])", newId);
        }

        static void SetAttr(XDocument doc, string name, string value)
        {
            var blk = doc.Root.Elements().First(e => L(e).StartsWith("SW.Blocks."));
            var el = blk.Elements().First(e => L(e) == "AttributeList").Elements().FirstOrDefault(e => L(e) == name);
            if (el != null) el.Value = value;
        }

        /// <summary>Indeks vzorca: najpogostejša konstanta v DB_Service_Settings.&lt;polje&gt;[n] (A05 → 15).</summary>
        static int TemplateIndex(XDocument fb)
        {
            var counts = new Dictionary<int, int>();
            foreach (var sym in fb.Descendants().Where(e => L(e) == "Symbol"))
            {
                var comps = sym.Elements().Where(e => L(e) == "Component").ToList();
                if (comps.Count < 2 || !((string)comps[0].Attribute("Name")).Equals("DB_Service_Settings", StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var cv in comps[1].Descendants().Where(e => L(e) == "ConstantValue"))
                    if (int.TryParse(cv.Value.Trim(), out int v)) counts[v] = counts.TryGetValue(v, out int c) ? c + 1 : 1;
            }
            if (counts.Count == 0) throw new InvalidOperationException("Vzorčni FB ne uporablja DB_Service_Settings.<polje>[n] – indeksa ni mogoče določiti.");
            return counts.OrderByDescending(k => k.Value).First().Key;
        }

        /// <summary>Člani S5Time vzorčnega Settings DB-ja po vrsti → indeks v Delay[0..9].</summary>
        static Dictionary<string, int> TemplateTimers(XDocument set)
        {
            var res = new Dictionary<string, int>(StringComparer.Ordinal);
            var stat = set.Descendants().FirstOrDefault(e => L(e) == "Section" && (string)e.Attribute("Name") == "Static");
            int i = 0;
            if (stat != null)
                foreach (var mem in stat.Elements().Where(e => L(e) == "Member"))
                    if (((string)mem.Attribute("Datatype") ?? "").Equals("S5Time", StringComparison.OrdinalIgnoreCase) && i < 10)
                        res[(string)mem.Attribute("Name")] = i++;
            return res;
        }

        /// <summary>Settings DB po standardu 4.3: ogrodje vzorčnega DB-ja (atributi, komentarji), člani zamenjani z Delay[0..9] + Part_n_BAD_Count.</summary>
        static XDocument BuildSettings(XDocument tpl, string name, int number)
        {
            var doc = new XDocument(tpl);
            SetAttr(doc, "Name", name);
            SetAttr(doc, "Number", number.ToString());
            var stat = doc.Descendants().First(e => L(e) == "Section" && (string)e.Attribute("Name") == "Static");
            var ns = stat.Name.Namespace;
            var proto = stat.Elements().FirstOrDefault(e => L(e) == "Member");
            XElement Attrs() => proto?.Elements().FirstOrDefault(e => L(e) == "AttributeList") is XElement a ? new XElement(a) : null;
            stat.RemoveNodes();
            var delay = new XElement(ns + "Member", new XAttribute("Name", ModelExtractor.TimerArrayName), new XAttribute("Datatype", "Array[0..9] of S5Time"),
                                     new XAttribute("Remanence", "Retain"), new XAttribute("Accessibility", "Public"), Attrs());
            for (int i = 0; i < 10; i++)
                delay.Add(new XElement(ns + "Subelement", new XAttribute("Path", i.ToString()), new XElement(ns + "StartValue", "S5T#0MS")));
            stat.Add(delay);
            stat.Add(new XElement(ns + "Member", new XAttribute("Name", "Part_n_BAD_Count"), new XAttribute("Datatype", "Int"),
                                  new XAttribute("Remanence", "Retain"), new XAttribute("Accessibility", "Public"), Attrs(), new XElement(ns + "StartValue", "3")));
            return doc;
        }

        /// <summary>Številke obstoječih blokov v exportu modela ("FB11" → "Unit_A01") – za opozorilo o trku številk.</summary>
        static Dictionary<string, string> ExistingNumbers(string exportDir)
        {
            var res = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(exportDir) || !Directory.Exists(Path.Combine(exportDir, "PLC"))) return res;
            var kindRx = new Regex(@"<SW\.Blocks\.(FB|FC|OB|GlobalDB|InstanceDB|ArrayDB)\b");
            var nameRx = new Regex(@"<Name>([^<]*)</Name>\s*<(?:Namespace|Number)");
            var numRx = new Regex(@"<Number>(\d+)</Number>");
            foreach (var f in Directory.EnumerateFiles(Path.Combine(exportDir, "PLC"), "*.xml", SearchOption.AllDirectories)
                                       .Where(f => f.IndexOf(Path.DirectorySeparatorChar + "Blocks" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                string x = File.ReadAllText(f);
                var k = kindRx.Match(x); var n = numRx.Match(x);
                if (!k.Success || !n.Success) continue;
                string kind = k.Groups[1].Value.EndsWith("DB") ? "DB" : k.Groups[1].Value;
                res[kind + n.Groups[1].Value] = Path.GetFileNameWithoutExtension(f);
            }
            return res;
        }

        static void Save(XDocument doc, string path)
        {
            using (var w = new StreamWriter(path, false, new UTF8Encoding(true)))
                doc.Save(w, SaveOptions.DisableFormatting);
        }
    }
}
