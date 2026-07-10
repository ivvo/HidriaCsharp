using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class KukaHellaImportException : Exception
    {
        public KukaHellaImportException(string message) : base(message) { }
    }

    // Parsanje že generiranega KUKA Hella projekta (glej Template.cs, regija KUKA Hella) nazaj v model.
    // Postaje se prepoznajo po "GLOBAL DEF robot_go<Station> ()" funkcijah v "<program>_motion.src" -
    // to je edino mesto, kjer je vsaka postaja samostojen blok. Omejitev: podprt je samo en robot na
    // projekt (generator ima znano omejitev pri več robotih - $config.dat/robotFunctions.src se v tem
    // primeru podvojijo, kar ni veljavna KRL koda, zato uvoz več-robotskih projektov zavrnemo).
    public static class KukaHellaProjectImporter
    {
        private static readonly Regex GoFunctionRegex = new Regex(@"GLOBAL DEF robot_go(\w+)\s*\(\)", RegexOptions.Compiled);

        public static ObservableCollection<ProgramModel> Import(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new KukaHellaImportException($"Mapa '{folderPath}' ne obstaja.");

            string programDir = Path.Combine(folderPath, "R1", "Program");
            if (!Directory.Exists(programDir))
                throw new KukaHellaImportException($"Mapa '{programDir}' ne obstaja - ali gre res za KUKA Hella projekt?");

            var motionFiles = Directory.GetFiles(programDir, "*_motion.src");
            if (motionFiles.Length == 0)
                throw new KukaHellaImportException($"V mapi '{programDir}' ni najdena nobena '_motion.src' datoteka.");
            if (motionFiles.Length > 1)
                throw new KukaHellaImportException(
                    "Uvoz trenutno podpira samo KUKA Hella projekte z enim robotom - generator ima znano omejitev, " +
                    "da se pri več robotih $config.dat in robotFunctions.src podvojita, kar ni veljavna KRL koda.");

            string motionFile = motionFiles[0];
            string content = File.ReadAllText(motionFile);
            var matches = GoFunctionRegex.Matches(content);
            if (matches.Count == 0)
                throw new KukaHellaImportException(
                    $"Datoteka '{Path.GetFileName(motionFile)}' ne vsebuje nobene 'GLOBAL DEF robot_go...()' funkcije - " +
                    "ni v formatu, ki bi ga generiralo to orodje (KUKA Hella).");

            string fileStem = Path.GetFileNameWithoutExtension(motionFile); // "{robot}_motion"
            const string suffix = "_motion";
            string programNameLower = fileStem.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? fileStem.Substring(0, fileStem.Length - suffix.Length)
                : fileStem;
            // Opomba: izvirna mešana velikost črk imena programa se ne da natančno obnoviti iz generirane kode.
            string reconstructedProgramName = char.ToUpper(programNameLower[0]) + programNameLower.Substring(1);

            var program = new ProgramModel(reconstructedProgramName, 1);
            program.Stations.Clear();

            string moveOnStationBlock = ExtractBlockOrThrow(content, "GLOBAL DEF robot_moveOnStation()", motionFile);

            foreach (Match m in matches)
            {
                string stationName = m.Groups[1].Value;
                var probe = new StationModel(stationName, false);
                string stationUpper = probe.RobotStationNameToUpper;

                bool freeEnabled = false;
                string caseMarker = $"CASE #TO_{stationUpper}";
                int caseIdx = moveOnStationBlock.IndexOf(caseMarker, StringComparison.Ordinal);
                if (caseIdx >= 0)
                {
                    int nextCaseIdx = moveOnStationBlock.IndexOf("CASE #TO_", caseIdx + caseMarker.Length, StringComparison.Ordinal);
                    int endIdx = nextCaseIdx >= 0 ? nextCaseIdx : moveOnStationBlock.Length;
                    string caseBody = moveOnStationBlock.Substring(caseIdx, endIdx - caseIdx);
                    freeEnabled = caseBody.Contains($"do{stationUpper}_FREE");
                }

                // KUKA generator (SimpleProgram varianta) ne podpira Pallet/večpozicijskih postaj -
                // ta podatka pri KUKA postajah ostaneta na privzetih vrednostih (false / 1).
                program.Stations.Add(new StationModel(stationName, freeEnabled));
            }

            return new ObservableCollection<ProgramModel> { program };
        }

        private static string ExtractBlockOrThrow(string content, string startMarker, string filePathForError)
        {
            int startIdx = content.IndexOf(startMarker, StringComparison.Ordinal);
            if (startIdx < 0)
                throw new KukaHellaImportException(
                    $"V datoteki '{Path.GetFileName(filePathForError)}' ni bilo mogoče najti '{startMarker}'.");

            var endMatch = Regex.Match(content.Substring(startIdx), @"\bEND\b");
            if (!endMatch.Success)
                throw new KukaHellaImportException(
                    $"Manjkajoč zaključek 'END' za '{startMarker}' v datoteki '{Path.GetFileName(filePathForError)}'.");

            return content.Substring(startIdx, endMatch.Index);
        }
    }
}
