using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Screen;

namespace TIA_HMIColorTool
{
    class TiaProcessInfo
    {
        public int Id;
        public string Display;
        public override string ToString() { return Display; }
    }

    class HmiTargetInfo
    {
        public string Name;
        public string DeviceName;
        internal HmiTarget Target;
        public override string ToString() { return Name + "   [" + DeviceName + "]"; }
    }

    enum HmiItemKind { Template, Screen }

    /// <summary>En zaslon ali predloga - samo podatki (brez Openness objektov), varno za UI thread.</summary>
    class HmiItem
    {
        public HmiTargetInfo Target;
        public HmiItemKind Kind;
        public string TiaPath;   // pot mape v TIA: "a/b/c" ali "" za koren
        public string Name;

        public string Key { get { return Kind + "|" + TiaPath + "|" + Name; } }
    }

    /// <summary>
    /// Vsi Openness klici. Metode kliči SAMO s StaWorker threada.
    /// Projekta NIKOLI ne shrani - to naredi uporabnik ročno v TIA Portalu.
    /// </summary>
    sealed class TiaService : IDisposable
    {
        TiaPortal _portal;
        Project _project;

        public bool IsConnected { get { return _project != null; } }
        public string ProjectName { get { return _project == null ? null : _project.Name; } }

        // ------------------------------------------------------------------ Priklop

        public static List<TiaProcessInfo> ListProcesses()
        {
            var list = new List<TiaProcessInfo>();
            foreach (var p in TiaPortal.GetProcesses())
            {
                string proj = p.ProjectPath != null ? p.ProjectPath.Name : "(ni odprtega projekta)";
                list.Add(new TiaProcessInfo { Id = p.Id, Display = "PID " + p.Id + " - " + proj + " (" + p.Mode + ")" });
            }
            return list;
        }

        public void Connect(int processId, Action<string> log)
        {
            Disconnect();
            var proc = TiaPortal.GetProcesses().FirstOrDefault(p => p.Id == processId);
            if (proc == null) throw new InvalidOperationException("TIA Portal proces " + processId + " ne obstaja več.");

            log("Priklapljam se na TIA Portal (PID " + processId + ") - v TIA Portalu potrdi Openness dostop, če vpraša ...");
            _portal = proc.Attach();
            if (_portal.Projects.Count == 0)
            {
                Disconnect();
                throw new InvalidOperationException("V izbranem TIA Portalu ni odprtega projekta.");
            }
            _project = _portal.Projects[0];
            log("Povezan na projekt: " + _project.Name);
        }

        public void Disconnect()
        {
            _project = null;
            if (_portal != null)
            {
                // Dispose na priklopljeni instanci samo odklopi - TIA Portal ostane odprt.
                try { _portal.Dispose(); } catch { }
                _portal = null;
            }
        }

        public void Dispose() { Disconnect(); }

        // ------------------------------------------------------------------ HMI targeti

        public List<HmiTargetInfo> FindAllHmiTargets()
        {
            EnsureConnected();
            var list = new List<HmiTargetInfo>();
            foreach (var device in GetAllDevices(_project))
                foreach (var item in AllDeviceItemsRecursive(device.DeviceItems))
                {
                    var container = item.GetService<SoftwareContainer>();
                    var target = container == null ? null : container.Software as HmiTarget;
                    if (target != null)
                        list.Add(new HmiTargetInfo { Name = target.Name, DeviceName = device.Name, Target = target });
                }
            return list;
        }

        static IEnumerable<Device> GetAllDevices(Project project)
        {
            foreach (Device d in project.Devices)
                yield return d;
            foreach (DeviceUserGroup grp in project.DeviceGroups)
                foreach (var d in GetAllDevicesInGroup(grp))
                    yield return d;
        }

        static IEnumerable<Device> GetAllDevicesInGroup(DeviceUserGroup grp)
        {
            foreach (Device d in grp.Devices)
                yield return d;
            foreach (DeviceUserGroup sub in grp.Groups)
                foreach (var d in GetAllDevicesInGroup(sub))
                    yield return d;
        }

        static IEnumerable<DeviceItem> AllDeviceItemsRecursive(DeviceItemComposition items)
        {
            foreach (DeviceItem item in items)
            {
                yield return item;
                foreach (var sub in AllDeviceItemsRecursive(item.DeviceItems))
                    yield return sub;
            }
        }

        // ------------------------------------------------------------------ Naštevanje zaslonov/predlog

