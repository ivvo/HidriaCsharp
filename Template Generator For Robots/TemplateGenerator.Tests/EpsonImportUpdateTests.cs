using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Classes;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    public class EpsonImportUpdateTests : IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();

        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Fact]
        public void Import_RoundTrips_StationListAndFlags()
        {
            var vm = TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true), ("Station2", false));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            var original = vm.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            var imported = vm2.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();

            Assert.Equal(original, imported);
        }

        [Fact]
        public void Update_AppendsNewStation_AndPreservesExistingLines()
        {
            TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true), ("Station2", false));
            string prgPath = Path.Combine(_outDir, "robot1.prg");
            string before = File.ReadAllText(prgPath);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station3";
            vm2.StationFreeEnabled = true;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(prgPath);
            // "For index = 0 To N" v Homing se namerno poveča ob vsaki dodani postaji - edina
            // dovoljena sprememba obstoječe vrstice, vse ostalo mora ostati nedotaknjeno.
            TestHelpers.AssertOnlyExpectedLineChanges(before, after, @"^\s*For index = 0 To \d+");
            Assert.Contains("Function robot1_goStation3()", after);
        }

        [Fact]
        public void Update_NewStation_BecomesOriginInExistingGoFunctions()
        {
            TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true), ("Station2", false));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station3";
            vm2.StationFreeEnabled = true;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string prgContent = File.ReadAllText(Path.Combine(_outDir, "robot1.prg"));
            string goStation1 = TestHelpers.ExtractBetween(prgContent, "Function robot1_goStation1()", "Fend");

            Assert.Contains("ROBOT1_FROM_STATION3", goStation1);
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates()
        {
            TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true));
            string ptsPath = Path.Combine(_outDir, "Robot11.pts");

            string ptsContent = File.ReadAllText(ptsPath);
            string taught = Regex.Replace(ptsContent,
                "(sLabel=\"pHome\"[\\s\\S]*?rX=)0([\\s\\S]*?rY=)0([\\s\\S]*?rZ=)0",
                "${1}10.0${2}20.0${3}30.0");
            Assert.NotEqual(ptsContent, taught); // regex se je moral ujeti, sicer je test brez pomena
            File.WriteAllText(ptsPath, taught);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string ptsAfter = File.ReadAllText(ptsPath);
            Assert.Contains("10.0", ptsAfter);
            Assert.Contains("20.0", ptsAfter);
            Assert.Contains("30.0", ptsAfter);
        }

        // Regresija za bug, ki ga je uporabnik prijavil na resničnem PickPlace projektu:
        // pallet postaja (dodatne P2/P3/P4 točke) je premikala novo primarno točko na napačno mesto
        // in Homing zanka se ni povečala.
        [Fact]
        public void Update_WithPalletStation_InsertsNewPrimaryPointAtCorrectIndex_AndBumpsHomingLoopBound()
        {
            var vm = new ShellViewModel();
            vm.ProgramName = "Robot1";
            vm.AddProgram();
            vm.SelectedProgram = vm.Program[0];
            vm.StationName = "Station1";
            vm.StationFreeEnabled = true;
            vm.AddStation();
            vm.Program[0].Stations[1].Pallet = true; // Station1 postane pallet postaja (P1,P2,P3,P4)
            vm.SelectedTemplate = "Epson Hidria";
            vm.GenerateSimpleProgram = true;
            vm.GenerateProject(_outDir);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            // previousStationCount = 2 (Home, Station1) -> Station2 primarna točka na indeksu 2,
            // Station1 pallet P2/P3/P4 se prenumerirajo z 2,3,4 na 3,4,5.
            string ptsAfter = File.ReadAllText(Path.Combine(_outDir, "Robot11.pts"));
            Assert.Matches(new Regex("Point3 \\{\r?\n\tnNumber=2\r?\n\tsLabel=\"pStation2\""), ptsAfter);
            Assert.Matches(new Regex("Point6 \\{\r?\n\tnNumber=5\r?\n\tsLabel=\"pStation1P4\""), ptsAfter);

            string prgContent = File.ReadAllText(Path.Combine(_outDir, "robot1.prg"));
            string homing = TestHelpers.ExtractBetween(prgContent, "Function robot1_homing", "Fend");
            Assert.Contains("For index = 0 To 2", homing);
        }

        [Fact]
        public void Import_ThrowsClearException_WhenFolderHasNoStations()
        {
            string emptyDir = TestHelpers.CreateTempDir();
            try
            {
                File.WriteAllText(Path.Combine(emptyDir, "Main.prg"), "Function main\nFend");
                var ex = Assert.Throws<EpsonImportException>(() => EpsonProjectImporter.Import(emptyDir));
                Assert.Contains("ni najdena", ex.Message);
            }
            finally
            {
                TestHelpers.DeleteDirSafely(emptyDir);
            }
        }
    }
}
