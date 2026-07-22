using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TemplateGenerator.Core.ViewModels;

namespace TemplateGenerator.Tests
{
    public static class TestHelpers
    {
        // Prepreči, da bi ShellViewModel med testi odpiral okna Raziskovalca (glej OpenFolder).
        [System.Runtime.CompilerServices.ModuleInitializer]
        internal static void SuppressExplorer()
        {
            Environment.SetEnvironmentVariable("TGR_SUPPRESS_EXPLORER", "1");
        }

        public static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "tgr_v3_test_" + Guid.NewGuid().ToString("N").Substring(0, 10));
            Directory.CreateDirectory(dir);
            return dir;
        }

        // Najboljši trud pri čiščenju (vključno z varnostnimi kopijami *ProjectUpdater.BackupFolder) -
        // naj čiščenje samo po sebi nikoli ne podre testa.
        public static void DeleteDirSafely(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);

                string? parent = Path.GetDirectoryName(dir);
                if (parent != null)
                    foreach (var backup in Directory.GetDirectories(parent, Path.GetFileName(dir) + "_backup_*"))
                        Directory.Delete(backup, true);
            }
            catch { /* ignoriraj - čiščenje ni del testa */ }
        }

        // Zgenerira projekt z izbranim proizvajalcem. V3 ShellViewModel ob konstrukciji že ustvari
        // privzeti program "robot" s postajo "Home", zato dodajamo postaje na ta privzeti program.
        public static ShellViewModel BuildAndGenerate(string template, string outDir,
            params (string name, bool free, int positions)[] stations)
        {
            var vm = new ShellViewModel();
            vm.SelectedProgram = vm.Program[0];
            foreach (var (name, free, positions) in stations)
            {
                vm.StationName = name;
                vm.StationFreeEnabled = free;
                vm.AddStation();
                if (positions != 1)
                    vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = positions;
            }
            vm.SelectedTemplate = template;
            vm.GenerateSimpleProgram = true;
            vm.GenerateProject(outDir);
            return vm;
        }

        // Epson/Yamaha/Kawasaki pišejo v časovno označeno podmapo; KUKA/ABB pišejo neposredno v outDir.
        public static string ResolveProjectDir(string template, string outDir)
        {
            string? prefix = template switch
            {
                "Epson Hidria" => "EpsonGeneratedTemplate_",
                "Yamaha Hidria" => "YamahaGeneratedTemplate_",
                "Kawasaki Hidria" => "KawasakiGeneratedTemplate_",
                _ => null
            };
            if (prefix == null) return outDir;
            return Directory.GetDirectories(outDir, prefix + "*").First(d => !d.Contains("_backup_"));
        }

        // Ali se vse vrstice iz "before" v istem vrstnem redu pojavijo v "after" (dovoljene so samo
        // vstavitve - nobena obstoječa vrstica se ne sme izgubiti).
        public static bool IsSubsequenceOfLines(string before, string after)
        {
            var b = before.Replace("\r", "").Split('\n');
            var a = after.Replace("\r", "").Split('\n');
            int j = 0;
            foreach (var line in b)
            {
                while (j < a.Length && a[j] != line) j++;
                if (j >= a.Length) return false;
                j++;
            }
            return true;
        }

        // Primerja besedilne datoteke (po pripadajočih imenih) v dveh mapah. Vrne null, če so vse
        // enake, sicer opis prve razlike. Uporablja se za "bajt-identičnost" (update == svež generate).
        public static string? FirstDirDifference(string dirA, string dirB, params string[] extensions)
        {
            string Rel(string root, string f) => f.Substring(root.Length).TrimStart('\\', '/');
            bool Wanted(string f) => extensions.Contains(Path.GetExtension(f).ToLower());

            var a = Directory.GetFiles(dirA, "*.*", SearchOption.AllDirectories)
                .Where(Wanted).Where(f => !f.Contains("_backup_"))
                .ToDictionary(f => Rel(dirA, f).ToLower(), f => f);
            var b = Directory.GetFiles(dirB, "*.*", SearchOption.AllDirectories)
                .Where(Wanted).Where(f => !f.Contains("_backup_"))
                .ToDictionary(f => Rel(dirB, f).ToLower(), f => f);

            foreach (var key in a.Keys)
            {
                if (!b.ContainsKey(key)) return $"Datoteka '{key}' obstaja v gold, ne pa v work.";
                string ca = File.ReadAllText(a[key]);
                string cb = File.ReadAllText(b[key]);
                if (ca == cb) continue;

                var la = ca.Replace("\r", "").Split('\n');
                var lb = cb.Replace("\r", "").Split('\n');
                int n = Math.Min(la.Length, lb.Length);
                for (int i = 0; i < n; i++)
                    if (la[i] != lb[i])
                        return $"'{key}' vrstica {i + 1}:\n  gold=[{la[i]}]\n  work=[{lb[i]}]";
                return $"'{key}': enako do {n} vrstic; dolžini gold={la.Length} work={lb.Length}";
            }
            foreach (var key in b.Keys)
                if (!a.ContainsKey(key)) return $"Datoteka '{key}' obstaja v work, ne pa v gold.";
            return null;
        }

        public static string ExtractBetween(string content, string startMarker, string endMarker)
        {
            int startIdx = content.IndexOf(startMarker, StringComparison.Ordinal);
            if (startIdx < 0) return "";
            int endIdx = content.IndexOf(endMarker, startIdx + startMarker.Length, StringComparison.Ordinal);
            if (endIdx < 0) return content.Substring(startIdx);
            return content.Substring(startIdx, endIdx - startIdx);
        }
    }
}
