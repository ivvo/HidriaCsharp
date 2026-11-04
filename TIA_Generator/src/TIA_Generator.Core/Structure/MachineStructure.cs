using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TIA_Generator.Extract;
using TIA_Generator.Model;

namespace TIA_Generator.Structure
{
    // Struktura stroja (glavni cilj generatorja): sektorji A, B, C … (ali en sektor), število postaj v sektorju, podpostaje.
    //   Sektorji:   Unit_A01 … Unit_A05, Unit_B01 …      PLC indeks = osnova sektorja + št. postaje (A 10+, B 20+, C 40+, D 60+, E 80+)
    //   En sektor:  Unit_01 … Unit_0n                    PLC indeks = št. postaje (1..n)
    //   Podpostaje: Unit_A01_1, Unit_A01_2 …            PLC indeks = naslednji prosti v razponu sektorja (urejljivo)
    // HMI indeks = PLC indeks − 1 (standard 5.3).

    public sealed class SectorDef
    {
        public string Letter;
        public string Text = "";
        public int StationCount;
        /// <summary>Številke postaj, ki jih v obstoječem stroju ni (npr. Purem B05) – pri obnovi seznama se ne ustvarijo.</summary>
        public List<int> Skip = new List<int>();
    }

    public sealed class StationDef
    {
        public string Id;
        public string Sector;      // "A" … ali "" pri enem sektorju
        public int No;             // številka postaje v sektorju
        public string Parent;      // podpostaja: oznaka nadrejene (A01)
        public int? SubNo;         // podpostaja: 1, 2 …
        public string Text = "";
        public int PlcIndex;       // 0 = dodeli samodejno
        public int SubCount;       // samo glavne postaje
        public bool Existing;      // obstaja v PLC (iz exporta)
        /// <summary>Glavna postaja, ki v PLC ne obstaja, nosi pa podpostaje (Purem B02 za B02_1..4) – ne gre v model/PLC.</summary>
        public bool Virtual;
        public int HmiIndex => PlcIndex - 1;
        public bool IsSub => Parent != null;
    }

    public sealed class StructureDef
    {
        public string MachineName = "Nov stroj";
        public bool Sectored = true;
        public bool Pc = true, Panel = true;
        public List<SectorDef> Sectors = new List<SectorDef>();
        public List<StationDef> Stations = new List<StationDef>();
    }

    public static class StructureBuilder
    {
        /// <summary>Razponi PLC indeksov po sektorjih (standard 2.0, pogl. 5.3). Osnova = lo; postaja n → lo + n.</summary>
        public static readonly IReadOnlyDictionary<string, (int lo, int hi)> Ranges = new Dictionary<string, (int, int)>
        {
            ["A"] = (10, 19), ["B"] = (20, 39), ["C"] = (40, 59), ["D"] = (60, 79), ["E"] = (80, 99),
        };
        public const int SingleMax = 99;
        public static readonly string[] SectorLetters = { "A", "B", "C", "D", "E" };

        static readonly Regex SectoredId = new Regex(@"^([A-E])(\d{2})(?:_(\d+))?$");
        static readonly Regex SingleId = new Regex(@"^(\d{2})(?:_(\d+))?$");

        public static string StationId(bool sectored, string sector, int no) => (sectored ? sector : "") + no.ToString("00");

        static (int lo, int hi) RangeOf(StructureDef d, string sector) =>
            d.Sectored && Ranges.TryGetValue(sector ?? "", out var r) ? r : (1, SingleMax);

        /// <summary>Nova struktura: n sektorjev (0 = en sektor) s po m postajami.</summary>
        public static StructureDef Create(string name, int sectors, int stationsPerSector)
        {
            var d = new StructureDef { MachineName = name, Sectored = sectors > 0 };
            if (d.Sectored)
                foreach (var l in SectorLetters.Take(Math.Min(sectors, SectorLetters.Length)))
                    d.Sectors.Add(new SectorDef { Letter = l, StationCount = stationsPerSector });
            else d.Sectors.Add(new SectorDef { Letter = "", StationCount = stationsPerSector });
            Rebuild(d);
            return d;
        }

