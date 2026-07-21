using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class AbbHidriaUpdateException : Exception
    {
        public AbbHidriaUpdateException(string message) : base(message) { }
    }

    // Kirurško vstavljanje na koncu dodanih postaj v že generiran, en-programski ABB Hidria projekt.
    // Predpostavke (glej AbbHidriaProjectImporter): en program/robot na projekt, nove postaje se
    // dodajajo samo na konec seznama. Namerno IZVEN obsega (ostane nespremenjeno ob "Update"):
    // OtherFunctions.mod (iskanje najbližje točke ob homingu) in EIORobot/EIOSimulacija.cfg -
    // za popolno osvežitev teh dveh je potreben poln "Generate".
    public static class AbbHidriaProjectUpdater
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

            string globalPath = Path.Combine(path, "Global.mod");
            string motionPath = Path.Combine(path, $"{program.ProgramName}_Motion.mod");
            string mainPath = Path.Combine(path, "robot_Main.mod");
            string communicationPath = Path.Combine(path, "Communication.mod");

            foreach (var p in new[] { globalPath, motionPath, mainPath, communicationPath })
                if (!File.Exists(p))
                    throw new AbbHidriaUpdateException($"Datoteka '{p}' ne obstaja - ali je bila izbrana pravilna mapa projekta?");

            UpdateGlobalMod(globalPath, program, newStations);

            string motionContent = File.ReadAllText(motionPath);
            string mainContent = File.ReadAllText(mainPath);
            string commContent = File.ReadAllText(communicationPath);

            int previousCount = program.Stations.Count - newStations.Count;

            for (int idx = 0; idx < newStations.Count; idx++)
            {
                var station = newStations[idx];
                int stationIndex = previousCount + idx;
                var notYetCreated = new HashSet<StationModel>(newStations.Skip(idx));

                motionContent = PatchExistingGoProcsForNewOrigin(motionContent, program, station, notYetCreated);
                motionContent = PatchMovement(motionContent, program, station);
                motionContent = PatchMoveOnStation(motionContent, program, station);
                motionContent = PatchMoveAway(motionContent, program, station);
                if (station.Positions != 1)
                    motionContent = PatchAdditionalPositions(motionContent, station);

                mainContent = PatchMainDispatch(mainContent, program, station, stationIndex);
                if (station.Pallet)
                    mainContent = PatchCreatePallet(mainContent, station);

                commContent = PatchProc(commContent, "PROC robot_stationsFree()",
                    station.StationFreeEnabled, $"\t\tSet do{station.RobotStationName}Free;");
                commContent = PatchProc(commContent, "PROC robot_stationBusy()",
                    station.StationFreeEnabled, $"\t\tReset do{station.RobotStationName}Free;");
                commContent = PatchProc(commContent, "PROC robot_resetDepartLocations()",
                    true, $"\t\tbRobotFrom{station.RobotStationName} := FALSE;");
                commContent = PatchProc(commContent, "PROC robot_resetDestLocations()",
                    true, $"\t\tbRobotTo{station.RobotStationName} := FALSE;");
            }

            File.WriteAllText(motionPath, motionContent);
            File.WriteAllText(mainPath, mainContent);
            File.WriteAllText(communicationPath, commContent);
        }

        #region ////// Iskanje in urejanje blokov "PROC ... ENDPROC" //////

        private static (int start, int end) FindProcBlockRange(string content, string startMarker)
        {
            int startIdx = content.IndexOf(startMarker, StringComparison.Ordinal);
            if (startIdx < 0)
                throw new AbbHidriaUpdateException(
                    $"'{startMarker}' ni bilo mogoče najti - datoteka morda ni bila generirana s tem orodjem ali je bila preveč spremenjena.");

            int endIdx = content.IndexOf("ENDPROC", startIdx, StringComparison.Ordinal);
            if (endIdx < 0)
                throw new AbbHidriaUpdateException($"Manjkajoč zaključek 'ENDPROC' za '{startMarker}'.");

            return (startIdx, endIdx);
        }

        // Če se markerIdx premakne nazaj čez vodilne presledke/tabe do začetka vrstice, vstavek
        // pristane PRED celo izvirno vrstico (ne sredi njenega zamika).
        private static int BackUpToStartOfLine(string text, int idx)
        {
            while (idx > 0 && (text[idx - 1] == ' ' || text[idx - 1] == '\t'))
                idx--;
            return idx;
        }

        private static string InsertBeforeMarkerInBlock(string content, string startMarker, string insideMarker, string insertion)
        {
            var (start, end) = FindProcBlockRange(content, startMarker);
            string block = content.Substring(start, end - start);
            int markerIdx = block.IndexOf(insideMarker, StringComparison.Ordinal);
            if (markerIdx < 0)
                throw new AbbHidriaUpdateException(
                    $"V '{startMarker}' ni bilo mogoče najti pričakovanega mesta za vstavljanje ('{insideMarker}').");
            markerIdx = BackUpToStartOfLine(block, markerIdx);

            int absoluteIdx = start + markerIdx;
            return content.Substring(0, absoluteIdx) + insertion + content.Substring(absoluteIdx);
        }

        // Glej enako opombo v EpsonProjectUpdater: vstavek gre takoj za prelomom vrstice, da cela
        // izvirna "ELSE" vrstica ostane nedotaknjena.
        private static readonly Regex StandaloneElseLineRegex = new Regex(@"\n[ \t]*ELSE(?!IF)", RegexOptions.Compiled);

        private static string InsertBeforeStandaloneElse(string content, string startMarker, string insertion)
        {
            var (start, end) = FindProcBlockRange(content, startMarker);
            string block = content.Substring(start, end - start);
            var m = StandaloneElseLineRegex.Match(block);
            if (!m.Success)
                throw new AbbHidriaUpdateException($"V '{startMarker}' ni bilo mogoče najti zaključnega 'ELSE' stavka.");

            int absoluteIdx = start + m.Index + 1;
            return content.Substring(0, absoluteIdx) + insertion + content.Substring(absoluteIdx);
        }

        private static string PatchProc(string content, string startMarker, bool shouldAdd, string lineText)
        {
            if (!shouldAdd) return content;
            var (start, end) = FindProcBlockRange(content, startMarker);
            // "ENDPROC" je v predlogi (Communication.txt) na isti izvorni vrstici kot {N} placeholder,
            // z vodilnimi presledki, ki pripadajo NJEGOVI lastni izrisani vrstici (nastane šele iz
            // {N}-jevega lastnega zaključnega prelom vrstice) - brez BackUpToStartOfLine bi vstavek
            // pristal sredi teh presledkov in "ENDPROC" ostal brez lastnega zamika.
            string block = content.Substring(start, end - start);
            int endIdx = BackUpToStartOfLine(block, end - start);
            int absoluteEndIdx = start + endIdx;
            return content.Substring(0, absoluteEndIdx) + lineText + "\r\n" + content.Substring(absoluteEndIdx);
        }

        #endregion

        #region ////// Global.mod: bool zastavice + naučene točke - samo dodaj, nikoli ne prepiši //////

        // Opomba: Template.cs (GetABBHidriaGlobalFunc, veja za en program) uporabi za PALLET postaje
        // ime točke brez predpone programa ("jStation1"), za privzeti/večpozicijski primer pa (zaradi
        // obstoječe nekonsistentnosti v generatorju) ime VKLJUČNO s predpono programa ("jRobot1_Station1").
        // Tu repliciramo to natančno vedenje, da se novo dodane točke poimenujejo enako, kot bi jih
        // poimenoval sam generator, in se ujemajo z že obstoječimi deklaracijami v datoteki.
        private static void UpdateGlobalMod(string path, ProgramModel program, List<StationModel> newStations)
        {
            string content = File.ReadAllText(path);
            int existingPalletCount = Regex.Matches(content, @"VAR num n\w+ :=").Count;

            foreach (var station in newStations)
            {
                content = InsertAfterLastLineMatching(content, @"VAR bool bRobotFrom\w+ ;",
                    $"\tVAR bool bRobotFrom{station.RobotStationName} ;\r\n");
                content = InsertAfterLastLineMatching(content, @"VAR bool bRobotTo\w+ ;",
                    $"\tVAR bool bRobotTo{station.RobotStationName} ;\r\n");

                if (station.Pallet)
                {
                    existingPalletCount++;
                    content = InsertAfterLastLineMatching(content, @"VAR num n\w+ :=\s*\d+;",
                        $"\tVAR num n{station.RobotStationName} :=  {existingPalletCount};\r\n");
                    content = InsertAfterLastLineMatching(content, @"CONST robtarget j\w+ :=[^\r\n]*",
                        BuildRobtargetLine($"{station.RobotStationName}1"));
                    content = InsertAfterLastLineMatching(content, @"CONST robtarget j\w+ :=[^\r\n]*",
                        BuildRobtargetLine($"{station.RobotStationName}2"));
                    content = InsertAfterLastLineMatching(content, @"CONST robtarget j\w+ :=[^\r\n]*",
                        BuildRobtargetLine($"{station.RobotStationName}3"));
                }
                else if (station.Positions != 1)
                {
                    for (int j = 1; j <= station.Positions; j++)
                        content = InsertAfterLastLineMatching(content, @"CONST robtarget j\w+ :=[^\r\n]*",
                            BuildRobtargetLine($"{program.ProgramName}_{station.RobotStationName}{j}"));
                }
                else
                {
                    content = InsertAfterLastLineMatching(content, @"CONST robtarget j\w+ :=[^\r\n]*",
                        BuildRobtargetLine($"{program.ProgramName}_{station.RobotStationName}"));
                }
            }

            File.WriteAllText(path, content);
        }

        private static string BuildRobtargetLine(string pointName)
        {
            return $"\tCONST robtarget j{pointName} :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\r\n";
        }

        private static string InsertAfterLastLineMatching(string content, string pattern, string newLine)
        {
            var matches = Regex.Matches(content, pattern);
            if (matches.Count == 0)
                throw new AbbHidriaUpdateException($"V 'Global.mod' ni bilo mogoče najti vrstice, ki ustreza '{pattern}'.");

            var last = matches[matches.Count - 1];
            int lineEnd = content.IndexOf('\n', last.Index + last.Length);
            lineEnd = lineEnd >= 0 ? lineEnd + 1 : content.Length;
            return content.Substring(0, lineEnd) + newLine + content.Substring(lineEnd);
        }

        #endregion

        #region ////// {Prog}_Motion.mod: robot_go{Postaja} / {Prog}_moveOnStation / {Prog}_moveAway //////

        // Gibanje do TARGET postaje - odvisno od tega, ali je ta postaja prva v seznamu (Home,
        // preprost enovrstičen premik) ali ne (Pallet/večpozicijska/privzeta varianta).
        private static string BuildMovementCommand(ProgramModel program, StationModel targetStation)
        {
            int index = program.Stations.IndexOf(targetStation);
            string s = targetStation.RobotStationName;

            if (index == 0)
                return $"\t\t\tMoveJ j{s},vFullSpeed,z100,tool0; \r\n";

            if (targetStation.Pallet)
                return "\t\t\tIF diRobotOnStation = 1 THEN\r\n" +
                       "\t\t\t\tMoveJ Offs (jPallett,0,0,nRobotStationZOffset),vFullSpeed,z100,tool0;\r\n" +
                       "\t\t\tELSE\r\n" +
                       "\t\t\t\tMoveJ Offs (jPallett,0,0,nRobotStationZOffset),vFullSpeed,fine,tool0;\r\n" +
                       "\t\t\tENDIF\r\n";

            if (targetStation.Positions != 1)
                return $"\t\t\tMoveJ{s}MaxZHeight nAdditionalPos;\r\n";

            return "\t\t\tIF diRobotOnStation = 1 THEN\r\n" +
                   $"\t\t\t\tMoveJ Offs (j{s},0,0,nRobotStationZOffset),vFullSpeed,z100,tool0;\r\n" +
                   "\t\t\tELSE\r\n" +
                   $"\t\t\t\tMoveJ Offs (j{s},0,0,nRobotStationZOffset),vFullSpeed,fine,tool0;\r\n" +
                   "\t\t\tENDIF\r\n";
        }

        // Glej enako opombo v EpsonProjectUpdater/KukaHellaProjectUpdater: vsaka robot_go<Postaja>()
        // funkcija ima svojo notranjo verigo "od kod prihajam", ki mora poznati VSE postaje kot izvor.
        private static string PatchExistingGoProcsForNewOrigin(
            string content, ProgramModel program, StationModel newStation, HashSet<StationModel> notYetCreated)
        {
            foreach (var existingStation in program.Stations)
            {
                if (notYetCreated.Contains(existingStation)) continue;

                string marker = $"PROC robot_go{existingStation.RobotStationName}()";
                string movementCmd = BuildMovementCommand(program, existingStation);
                string branch = $"\t\tELSEIF bFrom{newStation.RobotStationName} = TRUE THEN \r\n{movementCmd}\r\n";
                content = InsertBeforeStandaloneElse(content, marker, branch);
            }
            return content;
        }

        private static string PatchMovement(string content, ProgramModel program, StationModel station)
        {
            var regex = new Regex(@"PROC robot_go(\w+)\(\)");
            var matches = regex.Matches(content);
            if (matches.Count == 0)
                throw new AbbHidriaUpdateException("V 'Motion.mod' ni bilo mogoče najti nobenega obstoječega 'robot_go...()'.");

            var last = matches[matches.Count - 1];
            int endIdx = content.IndexOf("ENDPROC", last.Index, StringComparison.Ordinal);
            if (endIdx < 0)
                throw new AbbHidriaUpdateException("Manjkajoč zaključek 'ENDPROC' zadnjega 'robot_go...()'.");

            int insertPos = endIdx + "ENDPROC".Length;
            string newProcText = "\r\n\r\n" + BuildGoProcText(program, station);
            return content.Substring(0, insertPos) + newProcText + content.Substring(insertPos);
        }

        private static string BuildGoProcText(ProgramModel program, StationModel station)
        {
            string s = station.RobotStationName;
            string movementCmd = BuildMovementCommand(program, station);

            var sb = new StringBuilder();
            sb.Append($"\tPROC robot_go{s}()\r\n");
            sb.Append("\t\t! Set location to which the robot is going\r\n");
            sb.Append($"\t\tbRobotTo{s} := TRUE;\r\n\r\n");
            sb.Append("\t\t! Move away\r\n");
            sb.Append($"\t\t{program.ProgramName}_moveAway;\r\n\r\n");
            if (station.Pallet)
                sb.Append($"\t\tGetPalletTarget n{s},nAdditionalPos;\r\n\r\n");
            sb.Append("\t\t! Set Power mode\r\n");
            sb.Append("\t\trobot_setPowerMode;\r\n\r\n");
            if (station.StationFreeEnabled)
                sb.Append($"\t\tIF diRobotOnStation = 1 THEN\r\n\t\t\tReset do{s}Free;\r\n\t\tENDIF\r\n\r\n");

            foreach (var s2 in program.Stations)
            {
                if (s2.RobotStationName == "Home")
                    sb.Append($"\tIf bFrom{s2.RobotStationName} = TRUE THEN\r\n{movementCmd}\r\n");
                else
                    sb.Append($"\t\tELSEIF bFrom{s2.RobotStationName} = TRUE THEN\r\n{movementCmd}\r\n");
            }

            sb.Append("\t\tELSE\r\n");
            sb.Append($"            error_ProgramError(\"Function robot_go{s}()\");\r\n\r\n");
            sb.Append("\t\tENDIF\r\n\r\n");
            sb.Append("\t\t! Go on station\r\n");
            sb.Append($"\t\t{program.ProgramName}_moveOnStation;\r\n\r\n");
            sb.Append("\t\t! Reset depart locations\r\n");
            sb.Append("\t\trobot_resetDepartLocations;\r\n\r\n");
            sb.Append($"\t\t! Set the depart location as {s} (for the next move -> robot needs to know from where it is starting in the next move) and reset the destination location (depart location is now the destination location)\r\n");
            sb.Append($"\t\tbRobotFrom{s}:= TRUE;\r\n");
            sb.Append($"\t\tbRobotTo{s}:= FALSE;\r\n");
            sb.Append("\tENDPROC");
            return sb.ToString();
        }

        private static string PatchMoveOnStation(string content, ProgramModel program, StationModel station)
        {
            string marker = $"PROC {program.ProgramName}_moveOnStation()";
            string s = station.RobotStationName;

            var sb = new StringBuilder();
            sb.Append($"\t\t\tELSEIF bRobotTo{s} = TRUE THEN \r\n");
            if (station.StationFreeEnabled)
                sb.Append($"\t\t\t\tReset do{s}Free;\r\n");

            if (station.Pallet)
                sb.Append("\t\t\t\tMoveJ Offs(jPallett,0,0,nRobotStationZOffset),vFullSpeed, z50, Tool0; \r\n" +
                           "\t\t\t\tMoveL jPallett, vMoveOnStation, fine, Tool0; \r\n\r\n");
            else if (station.Positions != 1)
                sb.Append($"\t\t\t\tMoveJ{s}NearStation nAdditionalPos;\r\n\t\t\t\tMoveL{s} nAdditionalPos;\r\n\r\n");
            else
                sb.Append($"\t\t\t\tMoveJ Offs(j{s},0,0,nRobotStationZOffset),vFullSpeed, z50, Tool0; \r\n" +
                          $"\t\t\t\tMoveL j{s}, vMoveOnStation, fine, Tool0; \r\n\r\n");

            return InsertBeforeStandaloneElse(content, marker, sb.ToString());
        }

        private static string PatchMoveAway(string content, ProgramModel program, StationModel station)
        {
            string marker = $"PROC {program.ProgramName}_moveAway()";
            string s = station.RobotStationName;

            var sb = new StringBuilder();
            sb.Append($"\t\t\tELSEIF bRobotFrom{s} = TRUE THEN \r\n");
            sb.Append("\t\t\t\tAccSet 30,30\r\n");
            if (station.StationFreeEnabled)
            {
                sb.Append("\t\t\t\tIF nPosition = 99 THEN\r\n");
                sb.Append($"\t\t\t\t\tMoveLDo Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,fine,tool0,do{s}Free,1;\r\n");
                sb.Append("\t\t\t\tELSE\r\n");
                sb.Append($"\t\t\t\t\tMoveLDo Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,z50,tool0,do{s}Free,1;\r\n");
                sb.Append("\t\t\t\tENDIF\r\n\r\n");
            }
            else
            {
                sb.Append("\t\t\t\tIF nPosition = 99 THEN\r\n");
                sb.Append("\t\t\t\t\tMoveL Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,fine,tool0;\r\n");
                sb.Append("\t\t\t\tELSE\r\n");
                sb.Append("\t\t\t\t\tMoveL Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,z50,tool0;\r\n");
                sb.Append("\t\t\t\tENDIF\r\n\r\n");
            }

            return InsertBeforeStandaloneElse(content, marker, sb.ToString());
        }

        private static string PatchAdditionalPositions(string content, StationModel station)
        {
            string s = station.RobotStationName;
            var sb = new StringBuilder();

            sb.Append($"\tPROC MoveJ{s}MaxZHeight(num nAddPos)\r\n\t\tTEST nAddPos\r\n");
            for (int j = 1; j <= station.Positions; j++)
                sb.Append($"\t\tCase {j}:\r\n\t\t\tIF diRobotOnStation = 1 THEN\r\n" +
                          $"\t\t\t\tMoveJ Offs (j{s}{j},0,0,nRobotStationZOffset),vFullSpeed,z100,tool0;\r\n" +
                          "\t\t\tELSE\r\n" +
                          $"\t\t\t\tMoveJ Offs (j{s}{j},0,0,nRobotStationZOffset),vFullSpeed,fine,tool0;\r\n" +
                          "\t\t\tENDIF\r\n\r\n");
            sb.Append("\t\tDEFAULT:\r\n");
            sb.Append($"            error_WrongAdditionalNumber(\"Function MoveJ{s}MaxZHeight \",);\r\n");
            sb.Append("\t\tENDTEST\r\n\tENDPROC\r\n\r\n");

            sb.Append($"\tPROC MoveJ{s}NearStation(num nAddPos)\r\n\t\tTEST nAddPos\r\n");
            for (int j = 1; j <= station.Positions; j++)
                sb.Append($"\t\tCase {j}:\r\n\t\t\tMoveJ Offs (j{s}{j},0,0,nRobotStationSlowZOffset),vFullSpeed,z50,tool0;\r\n\r\n");
            sb.Append("\t\tDEFAULT:\r\n");
            sb.Append($"            error_WrongAdditionalNumber(\"Function MoveJ{s}NearStation \",);\r\n");
            sb.Append("\t\tENDTEST\r\n\tENDPROC\r\n\r\n");

            sb.Append($"\tPROC MoveL{s}(num nAddPos)\r\n\t\tTEST nAddPos\r\n");
            for (int j = 1; j <= station.Positions; j++)
                sb.Append($"\t\tCase {j}:\r\n\t\t\tMoveL j{s}{j},vMoveOnStation,fine,tool0;\r\n\r\n");
            sb.Append("\t\tDEFAULT:\r\n");
            sb.Append($"            error_WrongAdditionalNumber(\"Function MoveL{s} \",);\r\n");
            sb.Append("\t\tENDTEST\r\n\tENDPROC\r\n\r\n");

            int endModuleIdx = content.LastIndexOf("ENDMODULE", StringComparison.Ordinal);
            if (endModuleIdx < 0)
                throw new AbbHidriaUpdateException("V 'Motion.mod' ni bilo mogoče najti 'ENDMODULE'.");
            return content.Substring(0, endModuleIdx) + sb + content.Substring(endModuleIdx);
        }

        #endregion

        #region ////// robot_Main.mod: dispatch (TEST nPosition) + CreatePallet //////

        private static string PatchMainDispatch(string content, ProgramModel program, StationModel station, int stationIndex)
        {
            string marker = "PROC robot_mainTask()";
            string insertion = $"\t\t\t\t\tCase {stationIndex}:\r\n\t\t\t\t\t\t{program.ProgramName}_go{station.RobotStationName};\r\n\r\n";
            return InsertBeforeMarkerInBlock(content, marker, "DEFAULT:", insertion);
        }

        private static string PatchCreatePallet(string content, StationModel station)
        {
            string marker = "PROC robot_init()";
            string s = station.RobotStationName;
            string insertion = $"\t\tCreatePallet n{s} , j{s}1, j{s}2, j{s}3 , 0 , 0;\r\n";
            return InsertBeforeMarkerInBlock(content, marker, "! Start homing", insertion);
        }

        #endregion
    }
}
