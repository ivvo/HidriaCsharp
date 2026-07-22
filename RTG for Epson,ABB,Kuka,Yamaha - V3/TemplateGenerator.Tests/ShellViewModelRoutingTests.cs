using TemplateGenerator.Core.ViewModels;
using Xunit;

namespace TemplateGenerator.Tests
{
    public class ShellViewModelRoutingTests : System.IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Theory]
        [InlineData("Epson Hidria")]
        [InlineData("KUKA Hella")]
        [InlineData("ABB Hidria")]
        [InlineData("Yamaha Hidria")]
        [InlineData("Kawasaki Hidria")]
        public void ImportProject_AutoDetectsCorrectVendor(string vendor)
        {
            TestHelpers.BuildAndGenerate(vendor, _outDir, ("Station1", true, 1));
            string proj = TestHelpers.ResolveProjectDir(vendor, _outDir);

            var vm2 = new ShellViewModel();
            vm2.ImportProject(proj);

            Assert.Contains(vendor, vm2.TextUpdate);
            Assert.Single(vm2.Program);
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