        /// <summary>
        /// Obnovi seznam postaj iz števila postaj po sektorjih in števila podpostaj; ohrani opise, indekse in podatke obstoječih postaj.
        /// Postaje nad novim številom se odstranijo (tudi obstoječe – GUI to prej potrdi).
        /// </summary>
        public static void Rebuild(StructureDef d)
        {
            var old = d.Stations.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());
            var list = new List<StationDef>();
            if (!d.Sectored && d.Sectors.Count == 0) d.Sectors.Add(new SectorDef { Letter = "" });
            if (!d.Sectored) d.Sectors.RemoveAll(s => s != d.Sectors[0]);
            foreach (var s in d.Sectors)
            {
                string letter = d.Sectored ? s.Letter : "";
                if (!d.Sectored) s.Letter = "";
                for (int n = 1; n <= s.StationCount; n++)
                {
                    if (s.Skip.Contains(n)) continue;
                    string id = StationId(d.Sectored, letter, n);
                    old.TryGetValue(id, out var o);
                    var main = o ?? new StationDef { Id = id };
                    main.Sector = letter; main.No = n; main.Parent = null; main.SubNo = null;
                    list.Add(main);
                    for (int k = 1; k <= main.SubCount; k++)
                    {
                        string sid = id + "_" + k;
                        old.TryGetValue(sid, out var os);
                        var sub = os ?? new StationDef { Id = sid, Text = main.Text };
                        sub.Sector = letter; sub.No = n; sub.Parent = id; sub.SubNo = k; sub.SubCount = 0;
                        list.Add(sub);
                    }
                }
            }
            d.Stations = list;
            AllocateIndices(d);
        }

        /// <summary>Dodeli PLC indekse postajam z indeksom 0: glavna postaja = osnova + št. (če je prost), sicer in podpostaje = naslednji prosti v razponu.</summary>
        public static void AllocateIndices(StructureDef d)
        {
            foreach (var g in d.Stations.GroupBy(s => s.Sector ?? ""))
            {
                var (lo, hi) = RangeOf(d, g.Key);
                var used = new HashSet<int>(d.Stations.Where(s => s.PlcIndex > 0).Select(s => s.PlcIndex));
                foreach (var s in g.Where(x => !x.IsSub && !x.Virtual && x.PlcIndex == 0))
                {
                    int want = d.Sectored ? lo + s.No : s.No;
                    if (want <= hi && !used.Contains(want)) { s.PlcIndex = want; used.Add(want); }
                }
                int next = lo + 1;
                foreach (var s in g.Where(x => !x.Virtual && x.PlcIndex == 0))
                {
                    while (next <= hi && used.Contains(next)) next++;
                    if (next > hi) break;           // ni prostega – Validate javi
                    s.PlcIndex = next; used.Add(next);
                }
            }
        }

        /// <summary>Opozorila strukture: podvojeni indeksi, izven razpona, nedodeljeni.</summary>
        public static List<string> Validate(StructureDef d)
        {
            var w = new List<string>();
            if (d.Stations.Count == 0) w.Add("Stroj nima nobene postaje.");
            foreach (var s in d.Stations.Where(x => !x.Virtual))
            {
                var (lo, hi) = RangeOf(d, s.Sector);
                if (s.PlcIndex == 0) w.Add(s.Id + ": ni prostega PLC indeksa v razponu " + lo + "–" + hi + ".");
                else if (s.PlcIndex < lo || s.PlcIndex > hi)
                    w.Add(s.Id + ": PLC indeks " + s.PlcIndex + " je izven razpona " + (d.Sectored ? "sektorja " + s.Sector + " " : "") + lo + "–" + hi + " (standard 5.3).");
            }
            foreach (var g in d.Stations.Where(s => s.PlcIndex > 0).GroupBy(s => s.PlcIndex).Where(g => g.Count() > 1))
                w.Add("PLC indeks " + g.Key + " ima več postaj: " + string.Join(", ", g.Select(s => s.Id)) + ".");
            if (d.Sectored)
                foreach (var s in d.Sectors.Where(x => x.StationCount > 0))
                {
                    var (lo, hi) = Ranges[s.Letter];
                    int n = d.Stations.Count(x => x.Sector == s.Letter && !x.Virtual);
                    if (n > hi - lo) w.Add("Sektor " + s.Letter + ": " + n + " postaj/podpostaj, razpon " + lo + "–" + hi + " jih dovoli največ " + (hi - lo) + ".");
                }
            return w;
        }

