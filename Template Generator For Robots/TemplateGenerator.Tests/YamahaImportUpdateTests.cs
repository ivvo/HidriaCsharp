using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Classes;
using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    public class YamahaImportUpdateTests : IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();

        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        // Generira Yamaha projekt (en robot "Robot1"). Podpira tudi Positions != 1 (BuildAndGenerate
        // v TestHelpers zna samo (ime, free), zato ima ta razred svojo različico z Positions).
        private static ShellViewModel Gen(string outDir, params (string name, bool free, int positions)[] stations)
        {
            var vm = new ShellViewModel();
            vm.ProgramName = "Robot1";
            vm.AddProgram(); // doda "Home" (free=false, positions=1)
            vm.SelectedProgram = vm.Program[0];

            foreach (var (name, free, positions) in stations)
            {
                vm.StationName = name;
                vm.StationFreeEnabled = free;
                vm.AddStation();
                if (positions != 1)
                    vm.Program[0].Stations[vm.Program[0].Stations.Count - 1].Positions = positions;
            }

            vm.SelectedTemplate = "Yamaha";
            vm.GenerateSimpleProgram = true;
            vm.GenerateProject(outDir);
            return vm;
        }

        [Fact]
        public void Import_RoundTrips_StationListAndFlags()
        {
            Gen(_outDir, ("PartTake", true, 1), ("PartRls", false, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            var imported = vm2.Program[0].Stations
                .Select(s => (s.RobotStationName, s.StationFreeEnabled)).ToList();

            Assert.Equal(
                new[] { ("Home", false), ("PartTake", true), ("PartRls", false) }.ToList(),
                imported);
        }

        [Fact]
        public void Import_RecoversPositions_ForMultiPositionStation()
        {
            Gen(_outDir, ("PartTake", true, 1), ("Evacuate", true, 3));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            var evacuate = vm2.Program[0].Stations.Single(s => s.RobotStationName == "Evacuate");
            Assert.Equal(3, evacuate.Positions);
            Assert.Equal(1, vm2.Program[0].Stations.Single(s => s.RobotStationName == "PartTake").Positions);
        }

        [Fact]
        public void Import_AutoDetectedAsYamaha_AndSetsTemplate()
        {
            Gen(_outDir, ("PartTake", true, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);

            Assert.Equal("Yamaha", vm2.SelectedTemplate);
        }

        // Najmočnejši test: posodobljen projekt MORA biti bajt-za-bajt enak sveže generiranemu projektu
        // s celotnim (končnim) seznamom postaj - naučene koordinate so v obeh primerih ničelni
        // ograditelji, zato je vsaka razlika napaka v logiki posodabljanja (vrstni red odsekov,
        // origin-completeness, homing meja, razporeditev točk, IO signali ...).
        [Fact]
        public void Update_AppendFreeStation_ByteIdenticalToFullGenerate()
        {
            AssertUpdateEqualsFullGenerate(
                original: new[] { ("PartTake", true, 1), ("PartRls", true, 1) },
                added: new[] { ("Evacuate", true, 1) });
        }

        [Fact]
        public void Update_AppendMultiPositionStation_ByteIdenticalToFullGenerate()
        {
            AssertUpdateEqualsFullGenerate(
                original: new[] { ("PartTake", true, 1), ("PartRls", true, 1) },
                added: new[] { ("Evacuate", true, 2) });
        }

        // Prenumeriranje obstoječih dodatnih (večpozicijskih) točk navzgor, da naredimo prostor za
        // primarno točko nove postaje - Yamaha ekvivalent Epsonovega PickPlace buga.
        [Fact]
        public void Update_AppendAfterExistingMultiPositionStation_ByteIdenticalToFullGenerate()
        {
            AssertUpdateEqualsFullGenerate(
                original: new[] { ("PartTake", false, 2) },
                added: new[] { ("PartRls", true, 1) });
        }

        [Fact]
        public void Update_AppendMultipleStationsAtOnce_ByteIdenticalToFullGenerate()
        {
            AssertUpdateEqualsFullGenerate(
                original: new[] { ("PartTake", true, 1) },
                added: new[] { ("PartRls", true, 1), ("Evacuate", true, 2) });
        }

        [Fact]
        public void Update_AppendFirstFreeStation_ByteIdenticalToFullGenerate()
        {
            // Prvotni projekt nima nobene "free" postaje (Home ni free) - preveri prazen seznam v
            // stationsBusy/stationsFree in prvi SONM4 vnos.
            AssertUpdateEqualsFullGenerate(
                original: new (string, bool, int)[] { },
                added: new[] { ("PartTake", true, 1) });
        }

        [Fact]
        public void Update_NewStation_BecomesOriginInExistingGoFunctions()
        {
            Gen(_outDir, ("PartTake", true, 1));

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Evacuate";
            vm2.StationFreeEnabled = true;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string content = File.ReadAllText(Path.Combine(_outDir, "Robot1.all"));
            string goHome = TestHelpers.ExtractBetween(content, "*robot_goHome:", "RETURN");
            string goPartTake = TestHelpers.ExtractBetween(content, "*robot_goPartTake:", "RETURN");

            Assert.Contains("PositionFrom = Evacuate", goHome);
            Assert.Contains("PositionFrom = Evacuate", goPartTake);
        }

        [Fact]
        public void Update_PreservesTaughtCoordinates()
        {
            Gen(_outDir, ("PartTake", true, 1));
            string allPath = Path.Combine(_outDir, "Robot1.all");

            // "Nauči" točko P0 (Home) - zamenja ničelne koordinate z nečim prepoznavnim.
            string content = File.ReadAllText(allPath);
            string taught = Regex.Replace(content, @"P0=[^\r\n]+", "P0=111.111 222.222 333.333 0.000 0.000 0.000 1 0 0");
            Assert.NotEqual(content, taught); // regex se je moral ujeti
            File.WriteAllText(allPath, taught);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Evacuate";
            vm2.StationFreeEnabled = false;
            vm2.AddStation();
            vm2.UpdateProject(_outDir);

            string after = File.ReadAllText(allPath);
            Assert.Contains("111.111 222.222 333.333", after);
        }

        [Fact]
        public void Update_BumpsHomingLoopBound_ForMultiPositionStation()
        {
            Gen(_outDir, ("PartTake", true, 1)); // 2 postaji (Home, PartTake) -> FOR i = 0 TO 1

            var vm2 = new ShellViewModel();
            vm2.ImportProject(_outDir);
            vm2.SelectedProgram = vm2.Program[0];
            vm2.StationName = "Evacuate";
            vm2.StationFreeEnabled = true;
            vm2.AddStation();
            vm2.Program[0].Stations[2].Positions = 2; // Evacuate: +1 primarna +1 dodatna = 2 novi točki
            vm2.UpdateProject(_outDir);

            string content = File.ReadAllText(Path.Combine(_outDir, "Robot1.all"));
            // Skupno točk: Home(0), PartTake(1), Evacuate1(2), Evacuate2(3) => FOR i = 0 TO 3
            Assert.Contains("FOR i = 0 TO 3", content);
        }

        [Fact]
        public void Import_ThrowsClearException_WhenNoAllFile()
        {
            string emptyDir = TestHelpers.CreateTempDir();
            try
            {
                var ex = Assert.Throws<YamahaImportException>(() => YamahaProjectImporter.Import(emptyDir));
                Assert.Contains("ni najdena", ex.Message);
            }
            finally
            {
                TestHelpers.DeleteDirSafely(emptyDir);
            }
        }

        // -- pomožna metoda za bajt-primerjavo --------------------------------------------------

        private void AssertUpdateEqualsFullGenerate(
            (string name, bool free, int positions)[] original,
            (string name, bool free, int positions)[] added)
        {
            // 1) Sveže generiran projekt s celotnim (končnim) seznamom = zlati vzorec.
            string goldDir = TestHelpers.CreateTempDir();
            try
            {
                Gen(goldDir, original.Concat(added).ToArray());
                string gold = File.ReadAllText(Path.Combine(goldDir, "Robot1.all"));

                // 2) Generiraj samo prvotni seznam, uvozi, dodaj nove postaje, posodobi.
                Gen(_outDir, original);

                var vm2 = new ShellViewModel();
                vm2.ImportProject(_outDir);
                vm2.SelectedProgram = vm2.Program[0];
                foreach (var (name, free, positions) in added)
                {
                    vm2.StationName = name;
                    vm2.StationFreeEnabled = free;
                    vm2.AddStation();
                    if (positions != 1)
                        vm2.Program[0].Stations[vm2.Program[0].Stations.Count - 1].Positions = positions;
                }
                vm2.UpdateProject(_outDir);

                string updated = File.ReadAllText(Path.Combine(_outDir, "Robot1.all"));

                if (gold != updated)
                    Assert.Fail(FirstDifference(gold, updated));
            }
            finally
            {
                TestHelpers.DeleteDirSafely(goldDir);
            }
        }

        // Vrne berljiv opis prve razlike po vrsticah (za jasno sporočilo ob padcu testa).
        private static string FirstDifference(string expected, string actual)
        {
            var e = expected.Split('\n');
            var a = actual.Split('\n');
            int n = Math.Min(e.Length, a.Length);
            for (int i = 0; i < n; i++)
            {
                if (e[i] != a[i])
                    return $"Prva razlika v vrstici {i + 1}:\n  pricakovano: [{e[i]}]\n  dejansko:    [{a[i]}]";
            }
            return $"Datoteki se ujemata do vrstice {n}, a se razlikujeta v dolzini (pricakovano {e.Length}, dejansko {a.Length} vrstic).";
        }
    }
}
