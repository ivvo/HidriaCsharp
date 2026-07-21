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
