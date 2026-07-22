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
    // Vržena, ko obstoječa koda ne ustreza vzorcem, ki jih generira Template.cs -
    // uvoz se ustavi namesto da bi tiho ugibal napačne podatke.
    public class EpsonImportException : Exception
    {
        public EpsonImportException(string message) : base(message) { }
    }

    // Parsanje že generiranega Epson Hidria projekta (glej Template.cs, regija EPSON) nazaj v model.
    // Postaje se prepoznajo po dobesednih vzorcih, ki jih GetMovementsSimpleProgramFunc zapiše v <program>.prg -
    // to je edino mesto v generirani kodi, kjer je vsaka postaja samostojen "Function ..._go<Station>() ... Fend" blok.
    public static class EpsonProjectImporter
    {
        private static readonly Regex GoFunctionRegex = new Regex(@"Function\s+(\w+)_go(\w+)\s*\(\)", RegexOptions.Compiled);
        private static readonly Regex RobotNumberRegex = new Regex(@"Robot\s+(\d+)", RegexOptions.Compiled);

        public static ObservableCollection<ProgramModel> Import(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new EpsonImportException($"Mapa '{folderPath}' ne obstaja.");

            var prgFiles = Directory.GetFiles(folderPath, "*.prg")
                .Where(f => !Path.GetFileNameWithoutExtension(f).Equals("Main", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (prgFiles.Count == 0)
                throw new EpsonImportException($"V mapi '{folderPath}' ni najdena nobena programska (.prg) datoteka robota (razen Main.prg).");

            var result = new ObservableCollection<ProgramModel>();
            int robotIndex = 0;
            foreach (var file in prgFiles)
            {
                string content = File.ReadAllText(file);
                // Preskoči .prg brez "Function ..._go...()" funkcij: to niso robotski programi, ampak
                // pomožni programi (komunikacija, strojni vid ... npr. getOrientation.prg, HandIO.prg).
                // Ti ne smejo pokvariti uvoza - beremo samo Main.prg in robotske programe (robot.prg ...).
                if (!GoFunctionRegex.IsMatch(content)) continue;
                robotIndex++;
                result.Add(ImportProgram(file, content, folderPath, robotIndex));
            }

            if (result.Count == 0)
                throw new EpsonImportException(
                    $"V mapi '{folderPath}' ni bilo najdene nobene robotske .prg datoteke s 'Function ..._go...()' funkcijami " +
                    "(poleg Main.prg in morebitnih pomožnih programov).");

            return result;
        }

        private static ProgramModel ImportProgram(string filePath, string content, string folderPath, int fallbackRobotNumber)
        {
            var matches = GoFunctionRegex.Matches(content);
            string programNameLower = matches[0].Groups[1].Value;
            // Opomba: izvirna mešana velikost črk imena programa se ne da natančno obnoviti,
            // ker se v generirani kodi pojavljata samo ToLower()/ToUpper() varianta.
            string reconstructedProgramName = char.ToUpper(programNameLower[0]) + programNameLower.Substring(1);

            var robotNumberMatch = RobotNumberRegex.Match(content);
            int robotNumber = robotNumberMatch.Success ? int.Parse(robotNumberMatch.Groups[1].Value) : fallbackRobotNumber;

            var program = new ProgramModel(reconstructedProgramName, robotNumber);
            program.Stations.Clear();

            // Naučene koordinate iz "<program>1.pts" (sLabel -> rX/rY/rZ), za PRIKAZ v tabeli.
            // Iskanje po imenu je NEobčutljivo na velikost črk, ker imena postaj (go-funkcije),
            // imena točk v kodi in oznake v .pts v resničnih projektih niso vedno enako zapisana
            // (npr. postaja "ParTake1", koda "pParttake1", .pts "pPartTake1").
            var coords = ReadPtsCoordinates(folderPath, programNameLower);

            foreach (Match m in matches)
            {
                string stationName = m.Groups[2].Value;
                string functionBody = ExtractFunctionBody(content, m.Index, filePath);

                var probe = new StationModel(stationName, false);
                string stationUpper = probe.RobotStationNameToUpper;

                bool freeEnabled = functionBody.Contains($"_O_{stationUpper}_FREE");
                bool pallet = functionBody.Contains($"Go Pallet ({stationName},");
                int positions = 1;

                if (functionBody.Contains($"Call go{stationName}MaxZHeight"))
                    positions = CountAdditionalPositions(content, stationName);

                var station = new StationModel(stationName, freeEnabled)
                {
                    Pallet = pallet,
                    Positions = positions
                };

                // Koordinate: najprej poišči točko, katere IME ustreza imenu postaje (neobčutljivo na
                // velikost črk, brez končnih številk pozicije - npr. "pCheckweight1" -> "CheckWeight").
                // Če je ni (npr. postaja "ParTake1" a točka "pPartTake1"), se zatečemo na prvo "p..."
                // referenco v telesu go-funkcije, ki obstaja v .pts.
                string pointLabel = FindPointLabelByStationName(stationName, coords)
                                    ?? FindPointLabelInBody(functionBody, coords);
                if (pointLabel != null && coords.TryGetValue(pointLabel, out var c))
                {
                    station.Xcord = c.x;
                    station.Ycord = c.y;
                    station.Zcord = c.z;
                }

                program.Stations.Add(station);
            }

            return program;
        }

        private static string ExtractFunctionBody(string content, int functionStartIndex, string filePath)
        {
            int fendIndex = content.IndexOf("Fend", functionStartIndex, StringComparison.Ordinal);
            if (fendIndex < 0)
                throw new EpsonImportException(
                    $"V datoteki '{Path.GetFileName(filePath)}' manjka zaključek 'Fend' za eno od 'go' funkcij - datoteka ni v pričakovanem formatu.");
            return content.Substring(functionStartIndex, fendIndex - functionStartIndex);
        }

        // --- Branje naučenih koordinat (rX/rY/rZ) iz .pts, za PRIKAZ v tabeli ---
        // Rotacije (rU/rV/rW) se zaenkrat NE berejo (Epson jih ima označene drugače kot ostali roboti).
        // Opomba: to je le za PRIKAZ; posodobitev (Update) naučene točke ohrani neodvisno (prekrivanje
        // celotnega .pts po imenu točke), poln Generate pa Epson točke tako ali tako zapiše kot ničle.
        private static readonly Regex PointBlockRegex = new Regex(@"Point\d+\s*\{(?<body>[\s\S]*?)\r?\n\}", RegexOptions.Compiled);
        private static readonly Regex SLabelRegex = new Regex("sLabel=\"(?<lbl>[^\"]*)\"", RegexOptions.Compiled);
        private static readonly Regex PointRefRegex = new Regex(@"\bp[A-Za-z]\w*\b", RegexOptions.Compiled);

        // Prebere "<program>1.pts" v slovar (NEobčutljiv na velikost črk): sLabel -> (rX, rY, rZ).
        private static Dictionary<string, (double x, double y, double z)> ReadPtsCoordinates(string folderPath, string progLower)
        {
            var coords = new Dictionary<string, (double x, double y, double z)>(StringComparer.OrdinalIgnoreCase);

            // Generator zapiše primarne točke v "<program>1.pts". Projekt ima lahko še druge točkovne
            // datoteke (robot2.pts ...), zato ciljamo natanko primarno.
            string preferred = progLower + "1.pts";
            var ptsFiles = Directory.GetFiles(folderPath, "*.pts");
            string ptsPath = ptsFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(preferred, StringComparison.OrdinalIgnoreCase))
                             ?? ptsFiles.FirstOrDefault(f => Path.GetFileName(f).ToLower().StartsWith(progLower))
                             ?? ptsFiles.FirstOrDefault();
            if (ptsPath == null) return coords;

            foreach (Match m in PointBlockRegex.Matches(File.ReadAllText(ptsPath)))
            {
                string body = m.Groups["body"].Value;
                var lbl = SLabelRegex.Match(body);
                if (!lbl.Success) continue;
                coords[lbl.Groups["lbl"].Value] = (
                    ReadField(body, "rX"), ReadField(body, "rY"), ReadField(body, "rZ"));
            }
            return coords;
        }

        private static readonly Regex TrailingDigitsRegex = new Regex(@"\d+$", RegexOptions.Compiled);

        // Poišče točko, katere IME (brez vodilnega "p" in končnih številk pozicije) se ujema z imenom
        // postaje - neobčutljivo na velikost črk. Pri večpozicijski postaji vrne točko z najnižjo
        // pozicijsko številko (primarno). Npr. postaja "CheckWeight" -> "pCheckweight1".
        private static string FindPointLabelByStationName(string stationName, Dictionary<string, (double x, double y, double z)> coords)
        {
            // Najprej natančno "p<Postaja>".
            string exact = "p" + stationName;
            if (coords.ContainsKey(exact)) return exact;

            string best = null;
            int bestNum = int.MaxValue;
            foreach (var label in coords.Keys)
            {
                string baseName = label.StartsWith("p", StringComparison.OrdinalIgnoreCase) ? label.Substring(1) : label;
                var dm = TrailingDigitsRegex.Match(baseName);
                int num = dm.Success ? int.Parse(dm.Value) : 0;
                if (dm.Success) baseName = baseName.Substring(0, baseName.Length - dm.Length);

                if (baseName.Equals(stationName, StringComparison.OrdinalIgnoreCase) && num < bestNum)
                {
                    best = label;
                    bestNum = num;
                }
            }
            return best;
        }

        // Vrne prvo "p..." referenco v telesu go-funkcije, ki je dejansko točka v .pts (npr. "pParttake1").
        // Zasilna pot za primere, ko se ime postaje in ime točke razlikujeta (npr. "ParTake1"/"pPartTake1").
        private static string FindPointLabelInBody(string functionBody, Dictionary<string, (double x, double y, double z)> coords)
        {
            foreach (Match m in PointRefRegex.Matches(functionBody))
                if (coords.ContainsKey(m.Value))
                    return m.Value;
            return null;
        }

        private static double ReadField(string body, string field)
        {
            var m = Regex.Match(body, field + @"=(-?\d+(?:\.\d+)?)");
            return m.Success && double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0.0;
        }

        private static int CountAdditionalPositions(string content, string stationName)
        {
            string marker = $"Function go{stationName}MaxZHeight";
            int idx = content.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return 1;

            int fendIndex = content.IndexOf("Fend", idx, StringComparison.Ordinal);
            string block = fendIndex > idx ? content.Substring(idx, fendIndex - idx) : content.Substring(idx);

            var caseMatches = Regex.Matches(block, @"Case\s+\d+");
            return caseMatches.Count > 0 ? caseMatches.Count : 1;
        }
    }
}
