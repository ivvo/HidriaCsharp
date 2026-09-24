using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Siemens.Engineering;
using Siemens.Engineering.HW;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.SW.Tags;
using Siemens.Engineering.Hmi;
using Siemens.Engineering.Hmi.Screen;

namespace ExportEntireProject
{
    /// <summary>
    /// Exporta CEL TIA Portal projekt v XML: za vsak PLC device - vse bloke (OB/FB/FC/DB),
    /// UDT-je in tag tabele; za vsak HMI device - vse zaslone (rekurzivno po vseh podmapah).
    ///
    /// OMEJITVE (namerno NI vključeno, ker Openness tega ne podpira enostavno):
    ///   - HW konfiguracija naprav/modulov/omrežnih povezav
    ///   - Screen Templates, Faceplates - glej TODO spodaj, ni preverjeno
    ///   - Alarmi, recepti, text/graphic liste - dodaj po enakem vzorcu, če jih potrebuješ
    ///
    /// Program se poveže na ŽE ODPRT TIA Portal proces in NIČESAR ne piše nazaj v projekt -
    /// samo bere in exporta.
    /// </summary>
    class Program
    {
        static readonly string OutputRoot = @"C:\TIA_FullExport";

        static void Main(string[] args)
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("=== NAPAKA ===");
                Console.WriteLine(ex.GetType().Name + ": " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Pritisni Enter za izhod...");
                Console.ReadLine();
            }
        }

