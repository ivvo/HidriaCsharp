using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Communication;
using Siemens.Engineering.Hmi.Cycle;
using Siemens.Engineering.Hmi.Globalization;
using Siemens.Engineering.Hmi.RuntimeScripting;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.Hmi.TextGraphicList;
using Siemens.Engineering.Library.Types;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.ExternalSources;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.SW.WatchAndForceTables;

namespace TIA_ToolKit
{
    /// <summary>
    /// Kategorije za "Export all". Seznam je preverjen proti V20 Siemens.Engineering.xml (Export metode).
    /// NI izvozljivo prek Openness (Comfort/Advanced): HMI alarmi, recepti, uporabniki, arhivi, master kopije.
    /// </summary>
    enum ExportCategory
    {
        PlcBlocks, PlcSources, PlcTypes, PlcTags, PlcWatchTables,
        HmiScreens, HmiTags, HmiLists, HmiScripts, HmiCyclesConnections,
        ProjectGraphics, LibraryTypes
    }

    static class ExportCategories
    {
        public static readonly ExportCategory[] All = (ExportCategory[])Enum.GetValues(typeof(ExportCategory));

        public static string Label(ExportCategory c)
        {
            switch (c)
            {
                case ExportCategory.PlcBlocks: return "PLC bloki (OB/FB/FC/DB) - SimaticML XML";
                case ExportCategory.PlcSources: return "PLC izvorna koda (SCL/STL/DB/UDT kot tekst) - samo tekstovni jeziki";
                case ExportCategory.PlcTypes: return "PLC tipi (UDT)";
                case ExportCategory.PlcTags: return "PLC tag tabele (tagi + konstante)";
                case ExportCategory.PlcWatchTables: return "PLC watch / force tabele";
                case ExportCategory.HmiScreens: return "HMI zasloni, predloge, pop-upi, slide-ini, globalni elementi";
                case ExportCategory.HmiTags: return "HMI tag tabele";
                case ExportCategory.HmiLists: return "HMI text liste in graphic liste";
                case ExportCategory.HmiScripts: return "HMI VB skripte";
                case ExportCategory.HmiCyclesConnections: return "HMI cikli in povezave";
                case ExportCategory.ProjectGraphics: return "Grafike projekta (slike)";
                case ExportCategory.LibraryTypes: return "Projektna knjižnica - tipi (vse verzije)";
                default: return c.ToString();
            }
        }

        /// <summary>Izvorna koda je počasna in pri LAD/FBD vedno pade - privzeto izklopljena.</summary>
        public static bool DefaultOn(ExportCategory c) { return c != ExportCategory.PlcSources; }
    }

    sealed partial class TiaService
    {
        /// <summary>Nastavi UI thread (gumb "Prekini"); preverja se med posameznimi objekti.</summary>
        public volatile bool CancelRequested;

        public const string ManifestName = "manifest.json";
        public const string ExportLogName = "export_log.txt";

        /// <summary>Korenska mapa "Export all" za ta projekt: outDir\&lt;projekt&gt;.</summary>
        public string ExportAllRoot(string outDir)
        {
            EnsureConnected();
            return Path.Combine(outDir, Sanitize(_project.Name));
        }

        // ------------------------------------------------------------------ Kontekst izvoza

        sealed class Counter { public int Exported, Skipped, Failed; }

        sealed class Problem { public string Category, Object, Status, Reason; }

        sealed class TableInfo { public string Path; public int Tags; public int Constants; }

        sealed class PlcInfo
        {
            public string Name, Device;
            public readonly SortedDictionary<string, int> BlocksByType = new SortedDictionary<string, int>();
            public readonly SortedDictionary<string, int> BlocksByLanguage = new SortedDictionary<string, int>();
            public int Types, WatchTables, ForceTables;
            public readonly List<TableInfo> TagTables = new List<TableInfo>();
        }

        sealed class HmiInfo
        {
            public string Name, Device;
            public int Screens, Templates, Popups, Slideins, TextLists, GraphicLists, Scripts, Cycles, Connections;
            public readonly List<TableInfo> TagTables = new List<TableInfo>();
        }

