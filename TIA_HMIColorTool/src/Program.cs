using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TIA_HMIColorTool
{
    static class Program
    {
        // Mapa z Openness DLL-ji (TIA Portal V20)
        internal const string OpennessDir = @"C:\Program Files\Siemens\Automation\Portal V20\PublicAPI\V20";

        [STAThread]
        static void Main()
        {
            // Resolve handler MORA biti registriran, preden se dotaknemo katerega koli Siemens tipa.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveOpenness;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            RunApp();
        }

        // Ločena metoda (NoInlining), da JIT ne naloži Siemens.Engineering pred registracijo resolverja.
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void RunApp()
        {
            Application.Run(new MainForm());
        }

        static Assembly ResolveOpenness(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            if (!name.StartsWith("Siemens.Engineering", StringComparison.OrdinalIgnoreCase))
                return null;
            string path = Path.Combine(OpennessDir, name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        }
    }
}
