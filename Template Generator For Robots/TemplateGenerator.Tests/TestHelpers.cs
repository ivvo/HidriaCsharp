using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.ViewModels;

namespace TemplateGenerator.Tests
{
    public static class TestHelpers
    {
        // Prepreči, da bi ShellViewModel med testi odpiral okna Raziskovalca (glej OpenFolder).
        // ModuleInitializer se izvede enkrat ob nalaganju testne zbirke, pred katerimkoli testom.
        [System.Runtime.CompilerServices.ModuleInitializer]
        internal static void SuppressExplorer()
        {
            Environment.SetEnvironmentVariable("TGR_SUPPRESS_EXPLORER", "1");
        }

        public static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "tgr_test_" + Guid.NewGuid().ToString("N").Substring(0, 10));
            Directory.CreateDirectory(dir);
            return dir;
        }

        // Najboljši trud pri čiščenju (vključno z varnostno kopijo, ki jo naredi *ProjectUpdater.BackupFolder) -
        // naj čiščenje samo po sebi nikoli ne podre testa.
        public static void DeleteDirSafely(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);

                string parent = Path.GetDirectoryName(dir) ?? Path.GetTempPath();
                foreach (var backup in Directory.GetDirectories(parent, Path.GetFileName(dir) + "_backup_*"))
                    Directory.Delete(backup, true);
            }
            catch
            {
                // ignoriraj - čiščenje ni del testa, ki se preverja
            }
        }

        public static ShellViewModel BuildAndGenerate(string template, string outDir, params (string name, bool free)[] stations)
        {
            var vm = new ShellViewModel();
            vm.ProgramName = "Robot1";
            vm.AddProgram();
            vm.SelectedProgram = vm.Program[0];

            foreach (var (name, free) in stations)
            {
                vm.StationName = name;
                vm.StationFreeEnabled = free;
                vm.AddStation();
            }

            vm.SelectedTemplate = template;
            vm.GenerateSimpleProgram = true;
            vm.GenerateProject(outDir);
            return vm;
        }

        // Ali se vse vrstice iz "original" v istem vrstnem redu pojavijo v "updated" (dovoljene so
        // samo VSTAVITVE - nobena obstoječa vrstica se ne sme odstraniti ali spremeniti).
        public static bool IsSubsequenceOfLines(string original, string updated)
        {
            var originalLines = original.Split('\n');
            var updatedLines = updated.Split('\n');
            int j = 0;
            foreach (var line in originalLines)
            {
                while (j < updatedLines.Length && updatedLines[j] != line) j++;
                if (j >= updatedLines.Length) return false;
                j++;
            }
            return true;
        }

        // Strožja različica: vsaka vrstica iz "before", ki je dobesedno ne najdemo v "after", mora
        // ustrezati enemu od "allowedChangedLinePatterns" (namerna sprememba obstoječe vrstice - npr.
        // Epsonov "For index = 0 To N", ki se namerno poveča ob vsaki dodani postaji). Karkoli drugega,
        // kar izgine, je nepričakovana izguba/poškodba obstoječe vsebine.
        public static void AssertOnlyExpectedLineChanges(string before, string after, params string[] allowedChangedLinePatterns)
        {
            var afterSet = new System.Collections.Generic.HashSet<string>(after.Split('\n'));
            var unexplained = before.Split('\n')
                .Where(line => !afterSet.Contains(line))
                .Where(line => !allowedChangedLinePatterns.Any(p => Regex.IsMatch(line, p)))
                .ToList();

            Xunit.Assert.True(unexplained.Count == 0,
                "Nepričakovano spremenjene/odstranjene vrstice:\n" + string.Join("\n", unexplained));
        }

        public static string ExtractBetween(string content, string startMarker, string endMarker)
        {
            int startIdx = content.IndexOf(startMarker, StringComparison.Ordinal);
            if (startIdx < 0) return "";
            int endIdx = content.IndexOf(endMarker, startIdx, StringComparison.Ordinal);
            if (endIdx < 0) return content.Substring(startIdx);
            return content.Substring(startIdx, endIdx - startIdx);
        }
    }
}
