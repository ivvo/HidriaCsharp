using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    public class AbbHidriaImportUpdateTests : System.IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Fact]
        public void Import_RoundTrips_StationListAndFlags()
        {
            var vm = TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true, 1), ("Station2", false, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            var original = vm.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            var imported = vm2.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            Assert.Equal(original, imported);
        }

        [Fact]
        public void Update_AppendsNewStation_AndPreservesExistingLines()
        {
            TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true, 1), ("Station2", false, 1));
            string motion = Path.Combine(_outDir, "robot_Motion.mod");
            string before = File.ReadAllText(motion);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.StationName = "Station3"; vm2.StationFreeEnabled = true; vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(motion);
            Assert.True(TestHelpers.IsSubsequenceOfLines(before, after), "Obstoječe vrstice v Motion.mod se ne smejo izgubiti.");
            Assert.Contains("PROC robot_goStation3()", after);
        }

        [Fact]
        public void Update_NewStation_BecomesOriginInExistingGoProcs()
        {
            TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.StationName = "Station2"; vm2.StationFreeEnabled = true; vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string motion = File.ReadAllText(Path.Combine(_outDir, "robot_Motion.mod"));
            string goStation1 = TestHelpers.ExtractBetween(motion, "PROC robot_goStation1()", "ENDPROC");
            Assert.Contains("bFromStation2", goStation1);
        }

        [Fact]
        public void Update_PreservesTaughtRobtargets_InGlobalMod()
        {
            TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true, 1));
            string global = Path.Combine(_outDir, "Global.mod");
            string content = File.ReadAllText(global);

            // "Nauči" prvo robtarget točko (zamenjaj ničelno translacijo s prepoznavno).
            string taught = new Regex(@":=\[\[0,0,0\]").Replace(content, ":=[[111,222,333]", 1);
            Assert.NotEqual(content, taught);
            File.WriteAllText(global, taught);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.StationName = "Station2"; vm2.StationFreeEnabled = false; vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(global);
            Assert.Contains("[[111,222,333]", after);
        }
    }
}
