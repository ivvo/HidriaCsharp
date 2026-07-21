using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    // Vržena, ko obstoječa "BackupFile.all" ne ustreza vzorcem, ki jih generira Template.cs (regija
    // Yamaha) - uvoz se ustavi namesto da bi tiho ugibal napačne podatke.
    public class YamahaImportException : Exception
    {
        public YamahaImportException(string message) : base(message) { }
    }

    // Parsanje že generiranega Yamaha projekta (V3) nazaj v model. Pri Yamahi je celoten program
    // (koda + naučene točke + IO oznake) v ENI datoteki "BackupFile.all". Podprt je EN robot na
    // projekt (kot pri KUKA/ABB) - generator sicer zna zapisati več robotov v isto datoteko, a MAIN
    // odsek ("SWI <ROBOT>") upošteva le prvega, zato uvoz več-robotskih Yamaha datotek zavrnemo.
    //
    // Postaje se preberejo iz glave (spremenljivke pozicij "Ime = indeks") - to je zanesljiv,
    // urejen seznam; oznake "*..._go...:" vsebujejo tudi ne-postajno pomožno "*..._goPosition:".
    public static class YamahaProjectImporter
    {
        // Sidrano na začetek vrstice: sama oznaka je v stolpcu 0 ("*robot_goHome:"), medtem ko se
        // ista niz pojavi tudi v komentarju nad njo ("'----...*robot_goHome:") - tega ne smemo šteti.
        private static readonly Regex GoHomeLabelRegex = new Regex(@"(?m)^\*(\w+)_goHome:", RegexOptions.Compiled);
        private static readonly Regex PositionVarRegex = new Regex(@"^(\w+) = (\d+)\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
        private const string PosVarsMarker = "'position variable values, one number for each station";

        public static ObservableCollection<ProgramModel> Import(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new YamahaImportException($"Mapa '{folderPath}' ne obstaja.");

            var allFiles = Directory.GetFiles(folderPath, "*.all");
            if (allFiles.Length == 0)
                throw new YamahaImportException($"V mapi '{folderPath}' ni najdena nobena '.all' datoteka (Yamaha). Izberi mapo z 'BackupFile.all' (npr. YamahaGeneratedTemplate_...).");
            if (allFiles.Length > 1)
                throw new YamahaImportException($"V mapi '{folderPath}' je več '.all' datotek - pričakovana je ena (BackupFile.all).");

            string filePath = allFiles[0];
            string content = File.ReadAllText(filePath);

            var goHomeMatches = GoHomeLabelRegex.Matches(content);
            if (goHomeMatches.Count == 0)
                throw new YamahaImportException(
                    $"Datoteka '{Path.GetFileName(filePath)}' ne vsebuje '*..._goHome:' - ni v formatu, ki bi ga generiralo to orodje (Yamaha).");
            if (goHomeMatches.Count > 1)
                throw new YamahaImportException(
                    "Uvoz podpira samo Yamaha projekte z enim robotom (najdenih je več robotov v isti BackupFile.all).");

            string programNameLower = goHomeMatches[0].Groups[1].Value;

            // RobotNumber iz "NAME=<UPPER>\nPGN=<n>" (RobotNumber = n - 1). Če ga ne najdemo, privzeto 1.
            int robotNumber = 1;
            var pgnMatch = Regex.Match(content, @"NAME=" + Regex.Escape(programNameLower.ToUpper()) + @"\s*\r?\nPGN=(\d+)");
            if (pgnMatch.Success) robotNumber = int.Parse(pgnMatch.Groups[1].Value) - 1;

            var program = new ProgramModel(programNameLower, robotNumber);
            program.Stations.Clear();

            // --- seznam postaj iz glave ("Ime = indeks") ---
            int marker = content.IndexOf(PosVarsMarker, StringComparison.Ordinal);
            if (marker < 0)
                throw new YamahaImportException($"V '{Path.GetFileName(filePath)}' ni bilo mogoče najti glave s spremenljivkami pozicij.");
            int scanStart = content.IndexOf('\n', marker);
            if (scanStart < 0) scanStart = marker;

            var stationsByIndex = new SortedDictionary<int, string>();
            // Preberi zaporedne "Ime = indeks" vrstice takoj za markerjem.
            foreach (Match m in PositionVarRegex.Matches(content, scanStart))
            {
                // Ustavi se pri prvi vrstici, ki ni tik za prejšnjimi (vmesni komentar '\''): dovolimo
                // le neprekinjen blok. Preprosto: sprejmi vse "Ime = st", dokler indeksi tečejo 0..N.
                int idx = int.Parse(m.Groups[2].Value);
                if (!stationsByIndex.ContainsKey(idx) && idx == stationsByIndex.Count)
                    stationsByIndex[idx] = m.Groups[1].Value;
                else
                    break;
            }

            if (stationsByIndex.Count == 0)
                throw new YamahaImportException($"V glavi '{Path.GetFileName(filePath)}' ni bilo mogoče prebrati nobene postaje.");

            // --- [PNM]/[PNT]/[PCM] za pozicije in naučene koordinate ---
            var pnm = ParseIndexedSection(content, "[PNM]", "PN"); // idx -> ime točke
            var pnt = ParseIndexedSection(content, "[PNT]", "P");   // idx -> koordinate
            var pcm = ParseIndexedSection(content, "[PCM]", "PC");  // idx -> komentar
            var coordsByName = new Dictionary<string, string>();
            var commentByName = new Dictionary<string, string>();
            foreach (var kv in pnm)
            {
                if (pnt.TryGetValue(kv.Key, out var c)) coordsByName[kv.Value] = c;
                if (pcm.TryGetValue(kv.Key, out var cm)) commentByName[kv.Value] = cm;
            }

            var dot = CultureInfo.InvariantCulture;

            foreach (var kv in stationsByIndex)
            {
                string stationName = kv.Value;

                bool freeEnabled = content.Contains($"O_{stationName}_FREE");
                int positions = DetectPositions(pnm.Values, stationName);

                var station = new StationModel(stationName, freeEnabled) { Positions = positions };

                // Naučene koordinate primarne točke (za prikaz v tabeli). Primarna točka:
                // "p{S}1" pri večpozicijski, sicer "p{S}".
                string primaryName = positions != 1 ? $"p{stationName}1" : $"p{stationName}";
                if (coordsByName.TryGetValue(primaryName, out var coordLine))
                {
                    var parts = coordLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 6)
                    {
                        station.Xcord = ParseD(parts[0], dot);
                        station.Ycord = ParseD(parts[1], dot);
                        station.Zcord = ParseD(parts[2], dot);
                        station.R1cord = ParseD(parts[3], dot);
                        station.R2cord = ParseD(parts[4], dot);
                        station.R3cord = ParseD(parts[5], dot);
                    }
                }
                if (commentByName.TryGetValue(primaryName, out var cmt))
                    station.RobotStationComment = cmt;

                program.Stations.Add(station);
            }

            return new ObservableCollection<ProgramModel> { program };
        }

        // Positions = najvišji N v imenih "p{S}{N}" (npr. pEvacuate1, pEvacuate2 -> 2); sicer 1.
        private static int DetectPositions(IEnumerable<string> pointNames, string stationName)
        {
            var rx = new Regex("^p" + Regex.Escape(stationName) + @"(\d+)$");
            int max = 0;
            foreach (var name in pointNames)
            {
                var m = rx.Match(name);
                if (m.Success) max = Math.Max(max, int.Parse(m.Groups[1].Value));
            }
            return max >= 1 ? max : 1;
        }

        // Prebere sekcijo (npr. "[PNT]") v slovar indeks -> vrednost za vrstice "<prefix><n>=<vrednost>",
        // do naslednje "[...]" sekcije.
        private static SortedDictionary<int, string> ParseIndexedSection(string content, string sectionHeader, string linePrefix)
        {
            var result = new SortedDictionary<int, string>();
            int start = content.IndexOf(sectionHeader, StringComparison.Ordinal);
            if (start < 0) return result;
            start += sectionHeader.Length;
            int end = content.IndexOf("\n[", start, StringComparison.Ordinal);
            if (end < 0) end = content.Length;
            string section = content.Substring(start, end - start);

            var rx = new Regex("^" + Regex.Escape(linePrefix) + @"(\d+)=(.*)$", RegexOptions.Multiline);
            foreach (Match m in rx.Matches(section))
                result[int.Parse(m.Groups[1].Value)] = m.Groups[2].Value.TrimEnd('\r');
            return result;
        }

        private static double ParseD(string s, IFormatProvider fp)
        {
            return double.TryParse(s, NumberStyles.Float, fp, out var d) ? d : 0.0;
        }
    }
}
