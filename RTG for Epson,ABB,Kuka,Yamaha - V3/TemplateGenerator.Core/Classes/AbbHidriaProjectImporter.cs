using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class AbbHidriaImportException : Exception
    {
        public AbbHidriaImportException(string message) : base(message) { }
    }

    // Parsanje že generiranega ABB Hidria projekta (glej Template.cs, regija ABB Hidria) nazaj v model.
    // Postaje se prepoznajo po "PROC robot_go<Station>()" v "<Program>_Motion.mod" - edino mesto, kjer
    // je vsaka postaja samostojen blok. Omejitev: podprt je samo en program/robot na projekt (Global.mod,
    // robot_Main.mod in Communication.mod so v generatorju projektno skupne, ne po-robotske, datoteke).
    public static class AbbHidriaProjectImporter
    {
        private static readonly Regex GoProcRegex = new Regex(@"PROC robot_go(\w+)\(\)", RegexOptions.Compiled);

        public static ObservableCollection<ProgramModel> Import(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                throw new AbbHidriaImportException($"Mapa '{folderPath}' ne obstaja.");

            var motionFiles = Directory.GetFiles(folderPath, "*_Motion.mod");
            if (motionFiles.Length == 0)
                throw new AbbHidriaImportException($"V mapi '{folderPath}' ni najdena nobena '_Motion.mod' datoteka.");
            if (motionFiles.Length > 1)
                throw new AbbHidriaImportException(
                    "Uvoz trenutno podpira samo ABB Hidria projekte z enim programom/robotom - Global.mod, " +
                    "robot_Main.mod in Communication.mod so v generatorju projektno skupne datoteke.");

            string motionFile = motionFiles[0];
            string content = File.ReadAllText(motionFile);
            var matches = GoProcRegex.Matches(content);
            if (matches.Count == 0)
                throw new AbbHidriaImportException(
                    $"Datoteka '{Path.GetFileName(motionFile)}' ne vsebuje nobenega 'PROC robot_go...()' - " +
                    "ni v formatu, ki bi ga generiralo to orodje (ABB Hidria).");

            string fileStem = Path.GetFileNameWithoutExtension(motionFile); // "{Prog}_Motion"
            const string suffix = "_Motion";
            string programName = fileStem.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? fileStem.Substring(0, fileStem.Length - suffix.Length)
                : fileStem;
            // ABB shrani ime datoteke kot izvirno ProgramName (brez ToLower/ToUpper) - edini od treh
            // proizvajalcev, kjer se originalna velikost črk imena programa ohrani.

            var program = new ProgramModel(programName, 1);
            program.Stations.Clear();

            foreach (Match m in matches)
            {
                string stationName = m.Groups[1].Value;
                string body = ExtractProcBody(content, m.Index, motionFile);

                bool freeEnabled = body.Contains($"Reset do{stationName}Free;");
                bool pallet = body.Contains($"GetPalletTarget n{stationName},");
                int positions = 1;
                if (body.Contains($"MoveJ{stationName}MaxZHeight"))
                    positions = CountAdditionalPositions(content, stationName);

                program.Stations.Add(new StationModel(stationName, freeEnabled)
                {
                    Pallet = pallet,
                    Positions = positions
                });
            }

            return new ObservableCollection<ProgramModel> { program };
        }

        private static string ExtractProcBody(string content, int startIdx, string filePathForError)
        {
            int endIdx = content.IndexOf("ENDPROC", startIdx, StringComparison.Ordinal);
            if (endIdx < 0)
                throw new AbbHidriaImportException(
                    $"Manjkajoč zaključek 'ENDPROC' v datoteki '{Path.GetFileName(filePathForError)}'.");
            return content.Substring(startIdx, endIdx - startIdx);
        }

        private static int CountAdditionalPositions(string content, string stationName)
        {
            string marker = $"PROC MoveJ{stationName}MaxZHeight";
            int idx = content.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0) return 1;

            int endIdx = content.IndexOf("ENDPROC", idx, StringComparison.Ordinal);
            string block = endIdx > idx ? content.Substring(idx, endIdx - idx) : content.Substring(idx);

            var caseMatches = Regex.Matches(block, @"Case\s+\d+:");
            return caseMatches.Count > 0 ? caseMatches.Count : 1;
        }
    }
}
