using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Screen;

namespace ExportOneHmiScreen
{
    class Program
    {
        // === TUKAJ PRILAGODI ===
        static readonly string ScreenName = "Etaloniranje_Measure_B08_NOK_1"; // ime zaslona, ki ga exportaš
        static readonly string OutputFolder = @"C:\TIA_ColorExport";           // kam naj shrani XML
        static readonly string HmiTargetName = "HMI_RT_1";                     // ime pravega HMI targeta (ne HMI_2!)

        static void Main(string[] args)
        {
            try
            {
                RunExport();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("=== PRIŠLO JE DO NAPAKE ===");
                Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("--- Inner exception ---");
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Pritisni Enter za izhod...");
                Console.ReadLine();
            }
        }

        static void RunExport()
        {
            // 1. Priklop na ŽE ODPRT TIA Portal (mora biti odprt s tvojim projektom)
            var processes = TiaPortal.GetProcesses();
            Console.WriteLine($"Najdenih odprtih TIA Portal procesov: {processes.Count}");
            if (processes.Count == 0)
            {
                Console.WriteLine("Ni najdenega odprtega TIA Portal procesa. Odpri Project1_V20 in poženi znova.");
                return;
            }

            TiaPortal tiaPortal = processes[0].Attach();
            Console.WriteLine($"Odprtih projektov v tem TIA Portal procesu: {tiaPortal.Projects.Count}");
            if (tiaPortal.Projects.Count == 0)
            {
                Console.WriteLine("V priklopljenem TIA Portalu ni odprtega projekta.");
                return;
            }

            Project project = tiaPortal.Projects[0];
            Console.WriteLine($"Povezan na projekt: {project.Name}");

            // 2. Poišči vse HMI targete v projektu (deluje tudi, če je HMI gnezden v PC-System)
            var hmiTargets = FindAllHmiTargets(project).ToList();
            Console.WriteLine($"Najdenih HMI targetov: {hmiTargets.Count}");
            foreach (var t in hmiTargets)
                Console.WriteLine($"  - {t.Name}");

            if (hmiTargets.Count == 0)
            {
                Console.WriteLine("V projektu nisem našel nobenega HMI targeta.");
                return;
            }

            // Izbere target po IMENU (ne prvega v seznamu - projekt ima več HMI naprav!)
            HmiTarget hmiTarget = hmiTargets.FirstOrDefault(t => t.Name == HmiTargetName);
            if (hmiTarget == null)
            {
                Console.WriteLine($"HMI target z imenom '{HmiTargetName}' ni bil najden med zgornjimi. Preveri ime in popravi HmiTargetName v kodi.");
                return;
            }
            Console.WriteLine($"Uporabljam HMI target: {hmiTarget.Name}");

            // 3. Poišči zaslon po imenu (rekurzivno po vseh podmapah)
            Screen screen = FindScreenInFolder(hmiTarget.ScreenFolder, ScreenName);
            if (screen == null)
            {
                Console.WriteLine($"Zaslon '{ScreenName}' ni bil najden na targetu '{hmiTarget.Name}'.");
                Console.WriteLine();
                Console.WriteLine("=== SEZNAM VSEH ZASLONOV, KI JIH SKRIPTA VIDI ===");
                PrintAllScreens(hmiTarget.ScreenFolder, "Screens");
                Console.WriteLine("=== KONEC SEZNAMA ===");
                Console.WriteLine();
                Console.WriteLine("Primerjaj zgornje ime s tistim, kar iščeš, in popravi ScreenName v kodi (natančno kopiraj, brez presledkov).");
                return;
            }

            Console.WriteLine($"Zaslon najden: {screen.Name}");

            // 4. Export
            Directory.CreateDirectory(OutputFolder);
            string outFile = Path.Combine(OutputFolder, ScreenName + ".xml");
            Console.WriteLine($"Exportam v: {outFile}");
            screen.Export(new FileInfo(outFile), ExportOptions.WithDefaults);

            Console.WriteLine($"Export uspešen: {outFile}");
            Console.WriteLine("Odpri XML in poišči 'Background color' vrednosti (npr. 59,101,151 ali #3B6597),");
            Console.WriteLine("da vidiva, v kateri obliki so barve dejansko zapisane.");
        }

        // --- Rekurzivno iskanje vseh HMI targetov v projektu (tudi znotraj PC-System naprav) ---

        static IEnumerable<HmiTarget> FindAllHmiTargets(Project project)
        {
            foreach (var device in GetAllDevices(project))
            {
                var target = GetHmiTargetFromDevice(device);
                if (target != null)
                    yield return target;
            }
        }

        static IEnumerable<Device> GetAllDevices(Project project)
        {
            foreach (Device d in project.Devices)
                yield return d;

            foreach (DeviceUserGroup grp in project.DeviceGroups)
                foreach (var d in GetAllDevicesInGroup(grp))
                    yield return d;
        }

        static IEnumerable<Device> GetAllDevicesInGroup(DeviceUserGroup grp)
        {
            foreach (Device d in grp.Devices)
                yield return d;

            foreach (DeviceUserGroup sub in grp.Groups)
                foreach (var d in GetAllDevicesInGroup(sub))
                    yield return d;
        }

        static HmiTarget GetHmiTargetFromDevice(Device device)
        {
            foreach (var item in AllDeviceItemsRecursive(device.DeviceItems))
            {
                var container = item.GetService<SoftwareContainer>();
                if (container?.Software is HmiTarget target)
                    return target;
            }
            return null;
        }

        static IEnumerable<DeviceItem> AllDeviceItemsRecursive(DeviceItemComposition items)
        {
            foreach (DeviceItem item in items)
            {
                yield return item;
                foreach (var sub in AllDeviceItemsRecursive(item.DeviceItems))
                    yield return sub;
            }
        }

        // --- Izpis vseh zaslonov s polno potjo, za diagnostiko ---

        static void PrintAllScreens(ScreenSystemFolder rootFolder, string path)
        {
            foreach (Screen s in rootFolder.Screens)
                Console.WriteLine($"{path}/{s.Name}");

            foreach (ScreenUserFolder sub in rootFolder.Folders)
                PrintAllScreensInSubfolder(sub, $"{path}/{sub.Name}");
        }

        static void PrintAllScreensInSubfolder(ScreenUserFolder folder, string path)
        {
            foreach (Screen s in folder.Screens)
                Console.WriteLine($"{path}/{s.Name}");

            foreach (ScreenUserFolder sub in folder.Folders)
                PrintAllScreensInSubfolder(sub, $"{path}/{sub.Name}");
        }

        // --- Rekurzivno iskanje enega zaslona po imenu po vseh podmapah ---

        static Screen FindScreenInFolder(ScreenSystemFolder rootFolder, string name)
        {
            foreach (Screen s in rootFolder.Screens)
                if (s.Name == name) return s;

            foreach (ScreenUserFolder sub in rootFolder.Folders)
            {
                var found = FindScreenInSubfolder(sub, name);
                if (found != null) return found;
            }
            return null;
        }

        static Screen FindScreenInSubfolder(ScreenUserFolder folder, string name)
        {
            foreach (Screen s in folder.Screens)
                if (s.Name == name) return s;

            foreach (ScreenUserFolder sub in folder.Folders)
            {
                var found = FindScreenInSubfolder(sub, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