        /// <summary>Struktura iz obstoječega modela (izpeljanega iz exporta). Enote, ki ne ustrezajo shemi (npr. D01 v mapi Dosing, R_G10_EQ_1), ostanejo nedotaknjene.</summary>
        public static StructureDef FromModel(MachineModelFile m)
        {
            var d = new StructureDef
            {
                MachineName = m.Machine.Name,
                Pc = m.Machine.HmiDevices.Any(x => x.Kind == "PC"),
                Panel = m.Machine.HmiDevices.Any(x => x.Kind == "Panel"),
            };
            var managed = m.Units.Where(u => IsManaged(u, true)).ToList();
            var single = m.Units.Where(u => IsManaged(u, false)).ToList();
            d.Sectored = managed.Count >= single.Count;
            var units = d.Sectored ? managed : single;
            var rx = d.Sectored ? SectoredId : SingleId;

            foreach (var u in units)
            {
                var mm = rx.Match(u.Id);
                string sector = d.Sectored ? mm.Groups[1].Value : "";
                int no = int.Parse(mm.Groups[d.Sectored ? 2 : 1].Value);
                var sub = mm.Groups[d.Sectored ? 3 : 2];
                d.Stations.Add(new StationDef
                {
                    Id = u.Id, Sector = sector, No = no,
                    Parent = sub.Success ? StationId(d.Sectored, sector, no) : null,
                    SubNo = sub.Success ? int.Parse(sub.Value) : (int?)null,
                    Text = Get(u.Text, "sl-SI"), PlcIndex = u.PlcIndex ?? 0, Existing = u.Origin != "struktura",
                });
            }
            // podpostaje brez glavne postaje (npr. samo B02_1..4 brez B02) – glavno postajo dodamo kot "manjkajočo" (Skip), podpostaje ostanejo
            foreach (var s in d.Stations.Where(x => !x.IsSub)) s.SubCount = d.Stations.Count(x => x.Parent == s.Id);
            foreach (var g in d.Stations.GroupBy(x => x.Sector).OrderBy(g => g.Key))
            {
                var sec = new SectorDef { Letter = g.Key, StationCount = g.Max(x => x.No) };
                if (m.Machine.PartTexts != null && m.Machine.PartTexts.TryGetValue(g.Key, out var st)) sec.Text = st;
                var present = new HashSet<int>(g.Where(x => !x.IsSub).Select(x => x.No));
                var orphanParents = new HashSet<int>(g.Where(x => x.IsSub && !present.Contains(x.No)).Select(x => x.No));
                for (int n = 1; n <= sec.StationCount; n++)
                    if (!present.Contains(n) && !orphanParents.Contains(n)) sec.Skip.Add(n);
                // glavna postaja, ki obstaja samo kot nadrejena podpostajam (Purem B02 → B02_1..4): ustvarimo jo kot nosilec podpostaj, a označimo
                foreach (var n in orphanParents)
                {
                    string pid = StationId(d.Sectored, g.Key, n);
                    d.Stations.Add(new StationDef
                    {
                        Id = pid, Sector = g.Key, No = n, Existing = false, Virtual = true, Text = g.First(x => x.No == n).Text,
                        SubCount = g.Count(x => x.No == n && x.IsSub), PlcIndex = 0,
                    });
                    sec.Skip.Remove(n);
                }
                d.Sectors.Add(sec);
            }
            d.Stations = d.Stations.OrderBy(x => x.Sector).ThenBy(x => x.No).ThenBy(x => x.SubNo ?? 0).ToList();
            return d;
        }

