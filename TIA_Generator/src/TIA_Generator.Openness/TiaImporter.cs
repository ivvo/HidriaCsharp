using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;

namespace TIA_Generator.Openness
{
    /// <summary>En TIA Portal proces (samo podatki – varno za UI thread).</summary>
    public sealed class TiaProcess
    {
        public int Id;
        public string Project;
        public override string ToString() => "PID " + Id + " – " + (Project ?? "(brez projekta)");
    }

    /// <summary>Ena postavka načrta uvoza.</summary>
    public sealed class ImportItem
    {
        public string File;          // XML bloka (PLC ogrodje)
        public string Name;          // ime bloka
        public string Kind;          // FB / DB
        public int Number;
        public string GroupPath;     // "Machine/A/A01 - Postaja"
        public string Action;        // "nov", "obstaja – preskočeno", "prepiši", "KONFLIKT …"
        public bool Do;
        public override string ToString() => string.Format("{0,-6} {1}{2,-5} {3,-28} → {4}   [{5}]", Do ? "UVOZ" : "-", Kind, Number, Name, GroupPath, Action);
    }

    public sealed class ImportPlan
    {
        public string Plc;
        public List<ImportItem> Blocks = new List<ImportItem>();
        /// <summary>Biti, ki manjkajo v DB_Error_Messages (dodajo se na konec DB-ja, da se odmiki obstoječih – HMI alarmi – ne premaknejo).</summary>
        public List<string> AlarmBitsToAdd = new List<string>();
        public string AlarmDb;
        public List<string> Notes = new List<string>();
    }

    /// <summary>
    /// Uvoz PLC ogrodja (TIA_Generator "PLC ogrodje …") v odprt TIA Portal V20 prek Openness.
    /// Vsi klici tečejo na enem STA threadu. Projekta NIKOLI ne shrani – uporabnik pregleda in shrani v TIA.
    /// </summary>
    public sealed class TiaImporter : IDisposable
    {
        readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();
        readonly Thread _thread;
        TiaPortal _portal;
        Project _project;
        int _pid;

