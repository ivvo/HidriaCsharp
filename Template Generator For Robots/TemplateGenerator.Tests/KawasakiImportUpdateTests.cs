using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    // Kawasaki (prenešeno iz V3): generira "ProgramFile.as"; posodobitev deluje z RE-generacijo +
    // ohranjanjem naučenih točk po imenu. Homing išče najbližjo točko po imenu (ne po indeksu).
    public class KawasakiImportUpdateTests : IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        private static string AsPath(string dir) => Path.Combine(dir, "ProgramFile.as");

        private static ShellViewModel Gen(string outDir, params (string name, bool free, int positions)[] stations)
        {
            var vm = new ShellViewModel();
            vm.ProgramName = "Robot1";
            vm.AddProgram(); // doda "Home"
            vm.SelectedProgram = vm.Program[0];
            foreach (var (name, free, positions) in stations)
            {
                vm.StationName = name;
                vm.StationFreeEnabled = free;
                vm.AddStation();
                if (positions != 1)
                    vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = positions;
            }
            vm.SelectedTemplate = "Kawasaki";
            vm.GenerateProject(outDir);
            return vm;
        }

        [Fact]
        public void Import_RoundTrips_Stations_Free_Positions()
        {
            Gen(_outDir, ("PartTake", true, 1), ("Evacuate", true, 3));

            var vm = new ShellViewModel();
            vm.ImportProject(_outDir);

            var st = vm.Program[0].Stations
                .Select(s => (s.RobotStationName, s.StationFreeEnabled, s.Positions)).ToList();
            Assert.Equal(("Home", false, 1), st[0]);
            Assert.Equal(("PartTake", true, 1), st[1]);
            Assert.Equal(("Evacuate", true, 3), st[2]);
        }

        [Fact]
        public void Import_AutoDetectedAsKawasaki()
        {
            Gen(_outDir, ("PartTake", true, 1));

            var vm = new ShellViewModel();
            vm.ImportProject(_outDir);
            Assert.Equal("Kawasaki", vm.SelectedTemplate);
            Assert.Contains("Kawasaki", vm.TextUpdate);
        }

        [Fact]
        public void Update_ByteIdenticalToFullGenerate_WithMultiPositionStation()
        {
            string goldDir = TestHelpers.CreateTempDir();
            try
            {
                Gen(goldDir, ("PartTake", true, 1), ("PartRls", true, 1), ("Evacuate", true, 2));
                string gold = File.ReadAllText(AsPath(goldDir));

                Gen(_outDir, ("PartTake", true, 1), ("PartRls", true, 1));

                var vm = new ShellViewModel();
                vm.ImportProject(_outDir);
                vm.SelectedProgram = vm.Program[0];
                vm.StationName = "Evacuate"; vm.StationFreeEnabled = true; vm.AddStation();
                vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = 2;
                vm.UpdateProject(_outDir);

                string updated = File.ReadAllText(AsPath(_outDir));
                if (gold != updated)
                    Assert.Fail(FirstDiff(gold, updated));
            }
            finally { TestHelpers.DeleteDirSafely(goldDir); }
        }

        // Ohranjanje naučenih točk pri VEČpozicijski postaji: model hrani le eno koordinato na postajo,
        // zato distinct vrednosti pWeld1 != pWeld2 preživijo SAMO zaradi prekrivanja po imenu (ne prek
        // modela). To dokazuje nujnost prekrivanja (ne le regeneracije iz modela).
        [Fact]
        public void Update_PreservesTaughtCoordinates_IncludingDistinctMultiPositionPoints()
        {
            Gen(_outDir, ("Weld", true, 2));
            string asPath = AsPath(_outDir);

            string content = File.ReadAllText(asPath);
            content = Regex.Replace(content, @"(?m)^pWeld1 .+$", "pWeld1 11.1 11.2 11.3 0 0 0");
            content = Regex.Replace(content, @"(?m)^pWeld2 .+$", "pWeld2 22.1 22.2 22.3 0 0 0");
            Assert.Contains("pWeld1 11.1 11.2 11.3", content);
            Assert.Contains("pWeld2 22.1 22.2 22.3", content);
            File.WriteAllText(asPath, content);

            var vm = new ShellViewModel();
            vm.ImportProject(_outDir);
            vm.SelectedProgram = vm.Program[0];
            vm.StationName = "Extra"; vm.StationFreeEnabled = false; vm.AddStation();
            vm.UpdateProject(_outDir);

            string after = File.ReadAllText(asPath);
            Assert.Contains("pWeld1 11.1 11.2 11.3", after);
            Assert.Contains("pWeld2 22.1 22.2 22.3", after); // distinct - dokaže, da prekrivanje deluje
            Assert.Matches(@"(?m)^pExtra ", after);           // nova postaja dodana
        }

        private static string FirstDiff(string a, string b)
        {
            var la = a.Replace("\r", "").Split('\n');
            var lb = b.Replace("\r", "").Split('\n');
            int n = Math.Min(la.Length, lb.Length);
            for (int i = 0; i < n; i++)
                if (la[i] != lb[i])
                    return $"Prva razlika v vrstici {i + 1}:\n  gold=[{la[i]}]\n  work=[{lb[i]}]";
            return $"Ujemata se do {n} vrstic; dolžini gold={la.Length} work={lb.Length}";
        }
    }
}
