using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                robotIndex++;
                result.Add(ImportProgram(file, robotIndex));
            }
            return result;
        }

        private static ProgramModel ImportProgram(string filePath, int fallbackRobotNumber)
        {
            string content = File.ReadAllText(filePath);
            var matches = GoFunctionRegex.Matches(content);
            if (matches.Count == 0)
                throw new EpsonImportException(
                    $"Datoteka '{Path.GetFileName(filePath)}' ne vsebuje nobene 'Function ..._go...()' funkcije - " +
                    "ni v formatu, ki bi ga generiralo to orodje (Epson Hidria).");

            string programNameLower = matches[0].Groups[1].Value;
            // Opomba: izvirna mešana velikost črk imena programa se ne da natančno obnoviti,
            // ker se v generirani kodi pojavljata samo ToLower()/ToUpper() varianta.
            string reconstructedProgramName = char.ToUpper(programNameLower[0]) + programNameLower.Substring(1);

            var robotNumberMatch = RobotNumberRegex.Match(content);
            int robotNumber = robotNumberMatch.Success ? int.Parse(robotNumberMatch.Groups[1].Value) : fallbackRobotNumber;

            var program = new ProgramModel(reconstructedProgramName, robotNumber);
            program.Stations.Clear();

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