        sealed class ExportCtx
        {
            public string Root;
            public HashSet<ExportCategory> Categories;
            public Action<string> UiLog;
            public StreamWriter FileLog;
            public readonly Dictionary<ExportCategory, Counter> Counters = new Dictionary<ExportCategory, Counter>();
            public readonly List<Problem> Problems = new List<Problem>();
            public readonly List<PlcInfo> Plcs = new List<PlcInfo>();
            public readonly List<HmiInfo> Hmis = new List<HmiInfo>();
            public int GraphicsCount, LibraryTypeVersions;

            public bool Has(ExportCategory c) { return Categories.Contains(c); }

            public Counter Count(ExportCategory c)
            {
                Counter x;
                if (!Counters.TryGetValue(c, out x)) Counters[c] = x = new Counter();
                return x;
            }

            public void Log(string msg)
            {
                UiLog(msg);
                FileLog.WriteLine(DateTime.Now.ToString("HH:mm:ss") + "  " + msg);
            }
        }

        // ------------------------------------------------------------------ Glavna metoda

        /// <summary>
        /// Izvozi vse izbrane kategorije v outDir\&lt;projekt&gt;\ (mapa mora biti prazna ali neobstoječa -
        /// čiščenje naredi klicatelj). Samo bere: nič ne uvozi, ne prevaja in ne shrani projekta.
        /// Napaka pri enem objektu ne ustavi izvoza; vse gre v export_log.txt in manifest.json.
        /// </summary>
        public void ExportAll(string outDir, ICollection<ExportCategory> categories, Action<string> log,
            out int exported, out int skipped, out int failed)
        {
            EnsureConnected();
            CancelRequested = false;
            var root = ExportAllRoot(outDir);
            Directory.CreateDirectory(root);

            var ctx = new ExportCtx { Root = root, Categories = new HashSet<ExportCategory>(categories), UiLog = log };
            bool cancelled = false;
            var started = DateTime.Now;

            using (ctx.FileLog = new StreamWriter(Path.Combine(root, ExportLogName), false, new UTF8Encoding(true)))
            {
                ctx.Log("=== EXPORT ALL projekta " + _project.Name + " -> " + root);
                ctx.Log("    Kategorije: " + string.Join(", ", ExportCategories.All.Where(ctx.Has)));
                try
                {
                    var seen = new HashSet<object>();
                    foreach (var device in GetAllDevices(_project))
                        foreach (var item in AllDeviceItemsRecursive(device.DeviceItems))
                        {
                            var container = item.GetService<SoftwareContainer>();
                            var sw = container == null ? null : container.Software;
                            if (sw == null || !seen.Add(sw)) continue;
                            var plc = sw as PlcSoftware;
                            var hmi = sw as HmiTarget;
                            if (plc != null) ExportPlc(ctx, plc, device.Name);
                            else if (hmi != null) ExportHmi(ctx, hmi, device.Name);
                        }

                    if (ctx.Has(ExportCategory.ProjectGraphics)) ExportGraphics(ctx);
                    if (ctx.Has(ExportCategory.LibraryTypes)) ExportLibrary(ctx);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                    ctx.Log("=== PREKINJENO s strani uporabnika - izvoz je NEPOPOLN.");
                }

                exported = ctx.Counters.Values.Sum(c => c.Exported);
                skipped = ctx.Counters.Values.Sum(c => c.Skipped);
                failed = ctx.Counters.Values.Sum(c => c.Failed);

                WriteManifest(ctx, started, cancelled);
                ctx.Log("=== EXPORT ALL KONČAN" + (cancelled ? " (PREKINJEN)" : "") + ": " + exported + " datotek, " +
                        skipped + " preskočenih, " + failed + " napak, trajanje " +
                        (DateTime.Now - started).ToString(@"mm\:ss") + ". Projekt NI spremenjen.");
            }
        }

        // ------------------------------------------------------------------ Pomožno: export enega objekta

        void CheckCancel()
        {
            if (CancelRequested) throw new OperationCanceledException();
        }

        /// <summary>Exporta en objekt v Root\relPath. Napaka se zabeleži, izvoz teče naprej.</summary>
        bool Exp(ExportCtx c, ExportCategory cat, string relPath, Action<FileInfo> export)
        {
            CheckCancel();
            string full = UniquePath(Path.Combine(c.Root, relPath));
            string rel = PathUtil.MakeRelative(c.Root, full);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                export(new FileInfo(full));
                c.Count(cat).Exported++;
                c.Log("    OK   " + rel);
                return true;
            }
            catch (Exception ex)
            {
                c.Count(cat).Failed++;
                string reason = ex.Message.Replace("\r", " ").Replace("\n", " ");
                c.Problems.Add(new Problem { Category = cat.ToString(), Object = rel, Status = "failed", Reason = reason });
                c.Log("    NAPAKA " + rel + ": " + reason);
                return false;
            }
        }

