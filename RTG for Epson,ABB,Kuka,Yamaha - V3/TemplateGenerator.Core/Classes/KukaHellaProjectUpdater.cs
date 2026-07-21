using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class KukaHellaUpdateException : Exception
    {
        public KukaHellaUpdateException(string message) : base(message) { }
    }

    // Kirurško vstavljanje na koncu dodanih postaj v že generiran, en-robotski KUKA Hella projekt.
    // Predpostavke (glej KukaHellaProjectImporter): en robot na projekt, nove postaje se dodajajo
    // samo na konec seznama, KUKA generator (SimpleProgram) ne podpira Pallet/večpozicijskih postaj.
    public static class KukaHellaProjectUpdater
    {
        public static string BackupFolder(string path)
        {
            string trimmed = path.TrimEnd('\\', '/');
            string backupPath = trimmed + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            CopyDirectory(trimmed, backupPath);
            return backupPath;
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }

        public static void UpdateProgram(ProgramModel program, List<StationModel> newStations, string path)
        {
            if (newStations == null || newStations.Count == 0) return;

            string progLower = program.ProgramNameLowerChar;
            string programDir = Path.Combine(path, "R1", "Program");

            string mainSrcPath = Path.Combine(programDir, $"{progLower}_main.src");
            string motionSrcPath = Path.Combine(programDir, $"{progLower}_motion.src");
            string motionDatPath = Path.Combine(programDir, $"{progLower}_motion.dat");
            string functionsSrcPath = Path.Combine(programDir, "Functions", "robotFunctions.src");
            string configDatPath = Path.Combine(path, "R1", "System", "$config.dat");

            foreach (var p in new[] { mainSrcPath, motionSrcPath, motionDatPath, functionsSrcPath, configDatPath })
                if (!File.Exists(p))
                    throw new KukaHellaUpdateException($"Datoteka '{p}' ne obstaja - ali je bila izbrana pravilna mapa projekta?");

            string mainSrc = File.ReadAllText(mainSrcPath);
            string motionSrc = File.ReadAllText(motionSrcPath);

            int previousCount = program.Stations.Count - newStations.Count;

            for (int idx = 0; idx < newStations.Count; idx++)
            {
                var station = newStations[idx];
                int stationIndex = previousCount + idx;
                var notYetCreated = new HashSet<StationModel>(newStations.Skip(idx));

                mainSrc = PatchHoming(mainSrc, progLower, station);
                mainSrc = PatchMainTask(mainSrc, progLower, station, stationIndex);

                motionSrc = PatchExistingGoFunctionsForNewOrigin(motionSrc, program, station, notYetCreated);
                motionSrc = PatchMovement(motionSrc, program, station);
                motionSrc = PatchMoveOnStation(motionSrc, station);
                motionSrc = PatchMoveAway(motionSrc, station);
            }

            File.WriteAllText(mainSrcPath, mainSrc);
            File.WriteAllText(motionSrcPath, motionSrc);

            UpdateMotionDatFile(motionDatPath, newStations);
            UpdateFunctionsSrcFile(functionsSrcPath, newStations);
            UpdateConfigDatFile(configDatPath, newStations);
        }

        #region ////// Iskanje in urejanje blokov "DEF/GLOBAL DEF ... END" //////

        private static (int start, int end) FindDefBlockRange(string content, string startMarker)
        {
            int startIdx = content.IndexOf(startMarker, StringComparison.Ordinal);
            if (startIdx < 0)
                throw new KukaHellaUpdateException(
                    $"'{startMarker}' ni bilo mogoče najti - datoteka morda ni bila generirana s tem orodjem ali je bila preveč spremenjena.");

            var endMatch = Regex.Match(content.Substring(startIdx), @"\bEND\b");
            if (!endMatch.Success)
                throw new KukaHellaUpdateException($"Manjkajoč zaključek 'END' za '{startMarker}'.");

            return (startIdx, startIdx + endMatch.Index);
        }

        // Če se markerIdx premakne nazaj čez vodilne presledke/tabe do začetka vrstice, vstavek
        // pristane PRED celo izvirno vrstico (ne sredi njenega zamika) - velja tudi če je "insideMarker"
        // podan brez lastnega vodilnega zamika (npr. golo "DEFAULT").
        private static int BackUpToStartOfLine(string text, int idx)
        {
            while (idx > 0 && (text[idx - 1] == ' ' || text[idx - 1] == '\t'))
                idx--;
            return idx;
        }

        private static string InsertBeforeMarkerInBlock(string content, string startMarker, string insideMarker, string insertion)
        {
            var (start, end) = FindDefBlockRange(content, startMarker);
            string block = content.Substring(start, end - start);
            int markerIdx = block.IndexOf(insideMarker, StringComparison.Ordinal);
            if (markerIdx < 0)
                throw new KukaHellaUpdateException(
                    $"V '{startMarker}' ni bilo mogoče najti pričakovanega mesta za vstavljanje ('{insideMarker.Trim()}').");
            markerIdx = BackUpToStartOfLine(block, markerIdx);

            int absoluteIdx = start + markerIdx;
            return content.Substring(0, absoluteIdx) + insertion + content.Substring(absoluteIdx);
        }

        private static string InsertBeforeEndOfBlock(string content, string startMarker, string lineText)
        {
            var (start, end) = FindDefBlockRange(content, startMarker);
            return content.Substring(0, end) + lineText + "\r\n" + content.Substring(end);
        }

        #endregion

        #region ////// {robot}_main.src: DEF {robot}_homing() / DEF {robot}_mainTask() //////

        private static string PatchHoming(string content, string progLower, StationModel station)
        {
            string marker = $"DEF {progLower}_homing()";
            string insertion =
                $"\t\t\tIF Dist(j{station.RobotStationName},$POS_ACT) < ndistance THEN\r\n" +
                $"\t\t\t\tndistance = Dist(j{station.RobotStationName},$POS_ACT)\r\n" +
                $"\t\t\t\tFROM_LOC = #FROM_{station.RobotStationNameToUpper}\r\n" +
                "\t\t\t\tbRobot_onStation = TRUE\r\n" +
                "\t\t\tENDIF\r\n";
            return InsertBeforeMarkerInBlock(content, marker, "\t\tEndIf", insertion);
        }

        private static string PatchMainTask(string content, string progLower, StationModel station, int stationIndex)
        {
            string marker = $"DEF {progLower}_mainTask()";
            string insertion = $"\t\t\t\tCase {stationIndex}\r\n\t\t\t\t\trobot_go{station.RobotStationName}()\r\n";
            return InsertBeforeMarkerInBlock(content, marker, "\t\t\t\tDefault", insertion);
        }

        #endregion

        #region ////// {robot}_motion.src: robot_go{Postaja} / robot_moveOnStation / robot_moveAway //////

        private static string BuildMovementCommand(StationModel targetStation)
        {
            if (targetStation.RobotStationName == "Home")
                return "\t\t\tPTP xHome \r\n";
            return $"\t\t\tPTP frABOVE_STATION_Z_OFFSET : j{targetStation.RobotStationName} C_Ptp\r\n";
        }

        // Glej enako opombo v EpsonProjectUpdater: vsaka robot_go<Postaja>() funkcija ima svoje
        // notranje stikalo "od kod prihajam", ki mora poznati VSE postaje kot možen izvor.
        private static string PatchExistingGoFunctionsForNewOrigin(
            string content, ProgramModel program, StationModel newStation, HashSet<StationModel> notYetCreated)
        {
            foreach (var existingStation in program.Stations)
            {
                if (notYetCreated.Contains(existingStation)) continue;

                string marker = $"GLOBAL DEF robot_go{existingStation.RobotStationName} ()";
                string movementCmd = BuildMovementCommand(existingStation);
                string branch = $"\t\tCASE #FROM_{newStation.RobotStationNameToUpper}\r\n{movementCmd}";
                content = InsertBeforeMarkerInBlock(content, marker, "DEFAULT", branch);
            }
            return content;
        }

        private static string PatchMovement(string content, ProgramModel program, StationModel station)
        {
            var regex = new Regex(@"GLOBAL DEF robot_go(\w+)\s*\(\)");
            var matches = regex.Matches(content);
            if (matches.Count == 0)
                throw new KukaHellaUpdateException("V 'motion.src' ni bilo mogoče najti nobene obstoječe 'robot_go...()' funkcije.");

            var last = matches[matches.Count - 1];
            var endMatch = Regex.Match(content.Substring(last.Index), @"\bEND\b");
            if (!endMatch.Success)
                throw new KukaHellaUpdateException("Manjkajoč zaključek 'END' zadnje 'robot_go...()' funkcije.");

            int insertPos = last.Index + endMatch.Index + "END".Length;
            string newFunctionText = "\r\n\r\n" + BuildGoFunctionText(program, station);
            return content.Substring(0, insertPos) + newFunctionText + content.Substring(insertPos);
        }

        private static string BuildGoFunctionText(ProgramModel program, StationModel station)
        {
            string stationUpper = station.RobotStationNameToUpper;
            string movementCmd = BuildMovementCommand(station);

            var sb = new StringBuilder();
            sb.Append($"GLOBAL DEF robot_go{station.RobotStationName} ()\r\n");
            sb.Append("\t; Set location to which the robot is going\r\n");
            sb.Append($"\tTO_LOC = #TO_{stationUpper}\r\n\r\n");
            sb.Append("\t; Move away\r\n");
            sb.Append("\trobot_moveAway(); \r\n\r\n");
            sb.Append("\t; Set power mode \r\n");
            sb.Append("\trobot_setPowerMode() \r\n\r\n");
            sb.Append("\tSWITCH FROM_LOC \r\n");
            foreach (var s in program.Stations)
            {
                sb.Append($"\t\tCASE #FROM_{s.RobotStationNameToUpper}\r\n");
                sb.Append(movementCmd);
            }
            sb.Append("\tDEFAULT \r\n");
            sb.Append("\t\tmainHalt ()\r\n\r\n");
            sb.Append("\tENDSWITCH\r\n\r\n");
            sb.Append("\t; Go on station\r\n");
            sb.Append("\trobot_moveOnStation()\r\n\r\n");
            sb.Append($"  ; Set the depart location as \"{station.RobotStationName}\" (for the next move -> robot needs to know from where it is starting in the next move) and reset the destination location (depart location is now the destination location)\r\n");
            sb.Append($"\tFROM_LOC = #FROM_{stationUpper}\r\n");
            sb.Append("\tTO_LOC = #TO_NONE\r\n");
            sb.Append("END");
            return sb.ToString();
        }

        private static string PatchMoveOnStation(string content, StationModel station)
        {
            string marker = "GLOBAL DEF robot_moveOnStation()";
            string stationUpper = station.RobotStationNameToUpper;

            var sb = new StringBuilder();
            sb.Append($"\t\tCASE #TO_{stationUpper}\r\n");
            if (station.StationFreeEnabled)
                sb.Append($"\t\t\tTRIGGER WHEN DISTANCE = 0 DELAY = 0 DO do{stationUpper}_FREE = FALSE\r\n");
            sb.Append($"\t\t\tPTP frNEAR_STATION_OFFSET : j{station.RobotStationName} C_Ptp\r\n");
            sb.Append("\t\t\trobot_slowerWorkingMode(nROBOT_SLOW_PTP_SPEED, nROBOT_SLOW_LIN_SPEED, nROBOT_DEPART_ACCEL)\r\n");
            sb.Append("\t\t\tTRIGGER WHEN DISTANCE = 1 DELAY = 0 DO bRobot_onStation = True\r\n");
            sb.Append($"\t\t\tLIN j{station.RobotStationName}\r\n");

            return InsertBeforeMarkerInBlock(content, marker, "DEFAULT", sb.ToString());
        }

        private static string PatchMoveAway(string content, StationModel station)
        {
            string marker = "GLOBAL DEF robot_moveAway()";
            string stationUpper = station.RobotStationNameToUpper;

            var sb = new StringBuilder();
            sb.Append($"\t\tCASE #FROM_{stationUpper}\r\n");
            sb.Append("\t\t\tIF (bROBOT_LOCAL_RESET_FLAG == FALSE) and (bROBOT_LOCAL_HOMING_FLAG == FALSE) THEN\r\n");
            sb.Append("\t\t\t\t; Go slower working mode\r\n");
            sb.Append("\t\t\t\trobot_slowerWorkingMode(nROBOT_SLOW_PTP_SPEED, nROBOT_SLOW_LIN_SPEED, nROBOT_DEPART_ACCEL)\r\n");
            sb.Append($"\t\t\t\tLIN frNEAR_STATION_OFFSET : j{station.RobotStationName} C_Dis \r\n");
            sb.Append("\t\t\tENDIF\r\n");
            sb.Append("\t\t\t; Go full working speed\r\n");
            sb.Append("\t\t\trobot_fullWorkingSpeed()\r\n");
            if (station.StationFreeEnabled)
                sb.Append("\t\t\tTRIGGER WHEN DISTANCE = 1 DELAY = 0 DO robot_stationsFree() PRIO=-1\r\n");
            sb.Append("\t\t\tTRIGGER WHEN DISTANCE = 1 DELAY = 0 DO bRobot_onStation = False\r\n");
            sb.Append($"\t\t\tPTP frABOVE_STATION_Z_OFFSET : j{station.RobotStationName} C_Ptp\r\n");

            return InsertBeforeMarkerInBlock(content, marker, "DEFAULT", sb.ToString());
        }

        #endregion

        #region ////// {robot}_motion.dat: naučene točke - samo dodaj, nikoli ne prepiši //////

        private static void UpdateMotionDatFile(string path, List<StationModel> newStations)
        {
            string content = File.ReadAllText(path);

            var sb = new StringBuilder();
            foreach (var station in newStations)
                sb.Append($"DECL GLOBAL E6POS j{station.RobotStationName}={{X 0,Y 0.0,Z 0,A 0,B 0,C 0,S 0,T 0,E1 0.0,E2 0.0,E3 0.0,E4 0.0,E5 0.0,E6 0.0}}\r\n");

            int endIdx = content.LastIndexOf("ENDDAT", StringComparison.Ordinal);
            if (endIdx < 0)
                throw new KukaHellaUpdateException($"V datoteki '{Path.GetFileName(path)}' ni bilo mogoče najti 'ENDDAT'.");

            content = content.Substring(0, endIdx) + sb + content.Substring(endIdx);
            File.WriteAllText(path, content);
        }

        #endregion

        #region ////// Functions/robotFunctions.src: robot_stationsFree/robot_stationsBusy //////

        private static void UpdateFunctionsSrcFile(string path, List<StationModel> newStations)
        {
            string content = File.ReadAllText(path);

            foreach (var station in newStations)
            {
                if (!station.StationFreeEnabled) continue;
                content = InsertBeforeEndOfBlock(content, "GLOBAL DEF robot_stationsFree()", $"\tdo{station.RobotStationNameToUpper}_FREE = TRUE");
                content = InsertBeforeEndOfBlock(content, "GLOBAL DEF robot_stationsBusy()", $"\tdo{station.RobotStationNameToUpper}_FREE = FALSE");
            }

            File.WriteAllText(path, content);
        }

        #endregion

        #region ////// R1/System/$config.dat: SIGNAL doX_FREE + ENUM seznami //////

        private static void UpdateConfigDatFile(string path, List<StationModel> newStations)
        {
            string content = File.ReadAllText(path);

            var signalMatches = Regex.Matches(content, @"SIGNAL do\w+_FREE \$OUT\[(\d+)\]");
            int nextOutIndex;
            int insertAfterIdx;
            if (signalMatches.Count > 0)
            {
                var last = signalMatches[signalMatches.Count - 1];
                nextOutIndex = int.Parse(last.Groups[1].Value) + 1;
                // Vstavi ZA koncem cele zadnje SIGNAL vrstice (za njenim prelomom), ne takoj za "]",
                // sicer se nova SIGNAL vrstica prilepi na obstoječo (npr. "...$OUT[33]SIGNAL...$OUT[34]").
                int lineEnd = content.IndexOf('\n', last.Index + last.Length);
                insertAfterIdx = lineEnd >= 0 ? lineEnd + 1 : content.Length;
            }
            else
            {
                int marker = content.IndexOf("; Station free bits", StringComparison.Ordinal);
                if (marker < 0)
                    throw new KukaHellaUpdateException($"V '{Path.GetFileName(path)}' ni bilo mogoče najti '; Station free bits'.");
                int lineEnd = content.IndexOf('\n', marker);
                insertAfterIdx = lineEnd >= 0 ? lineEnd + 1 : marker;
                nextOutIndex = 33;
            }

            var newSignals = new StringBuilder();
            foreach (var station in newStations)
            {
                if (!station.StationFreeEnabled) continue;
                newSignals.Append($"SIGNAL do{station.RobotStationNameToUpper}_FREE $OUT[{nextOutIndex}]\r\n");
                nextOutIndex++;
            }
            content = content.Substring(0, insertAfterIdx) + newSignals + content.Substring(insertAfterIdx);

            foreach (var station in newStations)
            {
                content = AppendToEnumLine(content, "ENUM ROBOT_FROM_LOC FROM_NONE", $", FROM_{station.RobotStationNameToUpper}", path);
                content = AppendToEnumLine(content, "ENUM ROBOT_TO_LOC TO_NONE", $", TO_{station.RobotStationNameToUpper}", path);
            }

            File.WriteAllText(path, content);
        }

        private static string AppendToEnumLine(string content, string marker, string suffix, string path)
        {
            int idx = content.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
                throw new KukaHellaUpdateException($"'{marker}' ni bilo mogoče najti v '{Path.GetFileName(path)}'.");

            int lineEnd = content.IndexOfAny(new[] { '\r', '\n' }, idx);
            if (lineEnd < 0) lineEnd = content.Length;
            return content.Substring(0, lineEnd) + suffix + content.Substring(lineEnd);
        }

        #endregion
    }
}