        /// <summary>Vse predloge in zasloni targeta (v TIA vrstnem redu) - za drevo v GUI.</summary>
        public List<HmiItem> ListItems(HmiTargetInfo t)
        {
            EnsureConnected();
            var list = new List<HmiItem>();
            foreach (var x in EnumerateTemplates(t.Target))
                list.Add(new HmiItem { Target = t, Kind = HmiItemKind.Template, TiaPath = x.Item1, Name = x.Item2.Name });
            foreach (var x in EnumerateScreens(t.Target))
                list.Add(new HmiItem { Target = t, Kind = HmiItemKind.Screen, TiaPath = x.Item1, Name = x.Item2.Name });
            return list;
        }

        static IEnumerable<Tuple<string, Screen>> EnumerateScreens(HmiTarget target)
        {
            var root = target.ScreenFolder;
            foreach (Screen s in root.Screens) yield return Tuple.Create("", s);
            foreach (ScreenUserFolder sub in root.Folders)
                foreach (var t in EnumerateScreens(sub, sub.Name)) yield return t;
        }

        static IEnumerable<Tuple<string, Screen>> EnumerateScreens(ScreenUserFolder folder, string path)
        {
            foreach (Screen s in folder.Screens) yield return Tuple.Create(path, s);
            foreach (ScreenUserFolder sub in folder.Folders)
                foreach (var t in EnumerateScreens(sub, path + "/" + sub.Name)) yield return t;
        }

        static IEnumerable<Tuple<string, ScreenTemplate>> EnumerateTemplates(HmiTarget target)
        {
            var root = target.ScreenTemplateFolder;
            foreach (ScreenTemplate s in root.ScreenTemplates) yield return Tuple.Create("", s);
            foreach (ScreenTemplateUserFolder sub in root.Folders)
                foreach (var t in EnumerateTemplates(sub, sub.Name)) yield return t;
        }

        static IEnumerable<Tuple<string, ScreenTemplate>> EnumerateTemplates(ScreenTemplateUserFolder folder, string path)
        {
            foreach (ScreenTemplate s in folder.ScreenTemplates) yield return Tuple.Create(path, s);
            foreach (ScreenTemplateUserFolder sub in folder.Folders)
                foreach (var t in EnumerateTemplates(sub, path + "/" + sub.Name)) yield return t;
        }

        // ------------------------------------------------------------------ Poti datotek

        public static string TargetDir(string outDir, HmiTargetInfo t)
        {
            return Path.Combine(outDir, Sanitize(t.Name));
        }

        /// <summary>
        /// Deterministična pot XML datoteke za element:
        /// outDir\&lt;target&gt;\Templates|Screens\&lt;mape&gt;\&lt;ime&gt;.xml
        /// Ista pot se uporabi pri exportu, zamenjavi barv in importu.
        /// </summary>
        public static string ItemFile(string outDir, HmiItem item)
        {
            var parts = new List<string> { TargetDir(outDir, item.Target), item.Kind == HmiItemKind.Template ? "Templates" : "Screens" };
            if (item.TiaPath.Length > 0) parts.AddRange(item.TiaPath.Split('/').Select(Sanitize));
            parts.Add(Sanitize(item.Name) + ".xml");
            return Path.Combine(parts.ToArray());
        }

        static string Sanitize(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }

        // ------------------------------------------------------------------ Export

        /// <summary>Exporta izbrane predloge/zaslone enega targeta. Ostalih datotek v mapi ne spreminja.</summary>
        public void ExportItems(HmiTargetInfo t, IList<HmiItem> items, string outDir, Action<string> log, out int ok, out int failed)
        {
            EnsureConnected();
            ok = 0; failed = 0;
            var wanted = new HashSet<string>(items.Select(i => i.Key));
            log("=== Export targeta " + t.Name + " (" + wanted.Count + " izbranih) -> " + TargetDir(outDir, t));

            foreach (var x in EnumerateTemplates(t.Target))
            {
                var item = new HmiItem { Target = t, Kind = HmiItemKind.Template, TiaPath = x.Item1, Name = x.Item2.Name };
                if (!wanted.Remove(item.Key)) continue;
                var tpl = x.Item2;
                if (ExportOne(f => tpl.Export(f, ExportOptions.WithDefaults), outDir, item, log)) ok++; else failed++;
            }

            foreach (var x in EnumerateScreens(t.Target))
            {
                var item = new HmiItem { Target = t, Kind = HmiItemKind.Screen, TiaPath = x.Item1, Name = x.Item2.Name };
                if (!wanted.Remove(item.Key)) continue;
                var scr = x.Item2;
                if (ExportOne(f => scr.Export(f, ExportOptions.WithDefaults), outDir, item, log)) ok++; else failed++;
            }

            foreach (var missing in wanted)
            {
                log("    NAPAKA " + missing.Replace('|', ' ') + ": v projektu ne obstaja več (osveži drevo).");
                failed++;
            }
            log("  Target " + t.Name + ": exportiranih " + ok + ", napak " + failed);
        }