        static bool IsManaged(Unit u, bool sectored) => (sectored ? SectoredId : SingleId).IsMatch(u.Id);
        static string Get(LangText t, string lang) => t != null && t.TryGetValue(lang, out var v) ? v : "";

        /// <summary>
        /// Uveljavi strukturo v modelu: obstoječe enote posodobi (opis, indeks), nove ustvari (Origin = "struktura"), odstranjene iz sheme izbriše.
        /// Enote zunaj sheme (D01 v Machine/Dosing, linije R_/S_) ostanejo. existing = null → nov model (nov stroj, PLC še ne obstaja).
        /// </summary>
        public static MachineModelFile ApplyToModel(StructureDef d, MachineModelFile existing, TextDictionary dict)
        {
            var m = existing ?? NewModel(d);
            m.Machine.Name = d.MachineName;
            m.Machine.Type = d.Sectored ? "parts" : "single";
            if (m.Machine.PartTexts == null) m.Machine.PartTexts = new SortedDictionary<string, string>();
            foreach (var s in d.Sectors.Where(x => x.Letter.Length > 0))
                if (string.IsNullOrWhiteSpace(s.Text)) m.Machine.PartTexts.Remove(s.Letter); else m.Machine.PartTexts[s.Letter] = s.Text.Trim();
            SyncDevices(m, d);

            var byId = m.Units.ToDictionary(u => u.Id);
            var wanted = d.Stations.Where(s => !s.Virtual || byId.ContainsKey(s.Id)).ToList();
            var wantedIds = new HashSet<string>(wanted.Select(s => s.Id));
            // odstrani enote v shemi, ki jih struktura nima več
            m.Units.RemoveAll(u => IsManaged(u, d.Sectored) && !wantedIds.Contains(u.Id));

            foreach (var s in wanted)
            {
                if (!byId.TryGetValue(s.Id, out var u))
                {
                    u = NewUnit(d, s, dict);
                    m.Units.Add(u);
                }
                else
                {
                    if (!string.IsNullOrEmpty(s.Text) && Get(u.Text, "sl-SI") != s.Text) { u.Text["sl-SI"] = s.Text; u.TextSource = TextSource.Manual; }
                    if (s.PlcIndex > 0 && u.PlcIndex != s.PlcIndex)
                    {
                        u.PlcIndex = s.PlcIndex; u.HmiIndex = s.PlcIndex - 1;
                        u.IndexSource = (u.IndexSource ?? "") + " → struktura: " + s.PlcIndex + " (ročno)";
                    }
                    u.Parent = s.Parent; u.SubIndex = s.SubNo;
                }
            }
            m.Units = m.Units.OrderBy(u => u.Part ?? u.Group ?? "~").ThenBy(u => u.Id, StringComparer.Ordinal).ToList();
            m.Machine.Parts = m.Units.Where(u => u.Part != null).Select(u => u.Part).Distinct().OrderBy(x => x).ToList();

            m.Warnings.RemoveAll(w => w.Code != null && w.Code.StartsWith("STRUCT"));
            foreach (var w in Validate(d)) m.Warnings.Add(new ModelWarning { Code = "STRUCT", Message = w });
            ModelExtractor.RecalcStatistics(m);
            return m;
        }

        static void SyncDevices(MachineModelFile m, StructureDef d)
        {
            void Ensure(string kind, bool on, string target, string res, string conn)
            {
                var have = m.Machine.HmiDevices.Where(x => x.Kind == kind).ToList();
                if (on && have.Count == 0)
                    m.Machine.HmiDevices.Add(new HmiDevice { Kind = kind, Target = target, Resolution = res, Connection = conn, Source = "struktura (privzeto po standardu – preveri ime targeta)" });
                if (!on) m.Machine.HmiDevices.RemoveAll(x => x.Kind == kind);
            }
            Ensure("PC", d.Pc, "HMI_RT_1", "1920x1080", "HMI_Connection");
            Ensure("Panel", d.Panel, "HMI_Panel_1", "800x480", "HMI_Connection_1");
            var kinds = m.Machine.HmiDevices.Select(x => x.Kind).Distinct().ToList();
            foreach (var u in m.Units.Where(x => x.Origin == "struktura")) u.ShowOn = kinds.ToList();
        }

