using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    // Vržena, ko obstoječa ".all" datoteka ne ustreza vzorcem, ki jih generira Template.cs (regija
    // YAMAHA) - uvoz se ustavi namesto da bi tiho ugibal napačne podatke.
    public class YamahaImportException : Exception
    {
        public YamahaImportException(string message) : base(message) { }
    }

    // Parsanje že generiranega Yamaha projekta nazaj v model. Za razliko od Epson/KUKA/ABB je pri
    // Yamahi celoten program (koda + naučene točke + IO oznake) v ENI ".all" datoteki, in datoteke so
    // med seboj neodvisne (nič ni skupnega med robotoma), zato tu ni omejitve "en robot na projekt" -
    // vsaka ".all" datoteka postane svoj program.
    //
    // Postaje se prepoznajo po "*robot_go<Postaja>:" oznakah (edino mesto, kjer je vsaka postaja
    // samostojna oznaka; klici "GOSUB *robot_go<Postaja>" nimajo dvopičja in se ne ujamejo).
    public static class YamahaProjectImporter
    {
        private static readonly Regex GoLabelRegex = new Regex(@"\*robot_go(\w+):", RegexOptions.Compiled);
        private static readonly Regex AdditionalPosRegex = new Regex(@"additional_pos=(\d+)", RegexOptions.Compiled);

        public static ObservableCollection<ProgramModel> Import(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new YamahaImportException($"Mapa '{folderPath}' ne obstaja.");

            var allFiles = Directory.GetFiles(folderPath, "*.all")
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (allFiles.Count == 0)
                throw new YamahaImportException($"V mapi '{folderPath}' ni najdena nobena '.all' datoteka (Yamaha).");

            var result = new ObservableCollection<ProgramModel>();
            int robotIndex = 0;
            foreach (var file in allFiles)
            {
                robotIndex++;
                result.Add(ImportProgram(file, robotIndex));
            }
            return result;
        }

        private static ProgramModel ImportProgram(string filePath, int robotNumber)
        {
            string content = File.ReadAllText(filePath);
            var matches = GoLabelRegex.Matches(content);
            if (matches.Count == 0)
                throw new YamahaImportException(
                    $"Datoteka '{Path.GetFileName(filePath)}' ne vsebuje nobene '*robot_go...:' oznake - " +
                    "ni v formatu, ki bi ga generiralo to orodje (Yamaha).");

            // Ime programa: Yamaha koda uporablja fiksno predpono "*robot_" (ne imena programa),
            // zato se ime lahko obnovi samo iz imena datoteke - in to natančno (ohrani velikost črk).
            string programName = Path.GetFileNameWithoutExtension(filePath);

            var program = new ProgramModel(programName, robotNumber);
            program.Stations.Clear();

            string moveOnStationBlock = ExtractBetween(content, "*robot_moveOnStation:", "*robot_moveAway:");
            if (string.IsNullOrEmpty(moveOnStationBlock))
                throw new YamahaImportException(
                    $"V datoteki '{Path.GetFileName(filePath)}' ni bilo mogoče najti bloka '*robot_moveOnStation:' - datoteka ni v pričakovanem formatu.");

            foreach (Match m in matches)
            {
                string stationName = m.Groups[1].Value;

                // O_<Postaja>_FREE se pojavi samo za postaje z omogočenim "free" signalom (Home nikoli).
                bool freeEnabled = content.Contains($"O_{stationName}_FREE");
                int positions = DetectPositions(moveOnStationBlock, stationName);

                program.Stations.Add(new StationModel(stationName, freeEnabled)
                {
                    Positions = positions
                    // Yamaha ne podpira Pallet - ostane privzeto false.
                });
            }

            return program;
        }

        // Iz "*robot_moveOnStation:" veje za dano postajo prebere število fizičnih pozicij: če veja
        // vsebuje "additional_pos" izbor, je Positions = najvišja tam navedena vrednost, sicer 1.
        private static int DetectPositions(string moveOnStationBlock, string stationName)
        {
            string marker = $"PositionTo = {stationName} THEN";
            int idx = moveOnStationBlock.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return 1;

            int nextIdx = moveOnStationBlock.IndexOf("PositionTo = ", idx + marker.Length, StringComparison.Ordinal);
            int endIdx = nextIdx >= 0 ? nextIdx : moveOnStationBlock.Length;
            string branch = moveOnStationBlock.Substring(idx, endIdx - idx);

            var posMatches = AdditionalPosRegex.Matches(branch);
            if (posMatches.Count == 0) return 1;

            return posMatches.Cast<Match>().Max(pm => int.Parse(pm.Groups[1].Value));
        }

        private static string ExtractBetween(string content, string startMarker, string endMarker)
        {
            int startIdx = content.IndexOf(startMarker, StringComparison.Ordinal);
            if (startIdx < 0) return "";
            int endIdx = content.IndexOf(endMarker, startIdx + startMarker.Length, StringComparison.Ordinal);
            if (endIdx < 0) return content.Substring(startIdx);
            return content.Substring(startIdx, endIdx - startIdx);
        }
    }
}