        static void Run()
        {
            var processes = TiaPortal.GetProcesses();
            if (processes.Count == 0)
            {
                Console.WriteLine("Ni najdenega odprtega TIA Portal procesa. Odpri projekt in poženi znova.");
                return;
            }

            TiaPortal tiaPortal = processes[0].Attach();
            Project project = tiaPortal.Projects[0];
            Console.WriteLine($"Povezan na projekt: {project.Name}");

            Directory.CreateDirectory(OutputRoot);

            int plcCount = 0, hmiCount = 0;

            foreach (Device device in GetAllDevices(project))
            {
                foreach (var item in AllDeviceItemsRecursive(device.DeviceItems))
                {
                    var container = item.GetService<SoftwareContainer>();
                    if (container == null) continue;

                    if (container.Software is PlcSoftware plcSoftware)
                    {
                        plcCount++;
                        string plcPath = Path.Combine(OutputRoot, Sanitize(device.Name), "PLC");
                        Console.WriteLine($"[PLC] {device.Name} -> {plcPath}");
                        ExportPlc(plcSoftware, plcPath);
                    }
                    else if (container.Software is HmiTarget hmiTarget)
                    {
                        hmiCount++;
                        string hmiPath = Path.Combine(OutputRoot, Sanitize(device.Name), "HMI", "Screens");
                        Console.WriteLine($"[HMI] {device.Name} (target: {hmiTarget.Name}) -> {hmiPath}");
                        Directory.CreateDirectory(hmiPath);
                        ExportAllScreens(hmiTarget.ScreenFolder, hmiPath);

                        // TODO: Screen Templates / Faceplates - ime property-ja na HmiTarget
                        // (npr. hmiTarget.TemplateFolder ali podobno) ni bilo preverjeno.
                        // Preveri v Visual Studiu z IntelliSense na "hmiTarget." in dopolni,
                        // če potrebuješ tudi templates v exportu.
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Končano. PLC naprav: {plcCount}, HMI naprav: {hmiCount}.");
            Console.WriteLine($"Vse v: {OutputRoot}");
        }

        // ===================== PLC EXPORT =====================

        static void ExportPlc(PlcSoftware plcSoftware, string basePath)
        {
            string blocksPath = Path.Combine(basePath, "Blocks");
            string typesPath = Path.Combine(basePath, "Types");
            string tagsPath = Path.Combine(basePath, "TagTables");

            Directory.CreateDirectory(blocksPath);
            Directory.CreateDirectory(typesPath);
            Directory.CreateDirectory(tagsPath);

            int blockCount = ExportBlockGroup(plcSoftware.BlockGroup, blocksPath);
            int typeCount = ExportTypeGroup(plcSoftware.TypeGroup, typesPath);
            int tagTableCount = ExportTagTableGroup(plcSoftware.TagTableGroup, tagsPath);

            Console.WriteLine($"    Bloki: {blockCount}, UDT-ji: {typeCount}, Tag tabele: {tagTableCount}");
        }

        static int ExportBlockGroup(PlcBlockGroup group, string path)
        {
            int count = 0;
            Directory.CreateDirectory(path);
            foreach (PlcBlock block in group.Blocks)
            {
                try
                {
                    block.Export(new FileInfo(Path.Combine(path, Sanitize(block.Name) + ".xml")), ExportOptions.WithDefaults);
                    count++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    [PRESKOČEN] blok '{block.Name}': {ex.Message}");
                }
            }
            foreach (PlcBlockUserGroup subGroup in group.Groups)
            {
                count += ExportBlockGroup(subGroup, Path.Combine(path, Sanitize(subGroup.Name)));
            }
            return count;
        }

        static int ExportTypeGroup(PlcTypeGroup group, string path)
        {
            int count = 0;
            Directory.CreateDirectory(path);
            foreach (PlcType type in group.Types)
            {
                try
                {
                    type.Export(new FileInfo(Path.Combine(path, Sanitize(type.Name) + ".xml")), ExportOptions.WithDefaults);
                    count++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    [PRESKOČEN] UDT '{type.Name}': {ex.Message}");
                }
            }
            foreach (PlcTypeUserGroup subGroup in group.Groups)
            {
                count += ExportTypeGroup(subGroup, Path.Combine(path, Sanitize(subGroup.Name)));
            }
            return count;
        }

        static int ExportTagTableGroup(PlcTagTableGroup group, string path)
        {
            int count = 0;
            Directory.CreateDirectory(path);
            foreach (PlcTagTable tagTable in group.TagTables)
            {
                try
                {
                    tagTable.Export(new FileInfo(Path.Combine(path, Sanitize(tagTable.Name) + ".xml")), ExportOptions.WithDefaults);
                    count++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"    [PRESKOČENA] tag tabela '{tagTable.Name}': {ex.Message}");
                }
            }
            foreach (PlcTagTableUserGroup subGroup in group.Groups)
            {
                count += ExportTagTableGroup(subGroup, Path.Combine(path, Sanitize(subGroup.Name)));
            }
            return count;
        }

        // ===================== HMI EXPORT =====================

        static void ExportAllScreens(ScreenSystemFolder rootFolder, string path)
        {
            Directory.CreateDirectory(path);
            foreach (Screen screen in rootFolder.Screens)
            {
                screen.Export(new FileInfo(Path.Combine(path, Sanitize(screen.Name) + ".xml")), ExportOptions.WithDefaults);
            }
            foreach (ScreenUserFolder sub in rootFolder.Folders)
            {
                ExportScreensOfFolder(sub, Path.Combine(path, Sanitize(sub.Name)));
            }
        }

        static void ExportScreensOfFolder(ScreenUserFolder folder, string path)
        {
            Directory.CreateDirectory(path);
            foreach (Screen screen in folder.Screens)
            {
                screen.Export(new FileInfo(Path.Combine(path, Sanitize(screen.Name) + ".xml")), ExportOptions.WithDefaults);
            }
            foreach (ScreenUserFolder sub in folder.Folders)
            {
                ExportScreensOfFolder(sub, Path.Combine(path, Sanitize(sub.Name)));
            }
        }

        // ===================== SPLOŠNI HELPERJI =====================

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

        static IEnumerable<DeviceItem> AllDeviceItemsRecursive(DeviceItemComposition items)
        {
            foreach (DeviceItem item in items)
            {
                yield return item;
                foreach (var sub in AllDeviceItemsRecursive(item.DeviceItems))
                    yield return sub;
            }
        }

        static string Sanitize(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