        static MachineModelFile NewModel(StructureDef d)
        {
            var m = new MachineModelFile
            {
                Generator = "TIA_Generator " + typeof(StructureBuilder).Assembly.GetName().Version,
                GeneratedAt = DateTime.Now.ToString("s"),
                SourceExport = null,
            };
            m.Machine.Plc.Name = "PLC_1";
            m.Machine.Plc.Source = "struktura (nov stroj)";
            var map = m.Machine.DbMap;
            map.Counters = "DB1000"; map.ServiceSettings = "DB1010"; map.ErrorMessages = "DB1005";
            map.Takt = "DB1004"; map.Recipe = "DB2000"; map.CylinderControl = "DB4000";
            map.Names["counters"] = "DB_Counter_GOOD"; map.Names["serviceSettings"] = "DB_Service_Settings";
            map.Names["errorMessages"] = "DB_Error_Messages"; map.Names["takt"] = "DB_Takt";
            map.Names["recipe"] = "Recipe"; map.Names["cylinderControl"] = "CYLINDER_CONTROL_DB";
            map.Source = "privzeto po standardu 2.0 (nov stroj)";
            m.Global.ServiceSettingsArrays = new List<string> { "OFF", "TEST", "DOSING", "REPEAT_MEASURE", "FREE", "ERROR", "RESET", "RESET_OFF", "ENABLE_OPEN_GRIPPER", "PART_n_BAD_Count", "END", "STEP", "MOVING", "ENABLE_HMI_COMMAND", "DEBLOCK" };
            m.Global.ManualControlEnable = "Enable_Manual_Control";
            return m;
        }

        public static readonly string[] StandardFunctions = { "OFF", "TEST", "RESET", "RESET_OFF", "ERROR", "END", "FREE", "MOVING", "STEP", "ENABLE_HMI_COMMAND" };

        static Unit NewUnit(StructureDef d, StationDef s, TextDictionary dict)
        {
            string folder = d.Sectored ? "Machine/" + s.Sector + "/" + s.Id + " - " + (s.Text.Length > 0 ? s.Text : "Postaja") : "Machine/" + s.Id + " - " + (s.Text.Length > 0 ? s.Text : "Postaja");
            string settings = "Unit_" + s.Id + "_DB_Settings";
            var u = new Unit
            {
                Id = s.Id, Origin = "struktura", Fb = "Unit_" + s.Id,
                Part = d.Sectored ? s.Sector : null, Parent = s.Parent, SubIndex = s.SubNo, Folder = folder,
                Text = new LangText(s.Text, "", ""), TextSource = s.Text.Length > 0 ? "sl: struktura; en/de: " + TextSource.Missing : TextSource.Missing,
                PlcIndex = s.PlcIndex, HmiIndex = s.PlcIndex - 1, IndexSource = "struktura (standard 5.3)",
                SettingsDb = new SettingsDb { Name = settings, Number = 100 + s.PlcIndex, Layout = "Standard" },
                TimerLayout = "array",
                StationFunctions = StandardFunctions.ToList(),
            };
            for (int i = 0; i < 10; i++)
                u.Timers.Add(new TimerDef { Index = i, Member = "Delay[" + i + "]", ArrayElement = "Delay[" + i + "]", Show = false, Text = new LangText(), TextSource = TextSource.Missing });
            u.HmiTags.Timers = new HmiTagDef
            {
                Pc = s.Id + "_Timers", Panel = "Unit_" + s.Id + "_Timers",
                PlcTag = "\"" + settings + "\"." + ModelExtractor.TimerArrayName, DataType = "Array[0..9] of S5Time",
                Note = "Nova postaja – Settings DB ustvari 'PLC ogrodje'. Časovnike, ki jih postaja uporablja, označi 'Prikaži' in jim vpiši besedilo.",
            };
            u.PartBadCount = new PartBadCount { Member = "Part_n_BAD_Count", DataType = "Int", Text = dict?.Lookup("Part_n_BAD_Count").text };
            return u;
        }
    }
}
