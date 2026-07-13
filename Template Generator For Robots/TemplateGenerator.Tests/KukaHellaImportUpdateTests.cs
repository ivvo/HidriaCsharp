using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Classes;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    // KUKA Hella generator (SimpleProgram varianta) ne podpira Pallet/večpozicijskih postaj -
    // zato tu ni ustreznika Epsonovega "pallet + homing indeks" testa.
    public class KukaHellaImportUpdateTests : IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();

        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Fact]
        public void Import_RoundTrips_StationListAndFlags()
        {
            var vm = TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            var original = vm.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();
            var imported = vm2.Program[0].Stations.Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();

            Assert.Equal(original, imported);
        }

        [Fact]
        public void Update_AppendsNewStation_AndPreservesExistingLines()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true));
            string motionSrcPath = Path.Combine(_outDir, "R1", "Program", "robot1_motion.src");
            string before = File.ReadAllText(motionSrcPath);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(motionSrcPath);
            Assert.True(TestHelpers.IsSubsequenceOfLines(before, after));
            Assert.Contains("GLOBAL DEF robot_goStation2 ()", after);
        }

        [Fact]
        public void Update_NewStation_BecomesOriginInExistingGoFunctions()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string motionSrc = File.ReadAllText(Path.Combine(_outDir, "R1", "Program", "robot1_motion.src"));
            string goStation1 = TestHelpers.ExtractBetween(motionSrc, "GLOBAL DEF robot_goStation1 ()", "\r\nEND");
            Assert.Contains("CASE #FROM_STATION2", goStation1);
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true));
            string motionDatPath = Path.Combine(_outDir, "R1", "Program", "robot1_motion.dat");

            string datContent = File.ReadAllText(motionDatPath);
            string taught = Regex.Replace(datContent,
                @"(DECL GLOBAL E6POS jStation1=\{X )0(,Y )0\.0(,Z )0",
                "${1}111.1${2}222.2${3}333.3");
            Assert.NotEqual(datContent, taught);
            File.WriteAllText(motionDatPath, taught);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string datAfter = File.ReadAllText(motionDatPath);
            Assert.Contains("111.1", datAfter);
            Assert.Contains("222.2", datAfter);
            Assert.Contains("333.3", datAfter);
            Assert.Contains("DECL GLOBAL E6POS jStation2=", datAfter);
        }

        [Fact]
        public void Update_ExtendsConfigDatEnumLists()
        {
            TestHelpers.BuildAndGenerate("KUKA Hella", _outDir, ("Station1", true));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Station2";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string configDat = File.ReadAllText(Path.Combine(_outDir, "R1", "System", "$config.dat"));
            Assert.Contains("FROM_STATION2", configDat);
            Assert.Contains("TO_STATION2", configDat);
        }

        [Fact]
        public void Import_ThrowsClearException_WhenNoMotionFilesFound()
        {
            string emptyDir = TestHelpers.CreateTempDir();
            try
            {
                var ex = Assert.Throws<KukaHellaImportException>(() => KukaHellaProjectImporter.Import(emptyDir));
                Assert.Contains("R1", ex.Message);
            }
            finally
            {
                TestHelpers.DeleteDirSafely(emptyDir);
            }
        }
    }
}
