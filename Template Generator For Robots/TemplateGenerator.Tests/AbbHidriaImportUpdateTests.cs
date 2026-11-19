using System;
using System.IO;
using System.Linq;
using TemplateGenerator.Core.Classes;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    public class AbbHidriaImportUpdateTests : IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();

        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        // Generira ABB projekt z možnostjo Positions != 1 (BuildAndGenerate zna le (ime, free)).
        private static ShellViewModel Gen(string outDir, params (string name, bool free, int positions)[] stations)
        {
            var vm = new ShellViewModel();
            vm.ProgramName = "Robot1";
            vm.AddProgram();
            vm.SelectedProgram = vm.Program[0];
            foreach (var (name, free, positions) in stations)
            {
                vm.StationName = name;
                vm.StationFreeEnabled = free;
                vm.AddStation();
                if (positions != 1)
                    vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = positions;
            }
            vm.SelectedTemplate = "ABB Hidria";
            vm.GenerateSimpleProgram = true;
            vm.GenerateProject(outDir);
            return vm;
        }

        [Fact]
        public void Import_RoundTrips_StationListAndFlags()
        {
            var vm = TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            var original = vm.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            var imported = vm2.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();

            Assert.Equal(original, imported);
        }

        [Fact]
        public void Update_AppendsNewStation_AndPreservesExistingLines()
        {
            TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true));
            string motionPath = Path.Combine(_outDir, "Robot1_Motion.mod");
            string before = File.ReadAllText(motionPath);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(motionPath);
            Assert.True(TestHelpers.IsSubsequenceOfLines(before, after));
            Assert.Contains("PROC robot_goStation2()", after);
        }

        [Fact]
        public void Update_NewStation_BecomesOriginInExistingGoFunctions()
        {
            TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string motionContent = File.ReadAllText(Path.Combine(_outDir, "Robot1_Motion.mod"));
            string goStation1 = TestHelpers.ExtractBetween(motionContent, "PROC robot_goStation1()", "ENDPROC");
            Assert.Contains("bFromStation2", goStation1);
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates()
        {
            TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true));
            string globalPath = Path.Combine(_outDir, "Global.mod");

            string globalContent = File.ReadAllText(globalPath);
            const string marker = "CONST robtarget jRobot1_Station1 :=[[0,0,0]";
            const string replacement = "CONST robtarget jRobot1_Station1 :=[[44.4,55.5,66.6]";
            Assert.Contains(marker, globalContent); // ce to pade, se je poimenovanje tock v generatorju spremenilo
            string taught = globalContent.Replace(marker, replacement);
            File.WriteAllText(globalPath, taught);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string globalAfter = File.ReadAllText(globalPath);
            Assert.Contains(replacement, globalAfter);
            Assert.Contains("CONST robtarget jRobot1_Station2 :=", globalAfter);
        }

        // Sprememba Positions obstoječe postaje (1 -> 2): rezultat MORA biti bajt-identičen svežemu
        // Generate z Weld na 2 poziciji (robtargeti so v obeh ničelni).
        [Fact]
        public void Update_ChangeExistingStationPositions_1to2_ByteIdenticalToFullGenerate()
        {
            string goldDir = TestHelpers.CreateTempDir();
            try
            {
                Gen(goldDir, ("Weld", true, 2));
                Gen(_outDir, ("Weld", true, 1));

                var vm = new ShellViewModel();
                vm.ImportProject(_outDir);
                vm.Program[0].Stations.Single(s => s.RobotStationName == "Weld").Positions = 2;
                vm.UpdateProject(_outDir);

                AssertFilesByteIdentical(goldDir, _outDir, "Global.mod", "Robot1_Motion.mod", "robot_Main.mod", "Communication.mod");
            }
            finally { TestHelpers.DeleteDirSafely(goldDir); }
        }

        // Sprememba Positions navzdol (3 -> 1) mora biti prav tako bajt-identična svežemu Generate.
        [Fact]
        public void Update_ChangeExistingStationPositions_3to1_ByteIdenticalToFullGenerate()
        {
            string goldDir = TestHelpers.CreateTempDir();
            try
            {
                Gen(goldDir, ("Weld", true, 1), ("Drop", false, 1));
                Gen(_outDir, ("Weld", true, 3), ("Drop", false, 1));

                var vm = new ShellViewModel();
                vm.ImportProject(_outDir);
                vm.Program[0].Stations.Single(s => s.RobotStationName == "Weld").Positions = 1;
                vm.UpdateProject(_outDir);

                AssertFilesByteIdentical(goldDir, _outDir, "Global.mod", "Robot1_Motion.mod", "robot_Main.mod", "Communication.mod");
            }
            finally { TestHelpers.DeleteDirSafely(goldDir); }
        }

        // Ob spremembi 1 -> 2 se naučena koordinata ene točke ohrani (preimenuje v pozicijo 1),
        // nova pozicija 2 dobi ničle.
        [Fact]
        public void Update_ChangePositions_PreservesAndRenamesTaughtRobtarget()
        {
            Gen(_outDir, ("Weld", true, 1));
            string globalPath = Path.Combine(_outDir, "Global.mod");

            string g = File.ReadAllText(globalPath);
            const string marker = "CONST robtarget jRobot1_Weld :=[[0,0,0]";
            const string taughtVal = "CONST robtarget jRobot1_Weld :=[[11.1,22.2,33.3]";
            Assert.Contains(marker, g);
            File.WriteAllText(globalPath, g.Replace(marker, taughtVal));

            var vm = new ShellViewModel();
            vm.ImportProject(_outDir);
            vm.Program[0].Stations.Single(s => s.RobotStationName == "Weld").Positions = 2;
            vm.UpdateProject(_outDir);

            string after = File.ReadAllText(globalPath);
            Assert.Contains("CONST robtarget jRobot1_Weld1 :=[[11.1,22.2,33.3]", after); // naučena -> pozicija 1
            Assert.Contains("CONST robtarget jRobot1_Weld2 :=[[0,0,0]", after);           // nova pozicija = ničle
            Assert.DoesNotContain("robtarget jRobot1_Weld :=", after);                    // stare enojne ni več
        }

        private static void AssertFilesByteIdentical(string goldDir, string workDir, params string[] files)
        {
            foreach (var file in files)
            {
                string gold = File.ReadAllText(Path.Combine(goldDir, file));
                string work = File.ReadAllText(Path.Combine(workDir, file));
                if (gold != work)
                    Assert.Fail($"'{file}': {FirstDiff(gold, work)}");
            }
        }

        private static string FirstDiff(string a, string b)
        {
            var la = a.Replace("\r", "").Split('\n');
            var lb = b.Replace("\r", "").Split('\n');
            int n = Math.Min(la.Length, lb.Length);
            for (int i = 0; i < n; i++)
                if (la[i] != lb[i])
                    return $"vrstica {i + 1}:\n  gold=[{la[i]}]\n  work=[{lb[i]}]";
            return $"enako do {n} vrstic; dolžini gold={la.Length} work={lb.Length}";
        }

        [Fact]
        public void Import_ThrowsClearException_WhenNoMotionModFound()
        {
            string emptyDir = TestHelpers.CreateTempDir();
            try
            {
                var ex = Assert.Throws<AbbHidriaImportException>(() => AbbHidriaProjectImporter.Import(emptyDir));
                Assert.Contains("_Motion.mod", ex.Message);
            }
            finally
            {
                TestHelpers.DeleteDirSafely(emptyDir);
            }
        }
    }
}