        void Skip(ExportCtx c, ExportCategory cat, string rel, string reason)
        {
            c.Count(cat).Skipped++;
            c.Problems.Add(new Problem { Category = cat.ToString(), Object = rel, Status = "skipped", Reason = reason });
            c.Log("    PRESKOČENO " + rel + ": " + reason);
        }

        /// <summary>Openness Export ne prepiše datoteke; dvojna imena (po Sanitize) dobijo pripono _2, _3 ...</summary>
        static string UniquePath(string full)
        {
            if (!File.Exists(full)) return full;
            string dir = Path.GetDirectoryName(full), name = Path.GetFileNameWithoutExtension(full), ext = Path.GetExtension(full);
            for (int i = 2; ; i++)
            {
                string p = Path.Combine(dir, name + "_" + i + ext);
                if (!File.Exists(p)) return p;
            }
        }

        static string Join(string dir, string name) { return dir.Length == 0 ? Sanitize(name) : Path.Combine(dir, Sanitize(name)); }

        static string TreePath(string dir) { return dir.Replace('\\', '/'); }

        // ------------------------------------------------------------------ PLC

        void ExportPlc(ExportCtx c, PlcSoftware plc, string device)
        {
            var info = new PlcInfo { Name = plc.Name, Device = device };
            c.Plcs.Add(info);
            string baseDir = Path.Combine("PLC", Sanitize(plc.Name));
            c.Log("--- PLC " + plc.Name + " [" + device + "]");

            if (c.Has(ExportCategory.PlcBlocks) || c.Has(ExportCategory.PlcSources))
                ExportBlockGroup(c, plc, plc.BlockGroup, "", baseDir, info);
            if (c.Has(ExportCategory.PlcTypes) || c.Has(ExportCategory.PlcSources))
                ExportTypeGroup(c, plc, plc.TypeGroup, "", baseDir, info);
            if (c.Has(ExportCategory.PlcTags))
                ExportPlcTagGroup(c, plc.TagTableGroup, "", baseDir, info);
            if (c.Has(ExportCategory.PlcWatchTables))
                ExportWatchGroup(c, plc.WatchAndForceTableGroup, "", baseDir, info);
        }

        void ExportBlockGroup(ExportCtx c, PlcSoftware plc, PlcBlockGroup g, string sub, string baseDir, PlcInfo info)
        {
            foreach (PlcBlock b in g.Blocks)
            {
                CheckCancel();
                string type = b.GetType().Name;
                string lang = SafeLanguage(b);
                Increment(info.BlocksByType, type);
                Increment(info.BlocksByLanguage, lang);
                string rel = Path.Combine(baseDir, "Blocks", Join(sub, b.Name));

                if (b.IsKnowHowProtected)
                {
                    if (c.Has(ExportCategory.PlcBlocks)) Skip(c, ExportCategory.PlcBlocks, rel, "know-how zaščiten");
                    continue;
                }
                if (!b.IsConsistent)
                {
                    // Prevajanja namenoma NE delamo - export ne sme spreminjati projekta.
                    if (c.Has(ExportCategory.PlcBlocks)) Skip(c, ExportCategory.PlcBlocks, rel, "nekonsistenten (prevedi v TIA, nato ponovi)");
                    continue;
                }
                var block = b;
                if (c.Has(ExportCategory.PlcBlocks))
                    Exp(c, ExportCategory.PlcBlocks, rel + ".xml", f => block.Export(f, ExportOptions.WithDefaults));
                if (c.Has(ExportCategory.PlcSources))
                    ExportSource(c, plc, block as IGenerateSource, Path.Combine(baseDir, "Sources", Join(sub, b.Name)), SourceExtension(lang));
            }
            foreach (PlcBlockUserGroup s in g.Groups)
                ExportBlockGroup(c, plc, s, Join(sub, s.Name), baseDir, info);
        }

