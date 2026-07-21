using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    // Yamaha (V3): posodobitev deluje z RE-generacijo celotne "BackupFile.all" + prekrivanjem
    // naučenih točk po imenu. Najmočnejši test je bajt-identičnost s svežim Generate celotnega seznama.
    public class YamahaImportUpdateTests : IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        private static string AllPath(string projectDir) => Path.Combine(projectDir, "BackupFile.all");

        [Fact]
        public void Import_RoundTrips_Stations_Free_Positions()
        {
            TestHelpers.BuildAndGenerate("Yamaha Hidria", _outDir,
                ("PartTake", true, 1), ("Evacuate", true, 3));
            string proj = TestHelpers.ResolveProjectDir("Yamaha Hidria", _outDir);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);

            var st = vm.Program[0].Stations
                .Select(s => (s.RobotStationName, s.StationFreeEnabled, s.Positions)).ToList();

            Assert.Equal(("Home", false, 1), st[0]);
            Assert.Equal(("PartTake", true, 1), st[1]);
            Assert.Equal(("Evacuate", true, 3), st[2]);
        }

        [Fact]
        public void Import_AutoDetectedAsYamaha()
        {
            TestHelpers.BuildAndGenerate("Yamaha Hidria", _outDir, ("PartTake", true, 1));
            string proj = TestHelpers.ResolveProjectDir("Yamaha Hidria", _outDir);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);

            Assert.Equal("Yamaha Hidria", vm.SelectedTemplate);
            Assert.Contains("Yamaha Hidria", vm.TextUpdate);
        }

        // Najmočnejši test: posodobljen projekt MORA biti bajt-za-bajt enak svežemu Generate celotnega
        // (končnega) seznama postaj (naučenih koordinat tu ni - vse so ničle, zato je vsaka razlika
        // napaka v logiki posodabljanja: homing comma-CASE, redek seznam točk z vrzeljami, IO ...).
        [Fact]
        public void Update_ByteIdenticalToFullGenerate_WithMultiPositionStation()
        {
            // Gold: celoten seznam.
            string goldDir = TestHelpers.CreateTempDir();
            try
            {
                TestHelpers.BuildAndGenerate("Yamaha Hidria", goldDir,
                    ("PartTake", true, 1), ("PartRls", true, 1), ("Evacuate", true, 2));
                string gold = File.ReadAllText(AllPath(TestHelpers.ResolveProjectDir("Yamaha Hidria", goldDir)));

                // Work: prvotni seznam -> uvoz -> dodaj -> posodobi.
                TestHelpers.BuildAndGenerate("Yamaha Hidria", _outDir, ("PartTake", true, 1), ("PartRls", true, 1));
                string proj = TestHelpers.ResolveProjectDir("Yamaha Hidria", _outDir);

                var vm = new ShellViewModel();
                vm.ImportProject(proj);
                vm.StationName = "Evacuate"; vm.StationFreeEnabled = true; vm.AddStation();
                vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = 2;
                vm.UpdateProject(proj);

                string updated = File.ReadAllText(AllPath(proj));
                if (gold != updated)
                    Assert.Fail(FirstDiff(gold, updated));
            }
            finally { TestHelpers.DeleteDirSafely(goldDir); }
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates_StationAndSafePoints()
        {
            TestHelpers.BuildAndGenerate("Yamaha Hidria", _outDir, ("Station1", true, 1));
            string proj = TestHelpers.ResolveProjectDir("Yamaha Hidria", _outDir);
            string allPath = AllPath(proj);

            // "Nauči" primarno točko pStation1 in varno točko pSafeL na prepoznavne vrednosti.
            string content = File.ReadAllText(allPath);
            content = SetPointByName(content, "pStation1", "11.111 22.222 33.333 44.444 0.000 0.000 2 0 0");
            content = SetPointByName(content, "pSafeL", "99.999 88.888 77.777 66.666 0.000 0.000 2 0 0");
            File.WriteAllText(allPath, content);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);
            vm.StationName = "Station2"; vm.StationFreeEnabled = false; vm.AddStation();
            vm.UpdateProject(proj);

            string after = File.ReadAllText(allPath);
            Assert.Contains("11.111 22.222 33.333 44.444", after); // postajna točka ohranjena
            Assert.Contains("99.999 88.888 77.777 66.666", after); // varna točka ohranjena (samo prek prekrivanja)
            Assert.Matches(@"PN\d+=pStation2", after);             // nova postaja dodana
        }

        private static string SetPointByName(string content, string pointName, string coords)
        {
            var m = Regex.Match(content, @"(?m)^PN(\d+)=" + Regex.Escape(pointName) + "$");
            Assert.True(m.Success, $"Točke {pointName} ni bilo mogoče najti v [PNM].");
            int idx = int.Parse(m.Groups[1].Value);
            return Regex.Replace(content, @"(?m)^P" + idx + "=[^\r\n]*$", "P" + idx + "=" + coords);
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