        static bool ExportOne(Action<FileInfo> export, string outDir, HmiItem item, Action<string> log)
        {
            string full = ItemFile(outDir, item);
            string rel = ColorReplacer.MakeRelative(outDir, full);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                if (File.Exists(full)) File.Delete(full);   // Openness Export ne prepiše obstoječe datoteke
                export(new FileInfo(full));
                log("    OK   " + rel);
                return true;
            }
            catch (Exception ex)
            {
                log("    NAPAKA " + rel + ": " + ex.Message);
                return false;
            }
        }

        // ------------------------------------------------------------------ Import

        /// <summary>
        /// Uvozi izbrane elemente nazaj v njihovo mapo (ImportOptions.Override). Najprej predloge, nato zasloni.
        /// Elementi brez exportirane datoteke se preskočijo. Projekt se NE shrani.
        /// </summary>
        public void ImportItems(HmiTargetInfo t, IList<HmiItem> items, string outDir, Action<string> log,
            out int ok, out int failed, out int skipped)
        {
            EnsureConnected();
            ok = 0; failed = 0; skipped = 0;
            log("=== Import targeta " + t.Name + ": " + items.Count + " izbranih");

            // predloge najprej - zasloni se sklicujejo nanje
            foreach (var item in items.OrderBy(i => i.Kind == HmiItemKind.Template ? 0 : 1))
            {
                string full = ItemFile(outDir, item);
                string rel = ColorReplacer.MakeRelative(outDir, full);
                if (!File.Exists(full))
                {
                    log("    PRESKOČENO " + rel + ": ni exporta.");
                    skipped++;
                    continue;
                }
                try
                {
                    if (item.Kind == HmiItemKind.Template)
                    {
                        var coll = FindTemplateCollection(t.Target, item.TiaPath);
                        if (coll == null) throw new InvalidOperationException("Mapa predlog '" + item.TiaPath + "' v projektu ne obstaja več.");
                        coll.Import(new FileInfo(full), ImportOptions.Override);
                    }
                    else
                    {
                        var coll = FindScreenCollection(t.Target, item.TiaPath);
                        if (coll == null) throw new InvalidOperationException("Mapa zaslonov '" + item.TiaPath + "' v projektu ne obstaja več.");
                        coll.Import(new FileInfo(full), ImportOptions.Override);
                    }
                    log("    OK   " + rel);
                    ok++;
                }
                catch (Exception ex)
                {
                    log("    NAPAKA " + rel + ": " + ex.Message);
                    failed++;
                }
            }
            log("  Target " + t.Name + ": uvoženih " + ok + ", napak " + failed + ", preskočenih " + skipped + ". Projekt NI shranjen.");
        }

        static ScreenComposition FindScreenCollection(HmiTarget target, string tiaPath)
        {
            var root = target.ScreenFolder;
            if (tiaPath.Length == 0) return root.Screens;
            ScreenUserFolder folder = null;
            foreach (var part in tiaPath.Split('/'))
            {
                folder = folder == null ? root.Folders.Find(part) : folder.Folders.Find(part);
                if (folder == null) return null;
            }
            return folder.Screens;
        }

        static ScreenTemplateComposition FindTemplateCollection(HmiTarget target, string tiaPath)
        {
            var root = target.ScreenTemplateFolder;
            if (tiaPath.Length == 0) return root.ScreenTemplates;
            ScreenTemplateUserFolder folder = null;
            foreach (var part in tiaPath.Split('/'))
            {
                folder = folder == null ? root.Folders.Find(part) : folder.Folders.Find(part);
                if (folder == null) return null;
            }
            return folder.ScreenTemplates;
        }

        void EnsureConnected()
        {
            if (_project == null) throw new InvalidOperationException("Ni povezave s TIA Portalom.");
        }
    }
}
