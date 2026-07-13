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
