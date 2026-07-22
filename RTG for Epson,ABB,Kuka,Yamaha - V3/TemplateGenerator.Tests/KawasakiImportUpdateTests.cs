using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    // Kawasaki (V3): posodobitev deluje z RE-generacijo ProgramFile.as + prekrivanjem naučenih točk
    // po imenu. Kawasaki najbližjo točko išče po imenu (ne po indeksu).
    public class KawasakiImportUpdateTests : System.IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        private static string AsPath(string proj) => Directory.GetFiles(proj, "*.as").First();

        [Fact]
        public void Import_RoundTrips_Stations_Free_Positions()
        {
            TestHelpers.BuildAndGenerate("Kawasaki Hidria", _outDir,
                ("PartTake", true, 1), ("Evacuate", true, 3));
            string proj = TestHelpers.ResolveProjectDir("Kawasaki Hidria", _outDir);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);

            var st = vm.Program[0].Stations
                .Select(s => (s.RobotStationName, s.StationFreeEnabled, s.Positions)).ToList();
            Assert.Equal(("Home", false, 1), st[0]);
            Assert.Equal(("PartTake", true, 1), st[1]);
            Assert.Equal(("Evacuate", true, 3), st[2]);
        }

        [Fact]
        public void Import_AutoDetectedAsKawasaki()
        {
            TestHelpers.BuildAndGenerate("Kawasaki Hidria", _outDir, ("PartTake", true, 1));
            string proj = TestHelpers.ResolveProjectDir("Kawasaki Hidria", _outDir);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);
            Assert.Equal("Kawasaki Hidria", vm.SelectedTemplate);
            Assert.Contains("Kawasaki Hidria", vm.TextUpdate);
        }

        [Fact]
        public void Update_ByteIdenticalToFullGenerate_WithMultiPositionStation()
        {
            string goldDir = TestHelpers.CreateTempDir();
            try
            {
                TestHelpers.BuildAndGenerate("Kawasaki Hidria", goldDir,
                    ("PartTake", true, 1), ("PartRls", true, 1), ("Evacuate", true, 2));
                string goldProj = TestHelpers.ResolveProjectDir("Kawasaki Hidria", goldDir);

                TestHelpers.BuildAndGenerate("Kawasaki Hidria", _outDir, ("PartTake", true, 1), ("PartRls", true, 1));
                string workProj = TestHelpers.ResolveProjectDir("Kawasaki Hidria", _outDir);

                var vm = new ShellViewModel();
                vm.ImportProject(workProj);
                vm.StationName = "Evacuate"; vm.StationFreeEnabled = true; vm.AddStation();
                vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = 2;
                vm.UpdateProject(workProj);

                string? diff = TestHelpers.FirstDirDifference(goldProj, workProj, ".as");
                Assert.True(diff == null, diff);
            }
            finally { TestHelpers.DeleteDirSafely(goldDir); }
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates()
        {
            TestHelpers.BuildAndGenerate("Kawasaki Hidria", _outDir, ("Station1", true, 1));
            string proj = TestHelpers.ResolveProjectDir("Kawasaki Hidria", _outDir);
            string asPath = AsPath(proj);

            // "Nauči" točko pStation1 (zamenjaj njene ničelne koordinate).
            string content = File.ReadAllText(asPath);
            string taught = Regex.Replace(content, @"(?m)^pStation1 .+$", "pStation1 111.1 222.2 333.3 44.4 55.5 66.6");
            Assert.NotEqual(content, taught);
            File.WriteAllText(asPath, taught);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);
            vm.StationName = "Station2"; vm.StationFreeEnabled = false; vm.AddStation();
            vm.UpdateProject(proj);

            string after = File.ReadAllText(AsPath(proj));
            Assert.Contains("pStation1 111.1 222.2 333.3 44.4 55.5 66.6", after);
            Assert.Matches(@"(?m)^pStation2 ", after); // nova postaja dodana
        }
    }
}
