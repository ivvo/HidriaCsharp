using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Screen;

namespace ImportOneHmiScreen
{
    class Program
    {
        // === TUKAJ PRILAGODI ===
        static readonly string ScreenName = "Etaloniranje_Measure_B08_NOK_1";       // ime zaslona, ki ga importaš nazaj
        static readonly string ImportFilePath = @"C:\TIA_ColorExport\Etaloniranje_Measure_B08_NOK_1.xml"; // predelan XML
        static readonly string HmiTargetName = "HMI_RT_1";                          // ime pravega HMI targeta

        static void Main(string[] args)
        {
            try
            {
                RunImport();
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

        static void RunImport()
        {
            if (!File.Exists(ImportFilePath))
            {
                Console.WriteLine($"Datoteke ni: {ImportFilePath}");
                return;
            }

            var processes = TiaPortal.GetProcesses();
            if (processes.Count == 0)
            {
                Console.WriteLine("Ni najdenega odprtega TIA Portal procesa. Odpri Project1_V20 in poženi znova.");
                return;
            }

            TiaPortal tiaPortal = processes[0].Attach();
            Project project = tiaPortal.Projects[0];
            Console.WriteLine($"Povezan na projekt: {project.Name}");

            var hmiTargets = FindAllHmiTargets(project).ToList();
            HmiTarget hmiTarget = hmiTargets.FirstOrDefault(t => t.Name == HmiTargetName);
            if (hmiTarget == null)
            {
                Console.WriteLine($"HMI target '{HmiTargetName}' ni bil najden. Najdeni targeti:");
                foreach (var t in hmiTargets) Console.WriteLine("  - " + t.Name);
                return;
            }
            Console.WriteLine($"Uporabljam HMI target: {hmiTarget.Name}");

            // Poišči ScreenComposition (mapo), kjer zaslon TRENUTNO živi, da uvozimo na pravo mesto
            ScreenComposition targetCollection = FindScreensCollectionContaining(hmiTarget.ScreenFolder, ScreenName);
            if (targetCollection == null)
            {
                Console.WriteLine($"Zaslona '{ScreenName}' v projektu ne najdem (za določitev prave mape). Prekinjam - varneje je, da obstoječi zaslon najprej najdeš z export orodjem.");
                return;
            }

            Console.WriteLine($"Importam '{ImportFilePath}' ...");

            // POZOR: preveri v IntelliSense pravo vrednost ImportOptions (verjetno .Override)
            targetCollection.Import(new FileInfo(ImportFilePath), ImportOptions.Override);

            Console.WriteLine("Import končan. Preveri zaslon v TIA Portalu (barve naj bi se posodobile).");
            Console.WriteLine("Projekt še NI shranjen - shrani ročno (Ctrl+S) v TIA Portalu, ko preveriš rezultat.");
        }

        // --- Iskanje HMI targetov (enako kot v export orodju) ---

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

        // --- Poišče ScreenComposition (mapo), v kateri zaslon s tem imenom trenutno obstaja ---

        static ScreenComposition FindScreensCollectionContaining(ScreenSystemFolder root, string name)
        {
            foreach (Screen s in root.Screens)
                if (s.Name == name) return root.Screens;

            foreach (ScreenUserFolder sub in root.Folders)
            {
                var found = FindScreensCollectionContainingSub(sub, name);
                if (found != null) return found;
            }
            return null;
        }

        static ScreenComposition FindScreensCollectionContainingSub(ScreenUserFolder folder, string name)
        {
            foreach (Screen s in folder.Screens)
                if (s.Name == name) return folder.Screens;

            foreach (ScreenUserFolder sub in folder.Folders)
            {
                var found = FindScreensCollectionContainingSub(sub, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
