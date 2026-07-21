using System.IO;
using System.Linq;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    public class KukaHellaImportUpdateTests : System.IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Fact]
        public void Import_RoundTrips_StationListAndFlags()
        {
            var vm = TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true, 1), ("Station2", false, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            var original = vm.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            var imported = vm2.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            Assert.Equal(original, imported);
        }

        [Fact]
        public void Update_AppendsNewStation_AndPreservesExistingLines()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true, 1), ("Station2", false, 1));
            string motionSrc = Path.Combine(_outDir, "R1", "Program", "robot_motion.src");
            string before = File.ReadAllText(motionSrc);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.StationName = "Station3"; vm2.StationFreeEnabled = true; vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(motionSrc);
            Assert.True(TestHelpers.IsSubsequenceOfLines(before, after), "Obstoječe vrstice v motion.src se ne smejo izgubiti.");
            Assert.Contains("GLOBAL DEF robot_goStation3 ()", after);
        }

        [Fact]
        public void Update_NewStation_BecomesOriginInExistingGoFunctions()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.StationName = "Station2"; vm2.StationFreeEnabled = true; vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string motionSrc = File.ReadAllText(Path.Combine(_outDir, "R1", "Program", "robot_motion.src"));
            string goStation1 = TestHelpers.ExtractBetween(motionSrc, "GLOBAL DEF robot_goStation1 ()", "END");
            Assert.Contains("#FROM_STATION2", goStation1);
        }

        // Regresija: $config.dat je prej lepil dva SIGNAL naslova v eno vrstico
        // ("...$OUT[33]SIGNAL doSTATION2_FREE $OUT[34]"). Vsak SIGNAL mora biti v svoji vrstici.
        [Fact]
        public void Update_ConfigDat_SignalsAreOnSeparateLines()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.StationName = "Station2"; vm2.StationFreeEnabled = true; vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string config = File.ReadAllText(Path.Combine(_outDir, "R1", "System", "$config.dat"));
            Assert.DoesNotContain("$OUT[33]SIGNAL", config);
            Assert.Contains("SIGNAL doSTATION1_FREE $OUT[33]", config);
            Assert.Contains("SIGNAL doSTATION2_FREE $OUT[34]", config);
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates_InMotionDat()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true, 1));
            string motionDat = Path.Combine(_outDir, "R1", "Program", "robot_motion.dat");
            string content = File.ReadAllText(motionDat);
            string taught = content.Replace("jStation1={X 0,Y 0.0,Z 0", "jStation1={X 123.45,Y 67.8,Z 9");
            Assert.NotEqual(content, taught);
            File.WriteAllText(motionDat, taught);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.StationName = "Station2"; vm2.StationFreeEnabled = false; vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(motionDat);
            Assert.Contains("X 123.45,Y 67.8,Z 9", after);
        }
    }
}
