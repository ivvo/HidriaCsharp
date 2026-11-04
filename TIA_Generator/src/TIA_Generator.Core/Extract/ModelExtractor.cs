using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TIA_Generator.Model;
using TIA_ToolKit.Analyzer;

namespace TIA_Generator.Extract
{
    /// <summary>
    /// Faza 1 generatorja: TIA export ("Export all") → model stroja (HMI_GENERATOR_SPEC.md pogl. 3.1 in 4.1).
    /// Branje exporta in odmiki DB-jev: TIA_ToolKit.Analyzer (ExportReader, DbLayout).
    /// </summary>
    public sealed class ModelExtractor
    {
        readonly TextDictionary _dict;
        readonly Action<string> _log;
        MachineModelFile _m;
        PlcProgram _plc;

        public ModelExtractor(TextDictionary dict, Action<string> log = null)
        {
            _dict = dict;
            _log = log ?? (s => { });
        }

        // Oznaka enote v imenu FB "Unit_<oznaka>": Purem A01, B02_1; BMW A01_R, AT1, B00; 122A 00, T20; PHEV Stator 010; linija R_G10.
        static readonly Regex UnitFbRx = new Regex(@"^Unit_((?:[A-Z]_)?[A-Z]{0,2}\d{1,3}(?:_(?:\d+|[A-Z]))?)$");
        static readonly Regex PartRx = new Regex(@"^([A-E])\d{2}");
        static readonly Regex SubRx = new Regex(@"^(.+)_(\d+)$");
        static readonly Regex LineRx = new Regex(@"^([A-Z])_[A-Z]");
        // F/B = naprej/nazaj; O/C = odpri/zapri (prijemala, BMW G) – O se obravnava kot "naprej", C kot "nazaj"
        static readonly Regex CylRx = new Regex(@"^(.+)_(F_CMD|B_CMD|O_CMD|C_CMD|F|B|O|C)$");
        static readonly Regex ArrayRx = new Regex(@"^Array\s*\[\s*(-?\d+)\s*\.\.\s*(-?\d+)\s*\]\s+of\s+(.+)$", RegexOptions.IgnoreCase);

        /// <summary>Razponi PLC indeksov po delih stroja (standard 2.0, pogl. 5.3).</summary>
        static readonly Dictionary<string, (int lo, int hi)> PartRanges = new Dictionary<string, (int, int)>
        {
            ["A"] = (10, 19), ["B"] = (20, 39), ["C"] = (40, 59), ["D"] = (60, 79), ["E"] = (80, 99),
        };

        public MachineModelFile Extract(string exportDir)
        {
            var projects = ExportReader.FindProjects(exportDir);
            if (projects.Count == 0) throw new InvalidOperationException("V mapi ni exporta (manifest.json ali PLC\\): " + exportDir);
            if (projects.Count > 1)
                throw new InvalidOperationException("V mapi je več exportov – podaj enega:\n  " + string.Join("\n  ", projects));
            string dir = projects[0];
            _log("Berem export " + dir + " ...");
            var project = ExportReader.Read(dir, _log);

            _m = new MachineModelFile
            {
                Generator = "TIA_Generator " + typeof(ModelExtractor).Assembly.GetName().Version,
                GeneratedAt = DateTime.Now.ToString("s", CultureInfo.InvariantCulture),
                SourceExport = dir,
            };
            _m.Machine.Name = project.Name;
            _m.Machine.Project = ProjectNumber(dir);

            // PLC z največ enotami (projekti imajo praviloma en PLC)
            _plc = project.Plcs.OrderByDescending(p => p.Blocks.Count(b => b.Kind == BlockKind.FB && UnitFbRx.IsMatch(b.Name))).FirstOrDefault();
            if (_plc == null) throw new InvalidOperationException("Export nima PLC programa.");
            _m.Machine.Plc.Name = _plc.Name;
            _m.Machine.Plc.Source = "mapa PLC\\" + _plc.Name;

            ReadHmiDevices(dir, project);
            var roles = FindRoleDbs();
            ReadUnits(roles);
            ReadServiceSettings(roles.service);
            ReadCylinders(roles.cylinder);
            ReadRecipe(roles.recipe);
            ReadAlarms(roles.errors);
            Finish();
            return _m;
        }

        static string ProjectNumber(string dir)
        {
            var m = Regex.Match(dir, @"\d{4}-\d{3}[A-Z]");
            return m.Success ? m.Value : null;
        }

        void Warn(string unit, string code, string msg) => _m.Warnings.Add(new ModelWarning { Unit = unit, Code = code, Message = msg });

        // ------------------------------------------------------------------ HMI naprave