        void ExportTypeGroup(ExportCtx c, PlcSoftware plc, PlcTypeGroup g, string sub, string baseDir, PlcInfo info)
        {
            foreach (PlcType t in g.Types)
            {
                CheckCancel();
                info.Types++;
                var type = t;
                if (c.Has(ExportCategory.PlcTypes))
                    Exp(c, ExportCategory.PlcTypes, Path.Combine(baseDir, "Types", Join(sub, t.Name)) + ".xml",
                        f => type.Export(f, ExportOptions.WithDefaults));
                if (c.Has(ExportCategory.PlcSources))
                    ExportSource(c, plc, type as IGenerateSource, Path.Combine(baseDir, "Sources", "Types", Join(sub, t.Name)), ".udt");
            }
            foreach (PlcTypeUserGroup s in g.Groups)
                ExportTypeGroup(c, plc, s, Join(sub, s.Name), baseDir, info);
        }

        void ExportSource(ExportCtx c, PlcSoftware plc, IGenerateSource src, string relNoExt, string ext)
        {
            if (src == null || ext == null)
            {
                Skip(c, ExportCategory.PlcSources, relNoExt, "jezik nima tekstovnega vira (LAD/FBD/GRAPH ...)");
                return;
            }
            Exp(c, ExportCategory.PlcSources, relNoExt + ext,
                f => plc.ExternalSourceGroup.GenerateSource(new[] { src }, f, GenerateOptions.None));
        }

        static string SourceExtension(string lang)
        {
            switch (lang)
            {
                case "SCL": return ".scl";
                case "STL": return ".awl";
                case "DB": return ".db";
                default: return null;
            }
        }

        static string SafeLanguage(PlcBlock b)
        {
            try { return b.ProgrammingLanguage.ToString(); }
            catch { return "?"; }
        }

        void ExportPlcTagGroup(ExportCtx c, PlcTagTableGroup g, string sub, string baseDir, PlcInfo info)
        {
            foreach (PlcTagTable t in g.TagTables)
            {
                var table = t;
                info.TagTables.Add(new TableInfo { Path = TreePath(Join(sub, t.Name)), Tags = t.Tags.Count, Constants = t.UserConstants.Count });
                Exp(c, ExportCategory.PlcTags, Path.Combine(baseDir, "Tags", Join(sub, t.Name)) + ".xml",
                    f => table.Export(f, ExportOptions.WithDefaults));
            }
            foreach (PlcTagTableUserGroup s in g.Groups)
                ExportPlcTagGroup(c, s, Join(sub, s.Name), baseDir, info);
        }

        void ExportWatchGroup(ExportCtx c, PlcWatchAndForceTableGroup g, string sub, string baseDir, PlcInfo info)
        {
            foreach (PlcWatchTable t in g.WatchTables)
            {
                var table = t;
                info.WatchTables++;
                Exp(c, ExportCategory.PlcWatchTables, Path.Combine(baseDir, "WatchTables", Join(sub, t.Name)) + ".xml",
                    f => table.Export(f, ExportOptions.WithDefaults));
            }
            foreach (PlcForceTable t in g.ForceTables)
            {
                var table = t;
                info.ForceTables++;
                Exp(c, ExportCategory.PlcWatchTables, Path.Combine(baseDir, "ForceTables", Join(sub, t.Name)) + ".xml",
                    f => table.Export(f, ExportOptions.WithDefaults));
            }
            foreach (PlcWatchAndForceTableUserGroup s in g.Groups)
                ExportWatchGroup(c, s, Join(sub, s.Name), baseDir, info);
        }

        static void Increment(SortedDictionary<string, int> d, string key)
        {
            int n;
            d.TryGetValue(key, out n);
            d[key] = n + 1;
        }

        // ------------------------------------------------------------------ HMI