        public TiaImporter()
        {
            _thread = new Thread(() => { foreach (var a in _queue.GetConsumingEnumerable()) a(); }) { IsBackground = true, Name = "Openness" };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        Task<T> Run<T>(Func<T> f)
        {
            var tcs = new TaskCompletionSource<T>();
            _queue.Add(() => { try { tcs.SetResult(f()); } catch (Exception ex) { tcs.SetException(ex); } });
            return tcs.Task;
        }

        public void Dispose()
        {
            _queue.Add(() => { try { _portal?.Dispose(); } catch { } _portal = null; _project = null; });
            _queue.CompleteAdding();
        }

        // ------------------------------------------------------------------ priklop

        public Task<List<TiaProcess>> ListProcesses() => Run(() =>
            TiaPortal.GetProcesses().Select(p => new TiaProcess { Id = p.Id, Project = p.ProjectPath?.Name }).ToList());

        /// <summary>Priklop na proces; vrne imena PLC-jev v projektu.</summary>
        public Task<List<string>> Connect(int pid, Action<string> log) => Run(() =>
        {
            if (_portal == null || _pid != pid || _project == null)
            {
                try { _portal?.Dispose(); } catch { }
                var proc = TiaPortal.GetProcesses().FirstOrDefault(p => p.Id == pid) ?? throw new InvalidOperationException("TIA Portal proces " + pid + " ne obstaja več.");
                log("Priklapljam se na TIA Portal (PID " + pid + ") – v TIA potrdi Openness dostop, če vpraša ...");
                _portal = proc.Attach();
                _pid = pid;
                if (_portal.Projects.Count == 0)
                {
                    _portal.Dispose(); _portal = null;
                    throw new InvalidOperationException("V TIA Portalu (PID " + pid + ") Openness ne vidi projekta. Projekt mora biti odprt; če uporablja dodatek, ki ni nameščen " +
                                                        "(npr. SINAMICS Startdrive), ga Openness ne vidi – namesti dodatek ali odstrani napravo (kontrolni seznam osnovnega projekta, 1).");
                }
                _project = _portal.Projects[0];
            }
            log("Povezan na projekt " + _project.Name + ".");
            return Plcs().Select(p => p.name).ToList();
        });

        IEnumerable<(string name, PlcSoftware sw)> Plcs()
        {
            foreach (var d in AllDevices(_project.Devices, _project.DeviceGroups))
                foreach (var item in AllItems(d.DeviceItems))
                {
                    var sw = item.GetService<SoftwareContainer>()?.Software as PlcSoftware;
                    if (sw != null) yield return (sw.Name, sw);
                }
        }

        static IEnumerable<Device> AllDevices(DeviceComposition devices, DeviceUserGroupComposition groups)
        {
            foreach (var d in devices) yield return d;
            foreach (var g in groups)
                foreach (var d in AllDevices(g.Devices, g.Groups)) yield return d;
        }

        static IEnumerable<DeviceItem> AllItems(DeviceItemComposition items)
        {
            foreach (var i in items)
            {
                yield return i;
                foreach (var c in AllItems(i.DeviceItems)) yield return c;
            }
        }

        PlcSoftware Plc(string name) => Plcs().FirstOrDefault(p => p.name == name).sw ?? throw new InvalidOperationException("PLC " + name + " ni v projektu.");

        // ------------------------------------------------------------------ načrt (dry-run)

        static string L(XElement e) => e.Name.LocalName;

        /// <summary>Blok iz XML datoteke: vrsta, ime, številka (brez Openness).</summary>
        static (string kind, string name, int number) ReadXml(string file)
        {
            var root = XDocument.Load(file).Root;
            var blk = root.Elements().First(e => L(e).StartsWith("SW.Blocks."));
            var attrs = blk.Elements().First(e => L(e) == "AttributeList");
            string name = (string)attrs.Elements().FirstOrDefault(e => L(e) == "Name");
            int.TryParse((string)attrs.Elements().FirstOrDefault(e => L(e) == "Number"), out int num);
            string t = L(blk).Substring("SW.Blocks.".Length);
            return (t == "FB" || t == "FC" || t == "OB" ? t : "DB", name, num);
        }

        /// <summary>Vsi bloki v PLC: ime → (vrsta+številka, pot mape).</summary>
        static Dictionary<string, (string key, string path, PlcBlock block)> AllBlocks(PlcBlockGroup g, string path, Dictionary<string, (string, string, PlcBlock)> into = null)
        {
            into = into ?? new Dictionary<string, (string, string, PlcBlock)>(StringComparer.OrdinalIgnoreCase);
            foreach (PlcBlock b in g.Blocks)
            {
                string kind = b is FB ? "FB" : b is FC ? "FC" : b is OB ? "OB" : "DB";
                into[b.Name] = (kind + b.Number, path, b);
            }
            foreach (PlcBlockUserGroup sub in g.Groups) AllBlocks(sub, path.Length == 0 ? sub.Name : path + "/" + sub.Name, into);
            return into;
        }

        /// <summary>
        /// Načrt uvoza mape PLC ogrodja (&lt;skeletonDir&gt;\PLC\Machine\…): nov / obstaja / konflikt številke; manjkajoči biti alarmov.
        /// Ničesar ne spreminja.
        /// </summary>
        public Task<ImportPlan> Plan(string plcName, string skeletonDir, string alarmDbName, bool overwrite) => Run(() =>
        {
            var plc = Plc(plcName);
            var plan = new ImportPlan { Plc = plcName, AlarmDb = alarmDbName };
            string root = Path.Combine(skeletonDir, "PLC");
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException("Mapa PLC ogrodja nima podmape PLC: " + skeletonDir);

            var existing = AllBlocks(plc.BlockGroup, "");
            var byNumber = existing.ToDictionary(kv => kv.Value.key, kv => kv.Key, StringComparer.OrdinalIgnoreCase);
            var neededAlarms = new SortedSet<string>(StringComparer.Ordinal);

            // vrstni red: FB pred instanco (instanca potrebuje FB), nato Settings DB
            var files = Directory.EnumerateFiles(root, "*.xml", SearchOption.AllDirectories)
                                 .OrderBy(f => Path.GetDirectoryName(f), StringComparer.Ordinal)
                                 .ThenBy(f => f.EndsWith("_DB_Settings.xml") ? 2 : f.EndsWith("_DB.xml") ? 1 : 0).ToList();
            foreach (var f in files)
            {
                var (kind, name, number) = ReadXml(f);
                string group = Path.GetDirectoryName(f).Substring(root.Length).Trim('\\', '/').Replace('\\', '/');
                var it = new ImportItem { File = f, Name = name, Kind = kind, Number = number, GroupPath = group };
                bool nameExists = existing.TryGetValue(name, out var ex);
                if (byNumber.TryGetValue(kind + number, out var other) && !string.Equals(other, name, StringComparison.OrdinalIgnoreCase))
                    { it.Action = "KONFLIKT: " + kind + number + " je že " + other; it.Do = false; }
                else if (nameExists && !overwrite) { it.Action = "obstaja (" + ex.path + ") – preskočeno"; it.Do = false; }
                else if (nameExists) { it.Action = "PREPIŠI obstoječega (" + ex.path + ")"; it.Do = true; }
                else { it.Action = "nov"; it.Do = true; }
                plan.Blocks.Add(it);

                if (kind == "FB" && alarmDbName != null)
                    foreach (var sym in XDocument.Load(f).Descendants().Where(e => L(e) == "Symbol"))
                    {
                        var c = sym.Elements().Where(e => L(e) == "Component").Select(e => (string)e.Attribute("Name")).ToList();
                        if (c.Count >= 2 && c[0] == alarmDbName) neededAlarms.Add(c[1]);
                    }
            }
            // konflikt FB → preskoči tudi njegovo instanco in Settings (ista mapa)
            foreach (var g in plan.Blocks.GroupBy(b => b.GroupPath).Where(g => g.Any(b => b.Action.StartsWith("KONFLIKT"))))
                foreach (var b in g.Where(b => b.Do)) { b.Do = false; b.Action += " (preskočeno zaradi konflikta v isti postaji)"; }

            if (alarmDbName != null && neededAlarms.Count > 0)
            {
                if (!existing.TryGetValue(alarmDbName, out var adb)) plan.Notes.Add("DB " + alarmDbName + " ni v projektu – bitov alarmov ni mogoče dodati.");
                else
                {
                    var members = new HashSet<string>(DbMembers(adb.block), StringComparer.Ordinal);
                    plan.AlarmBitsToAdd = neededAlarms.Where(a => !members.Contains(a)).ToList();
                }
            }
            plan.Notes.Add("Klicev postaj v Main (OB1) in PLC tagov U_<oznaka>_RESET_ENABLED uvoz ne doda – glej PLC_ogrodje_porocilo.txt.");
            return plan;
        });

        /// <summary>Imena članov DB-ja (export v začasno datoteko).</summary>
        static List<string> DbMembers(PlcBlock db)
        {
            string tmp = Path.Combine(Path.GetTempPath(), "TIA_Generator_" + Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                db.Export(new FileInfo(tmp), ExportOptions.None);
                var stat = XDocument.Load(tmp).Descendants().FirstOrDefault(e => L(e) == "Section" && (string)e.Attribute("Name") == "Static");
                return stat == null ? new List<string>() : stat.Elements().Where(e => L(e) == "Member").Select(e => (string)e.Attribute("Name")).ToList();
            }
            finally { try { File.Delete(tmp); } catch { } }
        }

        // ------------------------------------------------------------------ uvoz

        public Task<int> Execute(ImportPlan plan, bool addAlarmBits, Action<string> log) => Run(() =>
        {
            var plc = Plc(plan.Plc);
            int ok = 0;
            foreach (var it in plan.Blocks.Where(b => b.Do))
            {
                try
                {
                    var grp = EnsureGroup(plc.BlockGroup, it.GroupPath);
                    var opts = it.Action.StartsWith("PREPIŠI") ? ImportOptions.Override : ImportOptions.None;
                    grp.Blocks.Import(new FileInfo(it.File), opts, SWImportOptions.IgnoreMissingReferencedObjects);
                    log("  uvožen " + it.Kind + it.Number + " " + it.Name + " → " + it.GroupPath);
                    ok++;
                }
                catch (Exception ex) { log("  NAPAKA " + it.Name + ": " + ex.Message); }
            }
            if (addAlarmBits && plan.AlarmBitsToAdd.Count > 0)
            {
                try { AppendAlarmBits(plc, plan.AlarmDb, plan.AlarmBitsToAdd, log); }
                catch (Exception ex) { log("  NAPAKA pri dodajanju bitov v " + plan.AlarmDb + ": " + ex.Message); }
            }
            log("Uvoz končan: " + ok + " blokov. Projekt NI shranjen – v TIA prevedi (Compile), preglej in shrani.");
            return ok;
        });

        static PlcBlockGroup EnsureGroup(PlcBlockSystemGroup root, string path)
        {
            PlcBlockGroup g = root;
            foreach (var part in (path ?? "").Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                g = g.Groups.Find(part) ?? (PlcBlockGroup)g.Groups.Create(part);
            return g;
        }

        /// <summary>Doda Bool člane na KONEC DB_Error_Messages (export → XML → import Override v isto mapo).</summary>
        static void AppendAlarmBits(PlcSoftware plc, string dbName, List<string> bits, Action<string> log)
        {
            var all = AllBlocks(plc.BlockGroup, "");
            if (!all.TryGetValue(dbName, out var e)) throw new InvalidOperationException("DB " + dbName + " ni v projektu.");
            var db = e.block;
            var group = (PlcBlockGroup)db.Parent;
            string tmp = Path.Combine(Path.GetTempPath(), "TIA_Generator_" + dbName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml");
            db.Export(new FileInfo(tmp), ExportOptions.WithDefaults);
            File.Copy(tmp, tmp.Replace(".xml", "_ORIGINAL.xml"), true);   // varnostna kopija
            var doc = XDocument.Load(tmp, LoadOptions.PreserveWhitespace);
            var stat = doc.Descendants().First(x => L(x) == "Section" && (string)x.Attribute("Name") == "Static");
            var proto = stat.Elements().LastOrDefault(x => L(x) == "Member" && (string)x.Attribute("Datatype") == "Bool");
            var ns = stat.Name.Namespace;
            foreach (var b in bits)
            {
                var m = proto != null ? new XElement(proto) : new XElement(ns + "Member", new XAttribute("Datatype", "Bool"));
                m.SetAttributeValue("Name", b);
                m.Elements().Where(x => L(x) == "Comment" || L(x) == "StartValue").Remove();
                stat.Add(m);
            }
            using (var w = new StreamWriter(tmp, false, new UTF8Encoding(true))) doc.Save(w, SaveOptions.DisableFormatting);
            group.Blocks.Import(new FileInfo(tmp), ImportOptions.Override, SWImportOptions.IgnoreMissingReferencedObjects);
            log("  " + dbName + ": dodanih " + bits.Count + " bitov na konec (" + string.Join(", ", bits) + "). Varnostna kopija: " + tmp.Replace(".xml", "_ORIGINAL.xml"));
            log("  POZOR: HMI alarme za nove bite dodaj ročno (Openness alarmov ne uvaža).");
        }
    }
}
