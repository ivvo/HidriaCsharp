using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TIA_HMIColorTool
{
    static class Program
    {
        // Verzija TIA Portala, za katero je ta exe zgrajen (glej TiaVersion v .csproj).
        // Openness se priklopi samo na odprt TIA Portal iste verzije.
#if TIA_V18
        internal const string TiaVersion = "V18";
#elif TIA_V19
        internal const string TiaVersion = "V19";
#else
        internal const string TiaVersion = "V20";
#endif
        // Mapa z Openness DLL-ji te verzije
        internal const string OpennessDir = @"C:\Program Files\Siemens\Automation\Portal " + TiaVersion + @"\PublicAPI\" + TiaVersion;

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

        /// <summary>
        /// Verzije vseh trenutno odprtih TIA Portal procesov (npr. "V18"), ne glede na to, za katero verzijo je ta exe.
        /// Ne uporablja Openness - samo pot do Siemens.Automation.Portal.exe (…\Portal V18\Bin\…).
        /// </summary>
        internal static List<string> RunningTiaVersions()
        {
            var versions = new List<string>();
            foreach (var p in System.Diagnostics.Process.GetProcessesByName("Siemens.Automation.Portal"))
            {
                try
                {
                    var m = Regex.Match(p.MainModule.FileName, @"\\Portal (V\d+(?:\.\d+)?)\\", RegexOptions.IgnoreCase);
                    if (m.Success && !versions.Contains(m.Groups[1].Value.ToUpperInvariant()))
                        versions.Add(m.Groups[1].Value.ToUpperInvariant());
                }
                catch { /* brez dostopa do procesa - preskoči */ }
                finally { p.Dispose(); }
            }
            return versions;
        }

        /// <summary>Pot do exe-ja tega orodja za podano verzijo TIA (V20 v osnovni mapi, ostale v podmapi &lt;verzija&gt;).</summary>
        internal static string ExeForVersion(string version)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string own = TiaVersion;   // spremenljivka namesto konstante - sicer prevajalnik javi "unreachable code"
            if (own != "V20") baseDir =Path.GetFullPath(Path.Combine(baseDir, ".."));   // iz …\Debug\V18\ nazaj v …\Debug\
            return version == "V20"
                ? Path.Combine(baseDir, "TIA_HMIColorTool.exe")
                : Path.Combine(baseDir, version, "TIA_HMIColorTool_" + version + ".exe");
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