        void ExportHmi(ExportCtx c, HmiTarget t, string device)
        {
            var info = new HmiInfo { Name = t.Name, Device = device };
            c.Hmis.Add(info);
            string baseDir = Path.Combine("HMI", Sanitize(t.Name));
            c.Log("--- HMI " + t.Name + " [" + device + "]");

            if (c.Has(ExportCategory.HmiScreens))
            {
                foreach (var x in EnumerateTemplates(t))
                {
                    var tpl = x.Item2;
                    info.Templates++;
                    Exp(c, ExportCategory.HmiScreens, Path.Combine(baseDir, "Templates", Join(SubDir(x.Item1), tpl.Name)) + ".xml",
                        f => tpl.Export(f, ExportOptions.WithDefaults));
                }
                foreach (var x in EnumerateScreens(t))
                {
                    var scr = x.Item2;
                    info.Screens++;
                    Exp(c, ExportCategory.HmiScreens, Path.Combine(baseDir, "Screens", Join(SubDir(x.Item1), scr.Name)) + ".xml",
                        f => scr.Export(f, ExportOptions.WithDefaults));
                }
                if (t.ScreenPopupFolder != null)
                    ExportPopupFolder(c, t.ScreenPopupFolder, "", baseDir, info);
                if (t.ScreenSlideinFolder != null)
                    foreach (ScreenSlidein s in t.ScreenSlideinFolder.ScreenSlideins)
                    {
                        var slidein = s;
                        info.Slideins++;
                        Exp(c, ExportCategory.HmiScreens, Path.Combine(baseDir, "Slideins", Sanitize(s.SlideinType.ToString())) + ".xml",
                            f => slidein.Export(f, ExportOptions.WithDefaults));
                    }
                var global = t.ScreenGlobalElements;
                if (global != null)
                    Exp(c, ExportCategory.HmiScreens, Path.Combine(baseDir, "ScreenGlobalElements.xml"),
                        f => global.Export(f, ExportOptions.WithDefaults));
                var overview = t.ScreenOverview;
                if (overview != null)
                    Exp(c, ExportCategory.HmiScreens, Path.Combine(baseDir, "ScreenOverview.xml"),
                        f => overview.Export(f, ExportOptions.WithDefaults));
            }

            if (c.Has(ExportCategory.HmiTags))
                ExportHmiTagFolder(c, t.TagFolder, "", baseDir, info);

            if (c.Has(ExportCategory.HmiLists))
            {
                foreach (TextList l in t.TextLists)
                {
                    var list = l;
                    info.TextLists++;
                    Exp(c, ExportCategory.HmiLists, Path.Combine(baseDir, "TextLists", Sanitize(l.Name)) + ".xml",
                        f => list.Export(f, ExportOptions.WithDefaults));
                }
                foreach (GraphicList l in t.GraphicLists)
                {
                    var list = l;
                    info.GraphicLists++;
                    Exp(c, ExportCategory.HmiLists, Path.Combine(baseDir, "GraphicLists", Sanitize(l.Name)) + ".xml",
                        f => list.Export(f, ExportOptions.WithDefaults));
                }
            }

            if (c.Has(ExportCategory.HmiScripts))
                ExportScriptFolder(c, t.VBScriptFolder, "", baseDir, info);

            if (c.Has(ExportCategory.HmiCyclesConnections))
            {
                foreach (Cycle cy in t.Cycles)
                {
                    var cycle = cy;
                    info.Cycles++;
                    Exp(c, ExportCategory.HmiCyclesConnections, Path.Combine(baseDir, "Cycles", Sanitize(cy.Name)) + ".xml",
                        f => cycle.Export(f, ExportOptions.WithDefaults));
                }
                foreach (Connection cn in t.Connections)
                {
                    var conn = cn;
                    info.Connections++;
                    Exp(c, ExportCategory.HmiCyclesConnections, Path.Combine(baseDir, "Connections", Sanitize(cn.Name)) + ".xml",
                        f => conn.Export(f, ExportOptions.WithDefaults));
                }
            }
        }

        /// <summary>TIA pot "a/b" -> relativna pot mape s Sanitize po segmentih.</summary>
        static string SubDir(string tiaPath)
        {
            return tiaPath.Length == 0 ? "" : Path.Combine(tiaPath.Split('/').Select(Sanitize).ToArray());
        }

        void ExportPopupFolder(ExportCtx c, ScreenPopupFolder folder, string sub, string baseDir, HmiInfo info)
        {
            foreach (ScreenPopup p in folder.ScreenPopups)
            {
                var popup = p;
                info.Popups++;
                Exp(c, ExportCategory.HmiScreens, Path.Combine(baseDir, "Popups", Join(sub, p.Name)) + ".xml",
                    f => popup.Export(f, ExportOptions.WithDefaults));
            }
            foreach (ScreenPopupUserFolder s in folder.Folders)
                ExportPopupFolder(c, s, Join(sub, s.Name), baseDir, info);
        }

