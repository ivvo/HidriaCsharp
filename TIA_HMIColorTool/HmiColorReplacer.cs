using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HmiColorReplacer
{
    /// <summary>
    /// Orodje za bulk zamenjavo RGB barvnih vrednosti v exportiranih XML datotekah
    /// TIA Portal HMI konfiguracije (Openness "Export extended HMI configuration data").
    ///
    /// UPORABA:
    ///   1. V TIA Portal Openness (ali File > Export) exportaj HMI screens/templates
    ///      v XML v neko mapo.
    ///   2. Odpri eno XML datoteko in poglej, KAKO je barva dejansko zapisana
    ///      (decimalno "R,G,B", hex "#RRGGBB", ali Windows BGR hex brez #).
    ///      Če se oblika razlikuje od spodnjih treh vzorcev, mi pošlji izsek XML-ja
    ///      okoli ene barve in prilagodim regex.
    ///   3. Poženi:  dotnet run -- "C:\pot\do\exported_xml_mape"
    ///      (ali kompiliraj kot .exe in poženi z argumentom poti)
    ///   4. Preveri _backup mapo in spremenjene datoteke, nato re-importaj v TIA Portal.
    ///
    /// VARNOST: Orodje NAJPREJ naredi kopijo cele mape (*_backup_<timestamp>),
    /// preden karkoli spremeni. Original ostane nedotaknjen v backup mapi.
    /// </summary>
    class Program
    {
        // -----------------------------------------------------------------
        // TUKAJ VPIŠI SVOJE BARVNE ZAMENJAVE (stara -> nova), v decimalni "R,G,B" obliki
        // -----------------------------------------------------------------
        static readonly Dictionary<(int R, int G, int B), (int R, int G, int B)> ColorMap = new()
        {
            { (59, 101, 151),  (24, 106, 154) },   // glavna modra
            { (204, 255, 255), (116, 198, 199) },  // svetlo modra (ni več dovoljena)
            // dodaj poljubno več parov po istem vzorcu:
            // { (R_staro, G_staro, B_staro), (R_novo, G_novo, B_novo) },
        };

        static int filesChanged = 0;
        static int totalReplacements = 0;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Uporaba: HmiColorReplacer <mapa_z_exportiranimi_xml_datotekami>");
                return;
            }

            string sourceDir = args[0];
            if (!Directory.Exists(sourceDir))
            {
                Console.WriteLine($"Mapa ne obstaja: {sourceDir}");
                return;
            }

            // Varnostna kopija pred spremembami
            string backupDir = sourceDir.TrimEnd(Path.DirectorySeparatorChar)
                                + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            CopyDirectory(sourceDir, backupDir);
            Console.WriteLine($"Varnostna kopija ustvarjena: {backupDir}");

            var xmlFiles = Directory.GetFiles(sourceDir, "*.xml", SearchOption.AllDirectories);
            Console.WriteLine($"Najdenih {xmlFiles.Length} XML datotek.");

            foreach (var file in xmlFiles)
            {
                ProcessFile(file);
            }

            Console.WriteLine();
            Console.WriteLine($"Končano. Spremenjenih datotek: {filesChanged}, skupno zamenjav: {totalReplacements}");
        }

        static void ProcessFile(string path)
        {
            string content = File.ReadAllText(path);
            string original = content;
            int fileReplacements = 0;

            foreach (var kvp in ColorMap)
            {
                var oldC = kvp.Key;
                var newC = kvp.Value;

                // Vzorec 1: decimalne vrednosti ločene z vejico, npr. "59,101,151" ali "59, 101, 151"
                string decimalPattern = $@"\b{oldC.R}\s*,\s*{oldC.G}\s*,\s*{oldC.B}\b";
                string decimalReplacement = $"{newC.R},{newC.G},{newC.B}";
                content = ReplaceAndCount(content, decimalPattern, decimalReplacement, ref fileReplacements);

                // Vzorec 2: hex oblika #RRGGBB
                string hexOld = $"#{oldC.R:X2}{oldC.G:X2}{oldC.B:X2}";
                string hexNew = $"#{newC.R:X2}{newC.G:X2}{newC.B:X2}";
                content = ReplaceAndCount(content, Regex.Escape(hexOld), hexNew, ref fileReplacements, ignoreCase: true);

                // Vzorec 3: Windows COLORREF hex brez #, v obratnem BGR vrstnem redu
                // (pogosto interno pri WinCC), npr. 97653B za RGB(59,101,151)
                string bgrOld = $"{oldC.B:X2}{oldC.G:X2}{oldC.R:X2}";
                string bgrNew = $"{newC.B:X2}{newC.G:X2}{newC.R:X2}";
                content = ReplaceAndCount(content, Regex.Escape(bgrOld), bgrNew, ref fileReplacements, ignoreCase: true);
            }

            if (content != original)
            {
                File.WriteAllText(path, content);
                filesChanged++;
                totalReplacements += fileReplacements;
                Console.WriteLine($"  {Path.GetFileName(path)}: {fileReplacements} zamenjav");
            }
        }

        static string ReplaceAndCount(string content, string pattern, string replacement, ref int counter, bool ignoreCase = false)
        {
            var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            var matches = Regex.Matches(content, pattern, options);
            counter += matches.Count;
            return Regex.Replace(content, pattern, replacement, options);
        }

        static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(source, file);
                string destFile = Path.Combine(dest, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(file, destFile, true);
            }
        }
    }
}
