using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    // Epson (V3): posodobitev deluje z RE-generacijo vseh datotek + prekrivanjem naučenih .pts točk
    // po imenu (sLabel). Najmočnejši test je bajt-identičnost s svežim Generate celotnega seznama.
    public class EpsonImportUpdateTests : System.IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Fact]
        public void Import_RoundTrips_StationListAndFlags()
        {
            var vm = TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true, 1), ("Station2", false, 1));
            string proj = TestHelpers.ResolveProjectDir("Epson Hidria", _outDir);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(proj);

            var original = vm.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            var imported = vm2.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            Assert.Equal(original, imported);
        }

        // Regresija: projekt s pomožnimi programi brez go-funkcij (komunikacija/strojni vid, npr.
        // getOrientation.prg, HandIO.prg) se mora vseeno uvoziti - te datoteke se preskočijo, ne pokvarijo uvoza.
        [Fact]
        public void Import_IgnoresHelperPrgFiles_WithoutGoFunctions()
        {
            TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true, 1), ("Station2", false, 1));
            string proj = TestHelpers.ResolveProjectDir("Epson Hidria", _outDir);
            File.WriteAllText(Path.Combine(proj, "getOrientation.prg"), "Function getOrientation()\n\tPrint \"vision\"\nFend\n");
            File.WriteAllText(Path.Combine(proj, "HandIO.prg"), "Function handIO()\n\tPrint \"io\"\nFend\n");

            var vm = new ShellViewModel();
            vm.ImportProject(proj);

            Assert.Contains("Epson Hidria", vm.TextUpdate);
            Assert.Single(vm.Program);
            Assert.Contains(vm.Program[0].Stations, s => s.RobotStationName == "Station1");
            Assert.Contains(vm.Program[0].Stations, s => s.RobotStationName == "Station2");
        }

        [Fact]
        public void Import_ReadsTaughtCoordinates_IntoModel()
        {
            TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true, 1));
            string proj = TestHelpers.ResolveProjectDir("Epson Hidria", _outDir);
            string ptsPath = Directory.GetFiles(proj, "*.pts").First();

            // "Nauči" točko pStation1 (nastavi rX/rY/rZ na prepoznavne vrednosti).
            string content = File.ReadAllText(ptsPath);
            string taught = Regex.Replace(content,
                "(sLabel=\"pStation1\"[\\s\\S]*?rX=)[-\\d.]+([\\s\\S]*?rY=)[-\\d.]+([\\s\\S]*?rZ=)[-\\d.]+",
                "${1}12.5${2}-34.75${3}6.0");
            Assert.NotEqual(content, taught);
            File.WriteAllText(ptsPath, taught);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);

            var station = vm.Program[0].Stations.Single(s => s.RobotStationName == "Station1");
            Assert.Equal(12.5, station.Xcord, 3);
            Assert.Equal(-34.75, station.Ycord, 3);
            Assert.Equal(6.0, station.Zcord, 3);
        }

        [Fact]
        public void Update_ByteIdenticalToFullGenerate_WithMultiPositionStation()
        {
            string goldDir = TestHelpers.CreateTempDir();
            try
            {
                TestHelpers.BuildAndGenerate("Epson Hidria", goldDir,
                    ("Station1", true, 1), ("Station2", true, 1), ("Station3", false, 2));
                string goldProj = TestHelpers.ResolveProjectDir("Epson Hidria", goldDir);

                TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true, 1), ("Station2", true, 1));
                string workProj = TestHelpers.ResolveProjectDir("Epson Hidria", _outDir);

                var vm = new ShellViewModel();
                vm.ImportProject(workProj);
                vm.StationName = "Station3"; vm.StationFreeEnabled = false; vm.AddStation();
                vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = 2;
                vm.UpdateProject(workProj);

                string? diff = TestHelpers.FirstDirDifference(goldProj, workProj, ".prg", ".pts", ".io", ".dat");
                Assert.True(diff == null, diff);
            }
            finally { TestHelpers.DeleteDirSafely(goldDir); }
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates_InPts()
        {
            TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true, 1));
            string proj = TestHelpers.ResolveProjectDir("Epson Hidria", _outDir);
            string ptsPath = Directory.GetFiles(proj, "*.pts").First();

            // "Nauči" točko pStation1 (zamenjaj njene ničelne koordinate s prepoznavnimi).
            string content = File.ReadAllText(ptsPath);
            string taught = Regex.Replace(content,
                "(sLabel=\"pStation1\"[\\s\\S]*?rX=)0([\\s\\S]*?rY=)0([\\s\\S]*?rZ=)0",
                "${1}123.4${2}56.7${3}89.0");
            Assert.NotEqual(content, taught); // regex se je moral ujeti
            File.WriteAllText(ptsPath, taught);

            var vm = new ShellViewModel();
            vm.ImportProject(proj);
            vm.StationName = "Station2"; vm.StationFreeEnabled = false; vm.AddStation();
            vm.UpdateProject(proj);

            string after = File.ReadAllText(Directory.GetFiles(proj, "*.pts").First());
            Assert.Contains("rX=123.4", after);
            Assert.Contains("rY=56.7", after);
            Assert.Contains("rZ=89.0", after);
            Assert.Contains("sLabel=\"pStation2\"", after); // nova postaja dodana
        }
    }
}