        void ExportHmiTagFolder(ExportCtx c, TagFolder folder, string sub, string baseDir, HmiInfo info)
        {
            foreach (TagTable t in folder.TagTables)
            {
                var table = t;
                info.TagTables.Add(new TableInfo { Path = TreePath(Join(sub, t.Name)), Tags = t.Tags.Count });
                Exp(c, ExportCategory.HmiTags, Path.Combine(baseDir, "Tags", Join(sub, t.Name)) + ".xml",
                    f => table.Export(f, ExportOptions.WithDefaults));
            }
            foreach (TagUserFolder s in folder.Folders)
                ExportHmiTagFolder(c, s, Join(sub, s.Name), baseDir, info);
        }

        void ExportScriptFolder(ExportCtx c, VBScriptFolder folder, string sub, string baseDir, HmiInfo info)
        {
            foreach (VBScript s in folder.VBScripts)
            {
                var script = s;
                info.Scripts++;
                Exp(c, ExportCategory.HmiScripts, Path.Combine(baseDir, "Scripts", Join(sub, s.Name)) + ".xml",
                    f => script.Export(f, ExportOptions.WithDefaults));
            }
            foreach (VBScriptUserFolder s in folder.Folders)
                ExportScriptFolder(c, s, Join(sub, s.Name), baseDir, info);
        }

        // ------------------------------------------------------------------ Projekt: grafike, knjižnica

        void ExportGraphics(ExportCtx c)
        {
            c.Log("--- Grafike projekta");
            foreach (MultiLingualGraphic g in _project.Graphics)
            {
                var graphic = g;
                c.GraphicsCount++;
                Exp(c, ExportCategory.ProjectGraphics, Path.Combine("Graphics", Sanitize(g.Name)) + ".xml",
                    f => graphic.Export(f, ExportOptions.WithDefaults));
            }
        }

        void ExportLibrary(ExportCtx c)
        {
            c.Log("--- Projektna knjižnica (tipi)");
            ExportLibraryFolder(c, _project.ProjectLibrary.TypeFolder, "");
        }

        void ExportLibraryFolder(ExportCtx c, LibraryTypeFolder folder, string sub)
        {
            foreach (LibraryType t in folder.Types)
                foreach (LibraryTypeVersion v in t.Versions)
                {
                    var version = v;
                    c.LibraryTypeVersions++;
                    string name = t.Name + "_V" + v.VersionNumber;
                    Exp(c, ExportCategory.LibraryTypes, Path.Combine("Library", "Types", Join(sub, name)) + ".xml",
                        f => version.Export(f, ExportOptions.WithDefaults));
                }
            foreach (LibraryTypeFolder s in folder.Folders)
                ExportLibraryFolder(c, s, Join(sub, s.Name));
        }

        // ------------------------------------------------------------------ manifest.json

