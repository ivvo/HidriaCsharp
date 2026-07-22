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
    public class KawasakiImportException : Exception
    {
        public KawasakiImportException(string message) : base(message) { }
    }

    // Parsanje že generiranega Kawasaki projekta (V3) nazaj v model. Celoten program (koda + naučene
    // točke + IO) je v eni datoteki "ProgramFile.as".
    //
    // Postaje se preberejo iz odseka parameters() ("position values, one number for each station"):
    // "HomePos = 0" (postaja Home), nato "Ime = indeks", do "onlymoveaway". Kawasaki najbližjo točko
    // ob homingu išče po IMENU (DISTANCE), ne po indeksu, zato ni številčne past kot pri Epson/Yamaha.
    public static class KawasakiProjectImporter
    {
        private const string PosVarsMarker = "\"position values, one number for each station\"";
        private static readonly Regex VarRegex = new Regex(@"^\s*(\w+)\s*=\s*(\d+)\s*$", RegexOptions.Compiled);
        // Definicijska vrstica točke je v stolpcu 0: "p<Ime> x y z r1 r2 r3" ali "#pHome ...".
        private static readonly Regex PointDefRegex = new Regex(@"(?m)^(#?p\w+) (-?\d[-\d. ]*)$", RegexOptions.Compiled);

        public static ObservableCollection<ProgramModel> Import(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new KawasakiImportException($"Mapa '{folderPath}' ne obstaja.");

            var asFiles = Directory.GetFiles(folderPath, "*.as");
            if (asFiles.Length == 0)
                throw new KawasakiImportException($"V mapi '{folderPath}' ni najdena nobena '.as' datoteka (Kawasaki). Izberi mapo z 'ProgramFile.as' (npr. KawasakiGeneratedTemplate_...).");
            if (asFiles.Length > 1)
                throw new KawasakiImportException($"V mapi '{folderPath}' je več '.as' datotek - pričakovana je ena (ProgramFile.as).");

            string content = File.ReadAllText(asFiles[0]);

            int marker = content.IndexOf(PosVarsMarker, StringComparison.Ordinal);
            if (marker < 0)
                throw new KawasakiImportException("V datoteki ni bilo mogoče najti spremenljivk pozicij ('position values, one number for each station') - ni v pričakovanem formatu (Kawasaki).");

            // Preberi zaporedne "Ime = indeks" vrstice za markerjem, do "onlymoveaway" ali prve ne-var vrstice.
            var stationsByIndex = new SortedDictionary<int, string>();
            using (var reader = new StringReader(content.Substring(content.IndexOf('\n', marker) + 1)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed == ";") continue;
                    var m = VarRegex.Match(line);
                    if (!m.Success) break; // konec bloka spremenljivk pozicij
                    string name = m.Groups[1].Value;
                    int idx = int.Parse(m.Groups[2].Value);
                    if (name == "onlymoveaway" || idx == 99) break;
                    // indeks 0 je Home (spremenljivka se imenuje "HomePos", postaja pa "Home").
                    stationsByIndex[idx] = idx == 0 ? "Home" : name;
                }
            }

            if (stationsByIndex.Count == 0 || !stationsByIndex.ContainsKey(0))
                throw new KawasakiImportException("Iz odseka parameters() ni bilo mogoče prebrati seznama postaj.");

            // Imena in koordinate točk (za pozicije in prikaz).
            var pointCoords = new Dictionary<string, string>();
            foreach (Match m in PointDefRegex.Matches(content))
                pointCoords[m.Groups[1].Value] = m.Groups[2].Value.Trim();

            // Kawasaki generator ne uporablja imena/številke programa v izhodu; privzeto "robot"/1.
            var program = new ProgramModel("robot", 1);
            program.Stations.Clear();

            var dot = CultureInfo.InvariantCulture;
            foreach (var kv in stationsByIndex)
            {
                string stationName = kv.Value;
                bool free = content.Contains($"O_{stationName}_FREE");
                int positions = DetectPositions(pointCoords.Keys, stationName);

                var station = new StationModel(stationName, free) { Positions = positions };

                // Koordinate primarne točke (za prikaz). Primarna: "p<S>1" pri večpozicijski, sicer "p<S>"
                // (Home: "#pHome").
                string primary = stationName == "Home" ? "#pHome" : (positions != 1 ? $"p{stationName}1" : $"p{stationName}");
                if (pointCoords.TryGetValue(primary, out var coord))
                {
                    var parts = coord.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

                program.Stations.Add(station);
            }

            return new ObservableCollection<ProgramModel> { program };
        }

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

        private static double ParseD(string s, IFormatProvider fp)
            => double.TryParse(s, NumberStyles.Float, fp, out var d) ? d : 0.0;
    }
}