        void ReadHmiDevices(string dir, ExportProject project)
        {
            string manifest = Path.Combine(dir, "manifest.json");
            var devices = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(manifest))
            {
                try
                {
                    var j = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(manifest));
                    if (j["hmi"] is Newtonsoft.Json.Linq.JArray arr)
                        foreach (var h in arr) devices[(string)h["name"] ?? ""] = (string)h["device"];
                }
                catch (Exception ex) { _log("  manifest.json: " + ex.Message); }
            }
            foreach (var root in project.HmiRoots)
            {
                string name = Path.GetFileName(root);
                devices.TryGetValue(name, out var dev);
                bool pc = (dev ?? "").IndexOf("PC", StringComparison.OrdinalIgnoreCase) >= 0;
                string conn = null;
                var connDir = Path.Combine(root, "Connections");
                if (Directory.Exists(connDir))
                {
                    var f = Directory.GetFiles(connDir, "*.xml").FirstOrDefault();
                    if (f != null) conn = Path.GetFileNameWithoutExtension(f);
                }
                _m.Machine.HmiDevices.Add(new HmiDevice
                {
                    Kind = pc ? "PC" : "Panel",
                    Target = name,
                    Resolution = pc ? "1920x1080" : "800x480",
                    Connection = conn ?? (pc ? "HMI_Connection" : "HMI_Connection_1"),
                    Source = "manifest.json (device: " + (dev ?? "?") + "), Connections\\" + (conn ?? "-") + "; ločljivost privzeta po standardu – preveri",
                });
            }
        }

        // ------------------------------------------------------------------ vloge DB-jev

        (Block service, Block errors, Block cylinder, Block recipe) FindRoleDbs()
        {
            var dbs = _plc.Blocks.Where(b => b.Kind == BlockKind.GlobalDB).ToList();
            Block ByName(params string[] names) =>
                names.Select(n => dbs.FirstOrDefault(b => string.Equals(b.Name, n, StringComparison.OrdinalIgnoreCase))).FirstOrDefault(b => b != null);

            var service = ByName("DB_Service_Settings")
                          ?? dbs.FirstOrDefault(b => TopMembers(b).Any(m => m.name == "OFF" && ArrayRx.IsMatch(m.type)) && TopMembers(b).Any(m => m.name == "TEST"));
            var errors = ByName("DB_Error_Messages") ?? dbs.FirstOrDefault(b => b.Name.IndexOf("Error_Messages", StringComparison.OrdinalIgnoreCase) >= 0);
            var cyl = ByName("CYLINDER_CONTROL_DB", "Cylinder_Control") ?? dbs.FirstOrDefault(b => b.Name.IndexOf("Cylinder_Control", StringComparison.OrdinalIgnoreCase) >= 0);
            var recipe = ByName("Recipe");
            var takt = ByName("DB_Takt");
            var counters = ByName("DB_Counter_GOOD", "DB_Counter_Common");

            void Set(string role, Block b, Action<string> setter)
            {
                if (b == null) { Warn(null, "DB-MISSING", "Ni najden DB za vlogo '" + role + "'."); return; }
                setter("DB" + b.Number);
                _m.Machine.DbMap.Names[role] = b.Name;
            }
            var map = _m.Machine.DbMap;
            Set("serviceSettings", service, v => map.ServiceSettings = v);
            Set("errorMessages", errors, v => map.ErrorMessages = v);
            Set("cylinderControl", cyl, v => map.CylinderControl = v);
            Set("recipe", recipe, v => map.Recipe = v);
            Set("takt", takt, v => map.Takt = v);
            Set("counters", counters, v => map.Counters = v);
            return (service, errors, cyl, recipe);
        }

        // ------------------------------------------------------------------ člani DB-ja

        sealed class DbMember
        {
            public string Path, Name, Type, Comment;
            public bool IsStructMember;
        }

        static string L(XElement e) => e.Name.LocalName;

        static IEnumerable<(string name, string type)> TopMembers(Block db)
        {
            if (db?.Interface == null) yield break;
            foreach (var sec in db.Interface.Elements().Where(x => L(x) == "Section"))
                foreach (var m in sec.Elements().Where(x => L(x) == "Member"))
                    yield return ((string)m.Attribute("Name"), ((string)m.Attribute("Datatype") ?? "").Trim());
        }

        /// <summary>Listi DB-ja (struct/UDT razširjeni s piko, polja kot en član). Samo razdelki Static/Input/Output/InOut.</summary>
        List<DbMember> Leaves(Block db)
        {
            var list = new List<DbMember>();
            if (db?.Interface == null) return list;
            void Walk(IEnumerable<XElement> members, string prefix)
            {
                foreach (var m in members)
                {
                    string name = (string)m.Attribute("Name");
                    string type = ((string)m.Attribute("Datatype") ?? "").Trim();
                    string path = prefix.Length == 0 ? name : prefix + "." + name;
                    var nested = m.Elements().FirstOrDefault(x => L(x) == "Sections");
                    var direct = m.Elements().Where(x => L(x) == "Member").ToList();
                    if (!ArrayRx.IsMatch(type) && (nested != null || direct.Count > 0))
                    {
                        if (nested != null) foreach (var s in nested.Elements().Where(x => L(x) == "Section")) Walk(s.Elements().Where(x => L(x) == "Member"), path);
                        else Walk(direct, path);
                        continue;
                    }
                    if (type.StartsWith("\"") && !ArrayRx.IsMatch(type) && _plc.Types.TryGetValue(type.Trim('"'), out var udt))
                    {
                        foreach (var s in udt.Elements().Where(x => L(x) == "Section")) Walk(s.Elements().Where(x => L(x) == "Member"), path);
                        continue;
                    }
                    list.Add(new DbMember { Path = path, Name = name, Type = type, Comment = CommentOf(m), IsStructMember = prefix.Length > 0 });
                }
            }
            foreach (var sec in db.Interface.Elements().Where(x => L(x) == "Section"))
            {
                string sn = (string)sec.Attribute("Name");
                if (sn == "Temp" || sn == "Constant" || sn == "Return") continue;
                Walk(sec.Elements().Where(x => L(x) == "Member"), "");
            }
            return list;
        }

        static string CommentOf(XElement member)
        {
            var c = member.Elements().FirstOrDefault(x => L(x) == "Comment");
            if (c == null) return null;
            var texts = c.Elements().Where(x => L(x) == "MultiLanguageText").ToList();
            var sl = texts.FirstOrDefault(t => ((string)t.Attribute("Lang") ?? "").StartsWith("sl")) ?? texts.FirstOrDefault();
            string v = sl?.Value?.Trim();
            return string.IsNullOrEmpty(v) ? null : v;
        }

        Dictionary<string, int> BitOffsets(Block db)
        {
            var res = new Dictionary<string, int>(StringComparer.Ordinal);
            var layout = db == null ? null : DbLayout.For(_plc, db);
            if (layout == null) return res;
            foreach (var leaf in layout.Leaves.Where(x => x.IsBool && layout.IsCertain(x.Byte)))
                res[leaf.Path] = leaf.Byte * 8 + leaf.Bit;
            return res;
        }

        // ------------------------------------------------------------------ enote

        void ReadUnits((Block service, Block errors, Block cylinder, Block recipe) roles)
        {
            // enota = FB "Unit_<oznaka>" z oznako po vzorcu ALI s pripadajočim "Unit_<oznaka>_DB_Settings" (BMW G: Unit_R_G10_EQ_1)
            var settingsNames = new HashSet<string>(_plc.Blocks.Where(b => b.Kind == BlockKind.GlobalDB && b.Name.EndsWith("_DB_Settings")).Select(b => b.Name));
            var fbs = _plc.Blocks.Where(b => (b.Kind == BlockKind.FB || b.Kind == BlockKind.FC) && b.Name.StartsWith("Unit_")
                                             && (UnitFbRx.IsMatch(b.Name) || settingsNames.Contains(b.Name + "_DB_Settings")))
                                 .GroupBy(b => b.Name).Select(g => g.First())
                                 .OrderBy(b => b.Name, StringComparer.Ordinal).ToList();
            string ssName = roles.service?.Name ?? "DB_Service_Settings";
            var idxRx = new Regex("\"?" + Regex.Escape(ssName) + "\"?\\.(\\w+)\\[(\\d+)\\]");

            foreach (var fb in fbs)
            {
                string id = fb.Name.Substring("Unit_".Length);
                var u = new Unit { Id = id, Fb = fb.Name, Folder = fb.Folder };
                var fno = Regex.Match((fb.Folder ?? "").Split('/').LastOrDefault() ?? "", @"^Unit\s+(\d+)\s");
                if (fno.Success) u.FolderNo = int.Parse(fno.Groups[1].Value);

                // del stroja: mapa Machine/<črka>/ (zanesljivo); sicer se odloči v drugem prehodu (glej spodaj)
                var seg = (fb.Folder ?? "").Split('/');
                if (seg.Length >= 2 && seg[0] == "Machine" && seg[1].Length == 1) u.Part = seg[1];
                else if (seg.Length >= 3 && seg[0] == "Machine") u.Group = seg[1];
                var sm = SubRx.Match(id);
                if (sm.Success) { u.Parent = sm.Groups[1].Value; u.SubIndex = int.Parse(sm.Groups[2].Value); }
                var lm = LineRx.Match(id);
                if (lm.Success) u.Line = lm.Groups[1].Value;

                // opis iz imena mape: "B02_1 - Krimpanje 1" → "Krimpanje 1"
                string last = (fb.Folder ?? "").Split('/').LastOrDefault() ?? "";
                int dash = last.IndexOf(" - ", StringComparison.Ordinal);
                string desc = dash >= 0 ? last.Substring(dash + 3).Trim() : last.Replace('_', ' ').Trim();
                if (desc.Length == 0) desc = id;
                u.Text = new LangText(desc, "", "");
                u.TextSource = dash >= 0 ? "sl: ime mape; en/de: " + TextSource.Missing : TextSource.Missing;

                ResolveIndex(u, fb, idxRx, ssName);
                ReadSettings(u);
                foreach (var d in _m.Machine.HmiDevices.Select(d => d.Kind).Distinct()) u.ShowOn.Add(d);
                _m.Units.Add(u);
            }

            // brez map po delih: del iz oznake (A01 → A). Če mape po delih obstajajo, enote v drugih mapah (Machine/Dosing/D01) niso del.
            bool folderParts = _m.Units.Any(u => u.Part != null);
            if (!folderParts)
                foreach (var u in _m.Units)
                {
                    var pm = PartRx.Match(u.Id);
                    if (pm.Success) u.Part = pm.Groups[1].Value;
                }
            _m.Machine.Parts = _m.Units.Where(u => u.Part != null).Select(u => u.Part).Distinct().OrderBy(x => x).ToList();
            _m.Machine.Lines = _m.Units.Where(u => u.Line != null).Select(u => u.Line).Distinct().OrderBy(x => x).ToList();
            _m.Machine.Type = _m.Units.Any(u => u.Part != null && PartRanges.ContainsKey(u.Part)) ? "parts" : "single";
            CheckIndexRanges();
            _log("Enot: " + _m.Units.Count);
        }

        void ResolveIndex(Unit u, Block fb, Regex idxRx, string ssName)
        {
            var uses = new List<(string fam, int idx)>();
            foreach (var nw in fb.Networks)
                foreach (var r in nw.Refs)
                    foreach (Match m in idxRx.Matches(r)) uses.Add((m.Groups[1].Value, int.Parse(m.Groups[2].Value)));
            if (uses.Count == 0 && fb.SclSource != null)
                foreach (Match m in idxRx.Matches(fb.SclSource)) uses.Add((m.Groups[1].Value, int.Parse(m.Groups[2].Value)));

            if (uses.Count == 0)
            {
                u.IndexSource = "FB enote ne uporablja polj DB_Service_Settings z indeksom – indeks določi ročno";
                Warn(u.Id, "IDX-NONE", "Indeksa postaje ni mogoče določiti (ni dostopov OFF[n]/TEST[n] …).");
                return;
            }
            // Lastni indeks postaje = indeks z največ različnimi funkcijami. OFF/ENABLE_HMI_COMMAND/RESET_OFF štejejo pol, ker jih
            // podpostaje (A01_1, D03 …) pogosto berejo z indeksom nadrejene postaje (OFF[11]), lastna stanja (ERROR, MOVING, END …) pa pišejo s svojim.
            var byIdx = uses.GroupBy(x => x.idx).Select(g => (idx: g.Key, n: g.Count(),
                                score: g.Select(x => x.fam).Distinct().Sum(f => InheritedFams.Contains(f) ? 0.5 : 1.0),
                                fams: new HashSet<string>(g.Select(x => x.fam))))
                            .OrderByDescending(x => x.score).ThenByDescending(x => x.n).ThenBy(x => x.idx).ToList();
            var main = byIdx[0];
            u.PlcIndex = main.idx;
            u.HmiIndex = main.idx - 1;
            u.StationFunctions = main.fams.OrderBy(x => x).ToList();
            u.IndexSource = "indeks z največ funkcijami " + ssName + " v " + fb.Name + " (" + main.fams.Count + " funkcij, " + main.n + "×); HMI = PLC − 1";
            var others = new List<string>();
            foreach (var o in byIdx.Skip(1))
            {
                if (o.fams.All(f => InheritedFams.Contains(f)))
                {
                    u.InheritsOffFromIndex = o.idx;
                    u.IndexSource += "; " + string.Join("/", o.fams.OrderBy(x => x)) + " bere z indeksom " + o.idx + " (nadrejena postaja)";
                }
                else others.Add(o.idx + " (" + string.Join(",", o.fams.OrderBy(x => x)) + ")");
            }
            if (others.Count > 0)
                Warn(u.Id, "IDX-MIXED", "FB uporablja tudi druge indekse: " + string.Join("; ", others) +
                                        " – preveri (predaja sosednji postaji ali napaka kopiranja, pravilo IDX-UNIT).");
        }

        static readonly HashSet<string> InheritedFams = new HashSet<string> { "OFF", "ENABLE_HMI_COMMAND", "RESET_OFF", "STEP" };

        void CheckIndexRanges()
        {
            foreach (var u in _m.Units.Where(x => x.PlcIndex.HasValue))
            {
                int i = u.PlcIndex.Value;
                if (_m.Machine.Type == "parts" && u.Part != null && PartRanges.TryGetValue(u.Part, out var r) && (i < r.lo || i > r.hi))
                    Warn(u.Id, "IDX-RANGE", "PLC indeks " + i + " je izven razpona standarda 2.0 za del " + u.Part + " (" + r.lo + "–" + r.hi + ").");
                else if (i < 1 || i > 99)
                    Warn(u.Id, "IDX-RANGE", "PLC indeks " + i + " je izven polja [1..99].");
            }
            foreach (var g in _m.Units.Where(x => x.PlcIndex.HasValue).GroupBy(x => x.PlcIndex.Value).Where(g => g.Count() > 1))
                Warn(string.Join(", ", g.Select(x => x.Id)), "IDX-DUP", "Več enot uporablja isti PLC indeks " + g.Key + ".");
        }

        // ------------------------------------------------------------------ Settings DB: časovniki, parametri

        static readonly Regex DisplayRx = new Regex(@"(^|_)(ACT|ACTUAL|Archive|Arh|Output|Readed|Status|Live|Counter|Count_Act)(_|$|\d)", RegexOptions.IgnoreCase);

        void ReadSettings(Unit u)
        {
            string dbName = "Unit_" + u.Id + "_DB_Settings";
            var db = _plc.Blocks.FirstOrDefault(b => b.Kind == BlockKind.GlobalDB && b.Name == dbName);
            if (db == null)
            {
                Warn(u.Id, "SETTINGS-MISSING", "Ni DB-ja " + dbName + " – brez časovnikov in parametrov.");
                return;
            }
            u.SettingsDb = new SettingsDb { Name = db.Name, Number = db.Number, Layout = db.MemoryLayout ?? "?" };
            var leaves = Leaves(db);
            string q = "\"" + db.Name + "\"";

            // časovniki: standard 2.0 = Array[0..9] of S5Time na začetku; stari projekti = posamezni člani S5Time
            var timerArray = leaves.FirstOrDefault(l => { var a = ArrayRx.Match(l.Type); return a.Success && a.Groups[3].Value.Trim().Equals("S5Time", StringComparison.OrdinalIgnoreCase); });
            var used = new HashSet<DbMember>();
            if (timerArray != null)
            {
                u.TimerLayout = "array";
                var a = ArrayRx.Match(timerArray.Type);
                int lo = int.Parse(a.Groups[1].Value), hi = int.Parse(a.Groups[2].Value);
                for (int i = lo; i <= hi; i++)
                {
                    u.Timers.Add(new TimerDef { Index = i - lo, Member = timerArray.Path + "[" + i + "]", Show = true, Text = new LangText(), TextSource = TextSource.Missing });
                }
                used.Add(timerArray);
                u.HmiTags.Timers = new HmiTagDef
                {
                    Pc = u.Id + "_Timers", Panel = "Unit_" + u.Id + "_Timers",
                    PlcTag = q + "." + timerArray.Path, DataType = timerArray.Type,
                };
                if (!string.IsNullOrEmpty(timerArray.Comment)) u.HmiTags.Timers.Note = "komentar polja: " + timerArray.Comment;
            }
            else
            {
                var s5 = leaves.TakeWhile(l => l.Type.Equals("S5Time", StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
                if (s5.Count == 0) s5 = leaves.Where(l => l.Type.Equals("S5Time", StringComparison.OrdinalIgnoreCase)).Take(10).ToList();
                if (s5.Count > 0)
                {
                    // Odločitev 2026-11-04: v PLC se člani zamenjajo s poljem "Delay : Array[0..9] of S5Time" (standard 4.3),
                    // HMI tag je simbolni na to polje. Model nosi preslikavo Member → Delay[i] in bloke, ki jih je treba popraviti.
                    u.TimerLayout = "members";
                    for (int i = 0; i < s5.Count; i++)
                    {
                        bool spare = s5[i].Name.IndexOf("Spare", StringComparison.OrdinalIgnoreCase) >= 0;
                        var (t, src) = TextFor(s5[i]);
                        u.Timers.Add(new TimerDef
                        {
                            Index = i, Member = s5[i].Path, ArrayElement = TimerArrayName + "[" + i + "]",
                            UsedIn = UsagesOf(db.Name, s5[i].Path),
                            Show = !spare, Text = spare ? null : t, TextSource = spare ? null : src,
                        });
                        used.Add(s5[i]);
                    }
                    bool atStart = leaves.Count > 0 && ReferenceEquals(leaves[0], s5[0]);
                    u.HmiTags.Timers = new HmiTagDef
                    {
                        Pc = u.Id + "_Timers", Panel = "Unit_" + u.Id + "_Timers",
                        PlcTag = q + "." + TimerArrayName, DataType = "Array[0..9] of S5Time",
                        RequiresPlcChange = true,
                        Note = "V PLC zamenjaj " + s5.Count + " članov S5Time v " + db.Name + " s poljem '" + TimerArrayName + " : Array[0..9] of S5Time'" +
                               (atStart ? " na začetku DB-ja" : " (člani niso na začetku DB-ja – polje postavi na začetek)") +
                               " in v blokih iz timers[].usedIn preimenuj člane v " + TimerArrayName + "[i]. Seznam: TIA_Generator plcchanges.",
                    };
                    var blocks = u.Timers.SelectMany(x => x.UsedIn).Select(BlockOfUsage).Distinct().ToList();
                    Warn(u.Id, "TIMERS-NOT-ARRAY", "Časovniki v " + db.Name + " niso polje (" + s5.Count + " članov S5Time) – potrebna sprememba PLC v " +
                                                   TimerArrayName + "[0..9]; uporabljeni v " + blocks.Count + " blokih.");
                    // časovnik enote uporablja FB druge enote – pogosto ostanek kopiranja (A01_1 bere Unit_A01_DB_Settings.Delay_Position_Error)
                    foreach (var t in u.Timers)
                        foreach (var use in t.UsedIn.Where(x => !BlockOfUsage(x).EndsWith("/" + u.Fb) && BlockOfUsage(x) != u.Fb
                                                                 && Regex.IsMatch(BlockOfUsage(x), @"(^|/)Unit_[^/]+$")))
                            Warn(u.Id, "TIMER-FOREIGN", db.Name + "." + t.Member + " uporablja " + use + " (FB druge enote) – preveri, ali ni napaka kopiranja.");
                }
                else Warn(u.Id, "TIMERS-NONE", "V " + db.Name + " ni članov S5Time.");
            }

            var pbc = leaves.FirstOrDefault(l => l.Name.Equals("Part_n_BAD_Count", StringComparison.OrdinalIgnoreCase));
            if (pbc != null)
            {
                used.Add(pbc);
                u.PartBadCount = new PartBadCount { Member = pbc.Path, DataType = pbc.Type, Text = _dict.Lookup("Part_n_BAD_Count").text };
            }

            foreach (var l in leaves.Where(x => !used.Contains(x)))
            {
                if (l.Type.Equals("S5Time", StringComparison.OrdinalIgnoreCase) && l.Name.IndexOf("Spare", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                var (t, src) = TextFor(l);
                u.Parameters.Add(new ParamDef
                {
                    Member = l.Path, DataType = l.Type,
                    Access = DisplayRx.IsMatch(l.Path) ? "display" : "input",
                    PlcTag = q + "." + l.Path, Text = t, TextSource = src,
                });
            }
        }

        /// <summary>Ime polja časovnikov po standardu 2.0 (člani Delay_… → Delay[0..9]).</summary>
        public const string TimerArrayName = "Delay";

        List<(Block block, string text)> _refText;

        /// <summary>"Machine/A/…/Unit_A01 [FB11] NW 3" → "Machine/A/…/Unit_A01" (pot bloka brez številke in networkov).</summary>
        static string BlockOfUsage(string usage)
        {
            int i = usage.IndexOf(" [", StringComparison.Ordinal);
            return i >= 0 ? usage.Substring(0, i) : usage;
        }

        /// <summary>Bloki (z networki), ki uporabljajo "<db>".<member> – STL/LAD/SCL reference in SCL vir.</summary>
        List<string> UsagesOf(string db, string member)
        {
            if (_refText == null)
                _refText = _plc.Blocks.Where(b => !b.IsDb)
                    .Select(b => (b, string.Join("\n", b.Networks.SelectMany(n => n.Refs)) + "\n" + (b.SclSource ?? ""))).ToList();
            var rx = new Regex(Regex.Escape(db) + "\"?\\.\"?" + Regex.Escape(member) + "\\b");
            return _refText.Where(x => rx.IsMatch(x.text)).Select(x =>
            {
                var nws = x.block.Networks.Where(n => n.Refs.Any(r => rx.IsMatch(r))).Select(n => n.Index).ToList();
                return x.block.DisplayPath + (nws.Count > 0 ? " NW " + string.Join(",", nws) : "");
            }).ToList();
        }

        (LangText, string) TextFor(DbMember m)
        {
            if (!string.IsNullOrEmpty(m.Comment)) return (new LangText(m.Comment, "", ""), "komentar (sl); en/de: " + TextSource.Missing);
            if (!m.Path.Contains('.')) { var r = _dict.Lookup(m.Path); return (r.text, r.source); }
            // član strukture: "<besedilo strukture>: <besedilo člana>" (CRIMP_POSITION.Value_Max → "Krimpanje položaj: Maksimalna vrednost")
            int dot = m.Path.LastIndexOf('.');
            var pre = _dict.Lookup(m.Path.Substring(0, dot));
            var leaf = _dict.Lookup(m.Path.Substring(dot + 1));
            var t = new LangText();
            foreach (var k in leaf.text.Keys) t[k] = (pre.text.TryGetValue(k, out var p) ? p : "") + ": " + leaf.text[k];
            string src = pre.source == TextSource.Missing || leaf.source == TextSource.Missing ? TextSource.Missing
                       : pre.source == TextSource.Composed || leaf.source == TextSource.Composed ? TextSource.Composed : TextSource.Dictionary;
            return (t, src);
        }

        // ------------------------------------------------------------------ DB_Service_Settings

        void ReadServiceSettings(Block db)
        {
            if (db == null) return;
            foreach (var (name, type) in TopMembers(db))
            {
                if (ArrayRx.IsMatch(type)) _m.Global.ServiceSettingsArrays.Add(name);
                else if (type.Equals("Bool", StringComparison.OrdinalIgnoreCase)) _m.Global.ServiceSettingsBools.Add(name);
            }
        }

        // ------------------------------------------------------------------ cilindri (DB4000)

        /// <summary>Zapisi oznake enote v imenih članov: Purem "B02_1_", PHEV "U_02_", 122A "U10_", tudi "Unit_10_".</summary>
        static IEnumerable<string> Aliases(Unit u)
        {
            yield return u.Id;
            yield return "U_" + u.Id;
            yield return "U" + u.Id;
            yield return "Unit_" + u.Id;
            // PHEV Stator: mapa "Unit 2 020 - …", v DB4000 "U_02_…"
            if (u.FolderNo.HasValue)
            {
                yield return "U_" + u.FolderNo.Value.ToString("00");
                yield return "U" + u.FolderNo.Value.ToString("00");
            }
        }

        /// <summary>Enota, katere oznaka (ali njen zapis) je predpona imena; vrne tudi dolžino predpone z "_". Najdaljša zmaga (B02_1 pred B02).</summary>
        (Unit unit, int prefixLen) OwnerWithPrefix(string memberName)
        {
            Unit best = null; int len = 0;
            foreach (var u in _m.Units)
                foreach (var a in Aliases(u))
                    if (memberName.StartsWith(a + "_", StringComparison.Ordinal) && a.Length + 1 > len) { best = u; len = a.Length + 1; }
            return (best, len);
        }

        Unit OwnerOf(string memberName) => OwnerWithPrefix(memberName).unit;

        void ReadCylinders(Block db)
        {
            if (db == null) return;
            var leaves = Leaves(db);
            var bits = BitOffsets(db);
            var manual = leaves.FirstOrDefault(l => l.Name.IndexOf("Manual_Control", StringComparison.OrdinalIgnoreCase) >= 0);
            _m.Global.ManualControlEnable = manual?.Path;
            if (manual != null && bits.TryGetValue(manual.Path, out int mb) && mb != 0)
                Warn(null, "CYL-ENABLE-POS", manual.Path + " ni prvi bit " + db.Name + " (bit " + mb + ") – standard 5.1.");

            var groups = new Dictionary<(Unit, string), CylinderDef>();
            var order = new List<(Unit, string)>();
            int orphans = 0;
            foreach (var l in leaves.Where(x => x.Type.Equals("Bool", StringComparison.OrdinalIgnoreCase) && x != manual))
            {
                var cm = CylRx.Match(l.Name);
                if (!cm.Success) continue;
                string baseName = cm.Groups[1].Value, kind = cm.Groups[2].Value;
                var (u, plen) = OwnerWithPrefix(baseName);
                if (u == null) { orphans++; continue; }
                string cyl = baseName.Substring(plen);
                var key = (u, cyl);
                if (!groups.TryGetValue(key, out var c))
                {
                    var (t, src) = _dict.Lookup(cyl);
                    c = new CylinderDef { Name = cyl, Text = t, TextSource = src };
                    groups[key] = c;
                    order.Add(key);
                }
                if (kind.StartsWith("O")) kind = "F" + kind.Substring(1);
                else if (kind == "C" || kind == "C_CMD") kind = "B" + kind.Substring(1);
                switch (kind)
                {
                    case "F_CMD": c.Bits.FwdCmd = l.Path; if (bits.TryGetValue(l.Path, out var bf)) c.ArrayIndex = bf; break;
                    case "B_CMD": c.Bits.BwdCmd = l.Path; if (bits.TryGetValue(l.Path, out var bb)) c.ArrayIndexBwd = bb; break;
                    case "F": c.Bits.Fwd = l.Path; break;
                    case "B": c.Bits.Bwd = l.Path; break;
                }
            }
            foreach (var key in order) key.Item1.Cylinders.Add(groups[key]);
            if (bits.Count > 0)
            {
                _m.Global.CylinderControlArraySize = bits.Values.Max() + 1;
                _m.Global.CylinderControlSource = "HMI tag Cylinder_Control = Array[0.." + bits.Values.Max() + "] of Bool na \"" + db.Name +
                                                  "\" od DBX0.0; indeks elementa = bitni odmik (Standard layout)";
            }
            foreach (var c in groups.Values.Where(c => c.Bits.FwdCmd == null || c.Bits.BwdCmd == null))
                Warn(null, "CYL-INCOMPLETE", "Cilinder " + c.Name + " nima obeh ukaznih bitov (F_CMD/B_CMD).");
            if (orphans > 0) Warn(null, "CYL-ORPHAN", orphans + " bitov v " + db.Name + " ne pripada nobeni enoti (predpona ni oznaka enote).");
            if (bits.Count == 0) Warn(null, "CYL-NOLAYOUT", db.Name + " ni ne-optimiziran (Standard) – indeksov v polju Cylinder_Control ni mogoče izračunati.");
        }

        // ------------------------------------------------------------------ recept (DB2000)

        void ReadRecipe(Block db)
        {
            if (db == null) return;
            string q = "\"" + db.Name + "\"";
            foreach (var l in Leaves(db))
            {
                var (t, src) = _dict.Lookup(l.Path, tok => IsUnitToken(tok));
                var p = new ParamDef { Member = l.Path, DataType = l.Type, Access = "input", Recipe = true, PlcTag = q + "." + l.Path, Text = t, TextSource = src };
                var owner = OwnerOf(l.Path);
                if (owner != null) { owner.RecipeParameters.Add(p); continue; }
                // predpona je nadrejena oznaka (B02_Crimp_Position_Max → B02_1..B02_4)
                var children = _m.Units.Where(u => u.Parent != null && l.Path.StartsWith(u.Parent + "_", StringComparison.Ordinal)).ToList();
                if (children.Count > 0) { foreach (var c in children) c.RecipeParameters.Add(p); continue; }
                _m.Global.RecipeOther.Add(p);
            }
        }

        bool IsUnitToken(string tok) => _m.Units.Any(u => u.Id == tok || u.Parent == tok);

        // ------------------------------------------------------------------ alarmi (DB1005)

        void ReadAlarms(Block db)
        {
            if (db == null) return;
            var bits = BitOffsets(db);
            // (zapis oznake, enota), najdaljši zapis prvi: B02_1 pred B02, U_02 pred 02
            var aliases = _m.Units.SelectMany(u => Aliases(u).Select(a => (alias: a, unit: u)))
                                  .OrderByDescending(x => x.alias.Length)
                                  .Select(x => (x.alias, x.unit, rx: new Regex("(^|_)" + Regex.Escape(x.alias) + "(_|$)"))).ToList();
            foreach (var l in Leaves(db).Where(x => x.Type.Equals("Bool", StringComparison.OrdinalIgnoreCase)))
            {
                string n = l.Name;
                string cls = Regex.IsMatch(n, "^(warn|warin)", RegexOptions.IgnoreCase) ? "Warnings" : "Errors";
                var hit = aliases.FirstOrDefault(x => x.rx.IsMatch(n));
                string unit = hit.unit?.Id;
                var idTokens = unit == null ? new HashSet<string>() : new HashSet<string>(TextDictionary.Tokens(hit.alias), StringComparer.OrdinalIgnoreCase);
                var (t, src) = _dict.Lookup(l.Path, tok => idTokens.Contains(tok) || Regex.IsMatch(tok, "^(error|warning|warinig|err)$", RegexOptions.IgnoreCase));
                if (unit != null) foreach (var k in t.Keys.ToList()) t[k] = unit + ": " + t[k];
                var a = new AlarmDef { Bit = l.Path, Class = cls, Unit = unit, Text = t, TextSource = src };
                if (bits.TryGetValue(l.Path, out int bo)) a.BitOffset = bo;
                _m.Global.Alarms.Add(a);
                if (unit != null) _m.Units.First(u => u.Id == unit).Alarms.Add(l.Path);
            }
        }

        // ------------------------------------------------------------------ statistika

        void Finish() => RecalcStatistics(_m);

        /// <summary>Statistika modela – kliči tudi po ročnem urejanju (GUI), preden model shraniš.</summary>
        public static void RecalcStatistics(MachineModelFile m)
        {
            var s = m.Statistics ?? (m.Statistics = new ModelStatistics());
            s.Units = m.Units.Count;
            s.Timers = m.Units.Sum(u => u.Timers.Count);
            s.TimersShown = m.Units.Sum(u => u.Timers.Count(t => t.Show));
            s.Parameters = m.Units.Sum(u => u.Parameters.Count);
            s.RecipeParameters = m.Units.SelectMany(u => u.RecipeParameters.Select(p => p.Member)).Distinct().Count() + m.Global.RecipeOther.Count;
            s.Cylinders = m.Units.Sum(u => u.Cylinders.Count);
            s.Alarms = m.Global.Alarms.Count;
            var sources = m.Units.SelectMany(u => u.Timers.Where(t => t.Show).Select(t => t.TextSource)
                                  .Concat(u.Parameters.Select(p => p.TextSource))
                                  .Concat(u.Cylinders.Select(c => c.TextSource)))
                          .Concat(m.Global.Alarms.Select(a => a.TextSource)).ToList();
            s.TextsFromDictionary = sources.Count(x => x == TextSource.Dictionary || x == TextSource.Manual);
            s.TextsComposed = sources.Count(x => x == TextSource.Composed);
            s.TextsMissing = sources.Count(x => x != null && x.Contains(TextSource.Missing));
        }
    }
}