        void WriteManifest(ExportCtx c, DateTime started, bool cancelled)
        {
            var j = new JsonWriter();
            j.Begin();
            j.Prop("tool", "TIA_ToolKit - Export all");
            j.Prop("project", _project.Name);
            j.Prop("projectPath", _project.Path == null ? "" : _project.Path.FullName);
            j.Prop("exportedAt", started.ToString("yyyy-MM-ddTHH:mm:ss"));
            j.Prop("cancelled", cancelled);
            j.Array("categories", ExportCategories.All.Where(c.Has).Select(x => (object)x.ToString()));

            j.BeginObj("summary");
            foreach (var cat in ExportCategories.All.Where(c.Has))
            {
                var n = c.Count(cat);
                j.BeginObj(cat.ToString());
                j.Prop("exported", n.Exported); j.Prop("skipped", n.Skipped); j.Prop("failed", n.Failed);
                j.End();
            }
            j.End();

            j.BeginArr("plc");
            foreach (var p in c.Plcs)
            {
                j.BeginObj(null);
                j.Prop("name", p.Name); j.Prop("device", p.Device);
                j.Prop("blockCount", p.BlocksByType.Values.Sum());
                j.Dict("blocksByType", p.BlocksByType);
                j.Dict("blocksByLanguage", p.BlocksByLanguage);
                j.Prop("udtCount", p.Types);
                j.Prop("watchTables", p.WatchTables); j.Prop("forceTables", p.ForceTables);
                j.Prop("tagCount", p.TagTables.Sum(t => t.Tags));
                j.Prop("constantCount", p.TagTables.Sum(t => t.Constants));
                WriteTables(j, p.TagTables, true);
                j.End();
            }
            j.EndArr();

            j.BeginArr("hmi");
            foreach (var h in c.Hmis)
            {
                j.BeginObj(null);
                j.Prop("name", h.Name); j.Prop("device", h.Device);
                j.Prop("screens", h.Screens); j.Prop("templates", h.Templates); j.Prop("popups", h.Popups); j.Prop("slideins", h.Slideins);
                j.Prop("textLists", h.TextLists); j.Prop("graphicLists", h.GraphicLists); j.Prop("scripts", h.Scripts);
                j.Prop("cycles", h.Cycles); j.Prop("connections", h.Connections);
                j.Prop("tagCount", h.TagTables.Sum(t => t.Tags));
                WriteTables(j, h.TagTables, false);
                j.End();
            }
            j.EndArr();

            j.Prop("graphics", c.GraphicsCount);
            j.Prop("libraryTypeVersions", c.LibraryTypeVersions);

            j.BeginArr("problems");
            foreach (var p in c.Problems)
            {
                j.BeginObj(null);
                j.Prop("category", p.Category); j.Prop("object", TreePath(p.Object)); j.Prop("status", p.Status); j.Prop("reason", p.Reason);
                j.End();
            }
            j.EndArr();
            j.End();

            File.WriteAllText(Path.Combine(c.Root, ManifestName), j.ToString(), new UTF8Encoding(false));
            c.Log("    manifest.json zapisan.");
        }

        static void WriteTables(JsonWriter j, List<TableInfo> tables, bool withConstants)
        {
            j.BeginArr("tagTables");
            foreach (var t in tables)
            {
                j.BeginObj(null);
                j.Prop("path", t.Path); j.Prop("tags", t.Tags);
                if (withConstants) j.Prop("constants", t.Constants);
                j.End();
            }
            j.EndArr();
        }

        /// <summary>Minimalni JSON pisalnik (.NET 4.8 brez dodatnih referenc), zamik 2 presledka.</summary>
        sealed class JsonWriter
        {
            readonly StringBuilder _sb = new StringBuilder();
            readonly Stack<bool> _first = new Stack<bool>();

            void Next(string name)
            {
                if (_first.Count > 0)
                {
                    if (!_first.Pop()) _sb.Append(',');
                    _first.Push(false);
                    _sb.Append('\n').Append(' ', _first.Count * 2);
                }
                if (name != null) _sb.Append(Str(name)).Append(": ");
            }

            public void Begin() { _sb.Append('{'); _first.Push(true); }
            public void BeginObj(string name) { Next(name); _sb.Append('{'); _first.Push(true); }
            public void BeginArr(string name) { Next(name); _sb.Append('['); _first.Push(true); }
            public void End() { Close('}'); }
            public void EndArr() { Close(']'); }

            void Close(char ch)
            {
                bool empty = _first.Pop();
                if (!empty) _sb.Append('\n').Append(' ', _first.Count * 2);
                _sb.Append(ch);
            }

            public void Prop(string name, string value) { Next(name); _sb.Append(Str(value)); }
            public void Prop(string name, int value) { Next(name); _sb.Append(value.ToString(CultureInfo.InvariantCulture)); }
            public void Prop(string name, bool value) { Next(name); _sb.Append(value ? "true" : "false"); }

            public void Array(string name, IEnumerable<object> values)
            {
                Next(name);
                _sb.Append('[').Append(string.Join(", ", values.Select(v => Str(Convert.ToString(v, CultureInfo.InvariantCulture))))).Append(']');
            }

            public void Dict(string name, IDictionary<string, int> d)
            {
                BeginObj(name);
                foreach (var kv in d) Prop(kv.Key, kv.Value);
                End();
            }

            static string Str(string s)
            {
                var sb = new StringBuilder("\"");
                foreach (char ch in s ?? "")
                {
                    switch (ch)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (ch < 0x20) sb.Append("\\u").Append(((int)ch).ToString("x4"));
                            else sb.Append(ch);
                            break;
                    }
                }
                return sb.Append('"').ToString();
            }

            public override string ToString() { return _sb.ToString() + "\n"; }
        }
    }
}
