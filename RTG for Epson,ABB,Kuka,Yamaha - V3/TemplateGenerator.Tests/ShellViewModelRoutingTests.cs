using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    public class ShellViewModelRoutingTests : System.IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Theory]
        [InlineData("KUKA Hella")]
        [InlineData("ABB Hidria")]
        [InlineData("Yamaha Hidria")]
        public void ImportProject_AutoDetectsCorrectVendor(string vendor)
        {
            TestHelpers.BuildAndGenerate(vendor, _outDir, ("Station1", true, 1));
            string proj = TestHelpers.ResolveProjectDir(vendor, _outDir);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(proj);

            Assert.Contains(vendor, vm2.TextUpdate);
            Assert.Single(vm2.Program);
        }

        // Epson: uvoz (branje) deluje, posodobitev pa je za V3 namenoma zaklenjena (drugačen generator).
        [Fact]
        public void Epson_Import_Works_But_Update_IsGated()
        {
            TestHelpers.BuildAndGenerate("Epson Hidria", _outDir, ("Station1", true, 1));
            string proj = TestHelpers.ResolveProjectDir("Epson Hidria", _outDir);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(proj);
            Assert.Contains("Epson Hidria", vm2.TextUpdate);

            vm2.StationName = "Station2"; vm2.StationFreeEnabled = true; vm2.AddStation();
            vm2.UpdateProject(proj);
            Assert.Contains("ni prilagojena", vm2.TextUpdate);
        }

        [Fact]
        public void ImportProject_ReportsClearError_ForUnrecognizedFolder()
        {
            var vm = new ShellViewModel();
            vm.ImportProject(_outDir); // prazna mapa
            Assert.Contains("ni bilo mogoče zaznati", vm.TextUpdate);
        }
    }
}
