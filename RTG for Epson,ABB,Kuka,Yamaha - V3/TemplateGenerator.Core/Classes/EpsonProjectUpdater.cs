using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class EpsonUpdateException : Exception
    {
        public EpsonUpdateException(string message) : base(message) { }
    }

    // Kirurško vstavljanje na koncu dodanih postaj v že generiran Epson Hidria projekt -
    // v nasprotju z GenerateProject (ki vse datoteke prepiše na novo) ta razred v <program>.prg
    // spremeni samo dele, ki se nanašajo na na novo dodane postaje, preostalo kodo (vključno z
    // morebitnimi ročnimi popravki) pusti nedotaknjeno. Predpostavka: nove postaje so dodane
    // samo na KONEC seznama postaj programa.
    public static class EpsonProjectUpdater
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

        // newStations: postaje, ki so bile dodane na konec program.Stations po zadnjem uvozu/posodobitvi.
        public static void UpdateProgram(ProgramModel program, List<StationModel> newStations, string path)
        {
            if (newStations == null || newStations.Count == 0) return;

            string prgPath = Path.Combine(path, $"{program.ProgramNameLowerChar}.prg");
            if (!File.Exists(prgPath))
                throw new EpsonUpdateException($"Datoteka '{prgPath}' ne obstaja - ali je bila izbrana pravilna mapa projekta?");

            string content = File.ReadAllText(prgPath);
            int previousCount = program.Stations.Count - newStations.Count;

            for (int idx = 0; idx < newStations.Count; idx++)
            {
                var station = newStations[idx];
                int stationIndex = previousCount + idx;
                // Postaje, ki jih ta ali kasnejša iteracija še ni ustvarila - njihovih go-funkcij
                // še ni v datoteki, zato jih PatchExistingGoFunctionsForNewOrigin ne sme poskusiti popraviti.
                var notYetCreated = new HashSet<StationModel>(newStations.Skip(idx));

                content = PatchHoming(content, program, station, stationIndex);
                content = PatchOperation(content, program, station, stationIndex);
                content = PatchExistingGoFunctionsForNewOrigin(content, program, station, notYetCreated);
                content = PatchMovement(content, program, station);
                content = PatchMoveOnStation(content, program, station);
                content = PatchMoveAway(content, program, station);
                content = PatchLineListFunction(content, $"{program.ProgramNameLowerChar}_stationsFree",
                    station.StationFreeEnabled, $"\tOn {program.ProgramNameUpperChar}_O_{station.RobotStationNameToUpper}_FREE");
                content = PatchLineListFunction(content, $"{program.ProgramNameLowerChar}_stationsBusy",
                    station.StationFreeEnabled, $"\tOff {program.ProgramNameUpperChar}_O_{station.RobotStationNameToUpper}_FREE");
                content = PatchLineListFunction(content, $"{program.ProgramNameLowerChar}_resetDepartLocations",
                    true, $"\tMemOff {program.ProgramNameUpperChar}_FROM_{station.RobotStationNameToUpper}");
                content = PatchLineListFunction(content, $"{program.ProgramNameLowerChar}_resetDestLocations",
                    true, $"\tMemOff {program.ProgramNameUpperChar}_TO_{station.RobotStationNameToUpper}");

                if (station.Positions != 1)
                    content = PatchAdditionalPositions(content, program, station);
            }

            File.WriteAllText(prgPath, content);

            // IOLabels.dat/.io: šteti bitni/bajtni odmiki so determinirana funkcija VRSTNEGA
            // REDA postaj - dodajanje na konec ne spremeni odmikov obstoječih postaj, zato je
            // varno (in preprosteje) ti dve datoteki v celoti na novo generirati.
            File.WriteAllText(Path.Combine(path, $"{program.ProgramName}.io"), Template.GetAllIOLabels(program));
            File.WriteAllText(Path.Combine(path, "IOLabels.dat"), Template.GetIOLablesFunc(program));

            // .pts NE gre v celoti regenerirati: GeneratePointsFunc vedno zapiše koordinate kot 0
            // (dejanske koordinate se na robota "naučijo" šele po prvotnem generiranju), zato bi
            // poln prepis zbrisal že naučene točke. Namesto tega se obstoječe točke ohranijo, nova
            // primarna točka pa se vstavi na PRAVO mesto (glej UpdatePointsFile).
            UpdatePointsFile(Path.Combine(path, $"{program.ProgramName}1.pts"), previousCount, newStations);

            new GenerateExcelIO().GenerateIO("Epson", program, path);
        }

        #region ////// .pts - ohrani obstoječe (naučene) točke, vstavi novo primarno na PRAVO mesto //////

        // GeneratePointsFunc razporedi točke v dveh prehodih: najprej po ena "primarna" točka na
        // postajo, na mestu = njen zaporedni indeks v Stations (0..N-1) - ker Homing najbližjo točko
        // išče PO TEM ŠTEVILČNEM INDEKSU (P(index)), ne po imenu. Šele NATO (na indeksih N, N+1, ...)
        // pridejo morebitne dodatne pallet/večpozicijske točke. Če te dodatne točke že obstajajo (za
        // katero od PREJ obstoječih postaj), zavzemajo prav ta prostor, kamor mora priti primarna
        // točka nove postaje - zato jih je treba za +1 (na novo dodano postajo) prenumerirati navzgor.
        private static readonly Regex NumberOfPointsRegex = new Regex(@"nNumberOfPoints=(\d+)", RegexOptions.Compiled);

        private static readonly Regex PointBlockRegex = new Regex(
            @"Point\d+\s*\{\r?\n\tnNumber=(?<num>\d+)\r?\n(?<rest>[\s\S]*?\r?\n\})",
            RegexOptions.Compiled);

        private static void UpdatePointsFile(string ptsPath, int previousStationCount, List<StationModel> newStations)
        {
            if (!File.Exists(ptsPath))
                throw new EpsonUpdateException($"Datoteka '{ptsPath}' ne obstaja - ali je bila izbrana pravilna mapa projekta?");

            string content = File.ReadAllText(ptsPath);

            var headerMatch = NumberOfPointsRegex.Match(content);
            if (!headerMatch.Success)
                throw new EpsonUpdateException(
                    $"V datoteki '{Path.GetFileName(ptsPath)}' ni bilo mogoče najti 'nNumberOfPoints' - datoteka ni v pričakovanem formatu.");

            var matches = PointBlockRegex.Matches(content);
            if (matches.Count == 0)
                throw new EpsonUpdateException($"V datoteki '{Path.GetFileName(ptsPath)}' ni bilo mogoče razčleniti nobene 'PointN' postavke.");

            int existingTotal = int.Parse(headerMatch.Groups[1].Value);
            if (matches.Count != existingTotal)
                throw new EpsonUpdateException(
                    $"Neskladje v '{Path.GetFileName(ptsPath)}': glava navaja {existingTotal} točk, najdenih pa je bilo {matches.Count}.");

            var existingByNumber = new SortedDictionary<int, string>();
            foreach (Match m in matches)
                existingByNumber[int.Parse(m.Groups["num"].Value)] = m.Groups["rest"].Value;

            int headStart = matches[0].Index;
            var last = matches[matches.Count - 1];
            int tailEnd = last.Index + last.Length;

            // Prenumeriraj obstoječe "surplus" (pallet/večpozicijske) točke navzgor, da naredimo
            // prostor za primarne točke novo dodanih postaj.
            int shift = newStations.Count;
            var rebuilt = new SortedDictionary<int, string>();
            foreach (var kv in existingByNumber)
            {
                int newNum = kv.Key >= previousStationCount ? kv.Key + shift : kv.Key;
                rebuilt[newNum] = kv.Value;
            }

            int nextPrimary = previousStationCount;
            foreach (var station in newStations)
            {
                rebuilt[nextPrimary] = BuildPointRestBody(PrimaryPointName(station));
                nextPrimary++;
            }

            int nextExtra = rebuilt.Keys.Max() + 1;
            foreach (var station in newStations)
                foreach (var extraName in ExtraPointNamesForStation(station))
                {
                    rebuilt[nextExtra] = BuildPointRestBody(extraName);
                    nextExtra++;
                }

            var sb = new StringBuilder();
            bool first = true;
            foreach (var kv in rebuilt)
            {
                if (!first) sb.Append("\n");
                first = false;
                sb.Append("Point").Append(kv.Key + 1).Append(" {\r\n");
                sb.Append("\tnNumber=").Append(kv.Key).Append("\r\n");
                sb.Append(kv.Value);
            }

            string newContent = content.Substring(0, headStart) + sb + content.Substring(tailEnd);
            newContent = NumberOfPointsRegex.Replace(newContent, $"nNumberOfPoints={rebuilt.Count}");

            File.WriteAllText(ptsPath, newContent);
        }

        private static string PrimaryPointName(StationModel station)
        {
            if (station.Pallet) return $"{station.RobotStationName}P1";
            if (station.Positions != 1) return $"{station.RobotStationName}1";
            return station.RobotStationName;
        }

        private static IEnumerable<string> ExtraPointNamesForStation(StationModel station)
        {
            if (station.Pallet)
            {
                yield return $"{station.RobotStationName}P2";
                yield return $"{station.RobotStationName}P3";
                yield return $"{station.RobotStationName}P4";
            }
            else if (station.Positions != 1)
            {
                for (int k = 2; k <= station.Positions; k++)
                    yield return $"{station.RobotStationName}{k}";
            }
        }

        private static string BuildPointRestBody(string pointName)
        {
            var sb = new StringBuilder();
            sb.Append("\tsLabel=\"p").Append(pointName).Append("\"\r\n");
            sb.Append("\tsDescription=\"\"\r\n");
            sb.Append("\tnUndefined=448\r\n");
            sb.Append("\trX=0\r\n");
            sb.Append("\trY=0\r\n");
            sb.Append("\trZ=0\r\n");
            sb.Append("\trU=0\r\n");
            sb.Append("\trV=0\r\n");
            sb.Append("\trW=0\r\n");
            sb.Append("\trR=0\r\n");
            sb.Append("\trS=0\r\n");
            sb.Append("\trT=0\r\n");
            sb.Append("\tnLocal=0\r\n");
            sb.Append("\tnHand=1\r\n");
            sb.Append("\tnElbow=1\r\n");
            sb.Append("\tnWrist=1\r\n");
            sb.Append("\tnJ1Flag=0\r\n");
            sb.Append("\tnJ2Flag=0\r\n");
            sb.Append("\tnJ4Flag=0\r\n");
            sb.Append("\tnJ6Flag=0\r\n");
            sb.Append("\trJ1Angle=0\r\n");
            sb.Append("\trJ4Angle=0\r\n");
            sb.Append("\tbSimVisible=False\r\n");
            sb.Append("}");
            return sb.ToString();
        }

        #endregion

        #region ////// Iskanje in urejanje blokov "Function ... Fend" //////

        private static (int start, int end) FindFunctionBlockRange(string content, string functionMarker)
        {
            int startIdx = content.IndexOf(functionMarker, StringComparison.Ordinal);
            if (startIdx < 0)
                throw new EpsonUpdateException(
                    $"Funkcije '{functionMarker}' ni bilo mogoče najti - datoteka morda ni bila generirana s tem orodjem ali je bila preveč spremenjena.");

            int fendIdx = content.IndexOf("Fend", startIdx, StringComparison.Ordinal);
            if (fendIdx < 0)
                throw new EpsonUpdateException($"Manjkajoč zaključek 'Fend' za funkcijo '{functionMarker}'.");

            return (startIdx, fendIdx);
        }

        // Če se markerIdx premakne nazaj čez vodilne presledke/tabe do začetka vrstice, vstavek
        // pristane PRED celo izvirno vrstico (ne sredi njenega zamika).
        private static int BackUpToStartOfLine(string text, int idx)
        {
            while (idx > 0 && (text[idx - 1] == ' ' || text[idx - 1] == '\t'))
                idx--;
            return idx;
        }

        private static string InsertBeforeMarkerInBlock(string content, string functionMarker, string insideMarker, string insertion)
        {
            var (start, end) = FindFunctionBlockRange(content, functionMarker);
            string block = content.Substring(start, end - start);
            int markerIdx = block.IndexOf(insideMarker, StringComparison.Ordinal);
            if (markerIdx < 0)
                throw new EpsonUpdateException(
                    $"V funkciji '{functionMarker}' ni bilo mogoče najti pričakovanega mesta za vstavljanje ('{insideMarker.Trim()}').");
            markerIdx = BackUpToStartOfLine(block, markerIdx);

            int absoluteInsertIdx = start + markerIdx;
            return content.Substring(0, absoluteInsertIdx) + insertion + content.Substring(absoluteInsertIdx);
        }

        // Ujame celoten prelom vrstice + vodilni presledek/tab pred "Else" (ne "ElseIf") - vstavek
        // gre TAKOJ ZA prelomom vrstice, tako da cela izvirna "Else" vrstica (z vsemi tabi) ostane
        // popolnoma nedotaknjena, namesto da bi vstavek pristal sredi njenega vodilnega zamika.
        private static readonly Regex StandaloneElseLineRegex = new Regex(@"\n[ \t]*Else(?!If)", RegexOptions.Compiled);

        private static string InsertBeforeStandaloneElse(string content, string functionMarker, string insertion)
        {
            var (start, end) = FindFunctionBlockRange(content, functionMarker);
            string block = content.Substring(start, end - start);
            var m = StandaloneElseLineRegex.Match(block);
            if (!m.Success)
                throw new EpsonUpdateException($"V funkciji '{functionMarker}' ni bilo mogoče najti zaključnega 'Else' stavka.");

            int absoluteInsertIdx = start + m.Index + 1;
            return content.Substring(0, absoluteInsertIdx) + insertion + content.Substring(absoluteInsertIdx);
        }

        private static string PatchLineListFunction(string content, string functionFullName, bool shouldAdd, string lineText)
        {
            if (!shouldAdd) return content;

            // Vstavi novo vrstico točno na mestu, kjer se začne zaključni "Fend" - obstoječi
            // zaključni prelom vrstice (CRLF ali LF) pred njim ostane nedotaknjen.
            string marker = $"Function {functionFullName}";
            var (start, end) = FindFunctionBlockRange(content, marker);
            return content.Substring(0, end) + lineText + "\r\n" + content.Substring(end);
        }

        #endregion

        #region ////// Popravki po sekciji (posnemajo GetXxxSimpleProgramFunc iz Template.cs) //////

        private static readonly Regex ForLoopBoundRegex = new Regex(@"For index = 0 To (\d+)", RegexOptions.Compiled);

        private static string PatchHoming(string content, ProgramModel program, StationModel station, int stationIndex)
        {
            string marker = $"Function {program.ProgramNameLowerChar}_homing";
            string insertion = $"\t\t\t\tCase {stationIndex}\n\t\t\t\t\tMemOn ROBOT_FROM_{station.RobotStationNameToUpper}\n\t\t\t\t\trobot_onStation = True\n\n";
            content = InsertBeforeMarkerInBlock(content, marker, "\t\t\t\tDefault", insertion);

            // "For index = 0 To N" preišče najbližjo naučeno točko po ZAPOREDNI poziciji (P(index)) -
            // brez povečanja N Homing nikoli ne bi preveril točke novo dodane postaje.
            var (start, end) = FindFunctionBlockRange(content, marker);
            string block = content.Substring(start, end - start);
            var m = ForLoopBoundRegex.Match(block);
            if (!m.Success)
                throw new EpsonUpdateException($"V '{marker}' ni bilo mogoče najti 'For index = 0 To ...' zanke za homing.");

            int newBound = int.Parse(m.Groups[1].Value) + 1;
            int groupStart = start + m.Groups[1].Index;
            return content.Substring(0, groupStart) + newBound + content.Substring(groupStart + m.Groups[1].Length);
        }

        private static string PatchOperation(string content, ProgramModel program, StationModel station, int stationIndex)
        {
            string marker = $"Function {program.ProgramNameLowerChar}_mainTask";
            string insertion = $"\t\t\t\tCase {stationIndex}\n\t\t\t\t\tCall {program.ProgramNameLowerChar}_go{station.RobotStationName}()\n";
            return InsertBeforeMarkerInBlock(content, marker, "\t\t\t\tDefault", insertion);
        }

        private static string BuildMovementCommand(ProgramModel program, StationModel station)
        {
            string progUpper = program.ProgramNameUpperChar;
            if (station.Pallet)
                return $"\t\tGo Pallet ({station.RobotStationName}, additionalPos) :Z({progUpper}_MAX_Z_HEIGHT) \n\n";
            if (station.Positions != 1)
                return $"\t\tCall go{station.RobotStationName}MaxZHeight \n\n";
            return $"\t\tGo p{station.RobotStationName} :Z({progUpper}_MAX_Z_HEIGHT) \n\n";
        }

        // Vsaka go{Postaja}() funkcija ima svojo notranjo verigo "od kod prihajam", ki mora
        // poznati VSE postaje kot možen izvor (vsaka veja te verige za DANO ciljno postajo
        // izvede isti premik ne glede na izvor - obstoječa lastnost generatorja). Če nove postaje
        // ne dodamo kot izvor v VSE obstoječe go-funkcije, bi robot ob premiku STRAN od nove
        // postaje padel v "Else / Error PROGRAM_ERROR" namesto da bi se premaknil.
        private static string PatchExistingGoFunctionsForNewOrigin(
            string content, ProgramModel program, StationModel newStation, HashSet<StationModel> notYetCreated)
        {
            string progUpper = program.ProgramNameUpperChar;
            string newStationUpper = newStation.RobotStationNameToUpper;

            foreach (var existingStation in program.Stations)
            {
                if (notYetCreated.Contains(existingStation)) continue;

                string marker = $"Function {program.ProgramNameLowerChar}_go{existingStation.RobotStationName}()";
                string movementCmd = BuildMovementCommand(program, existingStation);
                string branch = $"\tElseIf MemSw ({progUpper}_FROM_{newStationUpper}) = On Then\n{movementCmd}";
                content = InsertBeforeStandaloneElse(content, marker, branch);
            }
            return content;
        }

        private static string PatchMovement(string content, ProgramModel program, StationModel station)
        {
            var regex = new Regex($@"Function\s+{Regex.Escape(program.ProgramNameLowerChar)}_go\w+\s*\(\)");
            var matches = regex.Matches(content);
            if (matches.Count == 0)
                throw new EpsonUpdateException("V datoteki ni bilo mogoče najti nobene obstoječe 'go' funkcije premika, za katero bi dodali novo.");

            var last = matches[matches.Count - 1];
            int fendIdx = content.IndexOf("Fend", last.Index, StringComparison.Ordinal);
            if (fendIdx < 0)
                throw new EpsonUpdateException("Manjkajoč zaključek 'Fend' zadnje 'go' funkcije premika.");

            int insertPos = fendIdx + 4; // takoj za "Fend"
            string newFunctionText = "\n\n" + BuildMovementFunctionText(program, station);
            return content.Substring(0, insertPos) + newFunctionText + content.Substring(insertPos);
        }

        private static string BuildMovementFunctionText(ProgramModel program, StationModel station)
        {
            string progLower = program.ProgramNameLowerChar;
            string progUpper = program.ProgramNameUpperChar;
            string stationUpper = station.RobotStationNameToUpper;
            string movementCmd = BuildMovementCommand(program, station);

            // Opomba: replicira obstoječe (nekoliko nenavadno) vedenje GetMovementsSimpleProgramFunc,
            // kjer se ISTO gibanje ponovi v vsaki If/ElseIf veji ne glede na to, katera "from" lokacija je ustrezala.
            var destinations = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
            {
                var s = program.Stations[i];
                destinations.Append(i == 0
                    ? $"\tIf MemSw ({progUpper}_FROM_{s.RobotStationNameToUpper}) = On Then\n"
                    : $"\tElseIf MemSw ({progUpper}_FROM_{s.RobotStationNameToUpper}) = On Then\n");
                destinations.Append(movementCmd);
            }

            var stationFree = new StringBuilder();
            if (station.StationFreeEnabled)
            {
                stationFree.Append("\n\tIf Sw(ROBOT_I_ON_STATION) = On Then\n");
                stationFree.Append($"\t\tOff {progUpper}_O_{stationUpper}_FREE\n");
                stationFree.Append("\tEndIf\n");
            }

            var sb = new StringBuilder();
            sb.Append($"Function {progLower}_go{station.RobotStationName}()\n");
            sb.Append("\t' Set location to which the robot is going\n");
            sb.Append($"\tMemOn {progUpper}_TO_{stationUpper}\t\n");
            sb.Append($"\t{stationFree}\n");
            sb.Append("\t' Move away\n");
            sb.Append($"\tCall {progLower}_moveAway \n");
            sb.Append("\t\n");
            sb.Append("\t' Set power mode \n");
            sb.Append($"\tCall {progLower}_setPowerMode \n");
            sb.Append("\t\n");
            sb.Append(destinations);
            sb.Append("\tElse\n");
            sb.Append("\t\tError PROGRAM_ERROR\n");
            sb.Append("\tEndIf\n");
            sb.Append("\t\n");
            sb.Append("\t' Go on station\n");
            sb.Append($"\tCall {progLower}_moveOnStation\n");
            sb.Append("\t\n");
            sb.Append("\t' Reset depart locations\n");
            sb.Append($"\tCall {progLower}_resetDepartLocations\n");
            sb.Append("\t\n");
            sb.Append($"\t' Set the depart location as {station.RobotStationName} (for the next move -> robot needs to know from where it is starting in the next move) and reset the destination location (depart location is now the destination location)\n");
            sb.Append($"\tMemOn {progUpper}_FROM_{stationUpper}\n");
            sb.Append($"\tMemOff {progUpper}_TO_{stationUpper}\n");
            sb.Append("Fend");
            return sb.ToString();
        }

        private static string PatchMoveOnStation(string content, ProgramModel program, StationModel station)
        {
            string marker = $"Function {program.ProgramNameLowerChar}_moveOnStation";
            string progUpper = program.ProgramNameUpperChar;
            string progLower = program.ProgramNameLowerChar;
            string stationUpper = station.RobotStationNameToUpper;
            string s = station.RobotStationName;

            var sb = new StringBuilder();
            sb.Append($"\t\tElseIf MemSw({progUpper}_TO_{stationUpper}) = On Then\n");
            if (station.StationFreeEnabled)
                sb.Append($"\t\t\tOff {progUpper}_O_{stationUpper}_FREE\n\n");

            if (station.Pallet)
            {
                sb.Append($"\t\t\tIf CZ(CurPos) > (CZ(p{s}P1) + {progUpper}_STATION_Z_OFFSET) = True Then\n");
                sb.Append($"\t\t\t\tGo Pallet ({s} , additionalPos) +Z({progUpper}_STATION_Z_OFFSET)\n");
                sb.Append("\t\t\tEndIf\n");
                sb.Append("\t\t\t' Go slower working mode\n");
                sb.Append($"\t\t\tCall {progLower}_slowerWorkingMode({progUpper}_SLOWER_SPEED, {progUpper}_APPROACH_ACCEL, {progUpper}_APPROACH_DECEL)\n");
                sb.Append($"\t\t\tGo Pallet ({s} , additionalPos)\n\n");
            }
            else if (station.Positions != 1)
            {
                sb.Append($"\t\t\tIf CZ(CurPos) > (CZ(p{s}1) + {progUpper}_STATION_Z_OFFSET) = True Then\n");
                sb.Append($"\t\t\t\tCall go{s}NearStation\n");
                sb.Append("\t\t\tEndIf\n");
                sb.Append("\t\t\t' Go slower working mode\n");
                sb.Append($"\t\t\tCall {progLower}_slowerWorkingMode({progUpper}_SLOWER_SPEED, {progUpper}_APPROACH_ACCEL, {progUpper}_APPROACH_DECEL)\n");
                sb.Append($"\t\t\tCall go{s}\n\n");
            }
            else
            {
                sb.Append($"\t\t\tIf CZ(CurPos) > (CZ(p{s}) + {progUpper}_STATION_Z_OFFSET) = True Then\n");
                sb.Append($"\t\t\t\tGo p{s} +Z({progUpper}_STATION_Z_OFFSET)\n");
                sb.Append("\t\t\tEndIf\n");
                sb.Append("\t\t\t' Go slower working mode\n");
                sb.Append($"\t\t\tCall {progLower}_slowerWorkingMode({progUpper}_SLOWER_SPEED, {progUpper}_APPROACH_ACCEL, {progUpper}_APPROACH_DECEL)\n");
                sb.Append($"\t\t\tGo p{s}\n\n");
            }

            return InsertBeforeStandaloneElse(content, marker, sb.ToString());
        }

        private static string PatchMoveAway(string content, ProgramModel program, StationModel station)
        {
            string marker = $"Function {program.ProgramNameLowerChar}_moveAway";
            string progUpper = program.ProgramNameUpperChar;
            string progLower = program.ProgramNameLowerChar;
            string stationUpper = station.RobotStationNameToUpper;

            var sb = new StringBuilder();
            sb.Append($"\t\tElseIf MemSw({progUpper}_FROM_{stationUpper}) = On Then\n");
            sb.Append($"\t\t\tIf CZ(CurPos) + {progUpper}_STATION_Z_OFFSET < {progUpper}_MAX_Z_HEIGHT = True Then\n");
            sb.Append("\t\t\t\t' Go slower depart working speed\n");
            sb.Append($"\t\t\t\tCall {progLower}_slowerWorkingMode({progUpper}_SLOWER_SPEED, {progUpper}_DEPART_ACCEL, {progUpper}_DEPART_DECEL)\n");
            sb.Append($"\t\t\t\tGo RealPos +Z({progUpper}_STATION_Z_OFFSET)\n");
            sb.Append("\t\t\tEndif\n");
            sb.Append("\t\t\t' Go full wroking speed\n");
            sb.Append($"\t\t\tCall {progLower}_fullWorkingSpeed\n");
            sb.Append($"\t\t\tGo RealPos :Z({progUpper}_MAX_Z_HEIGHT)\n\n");
            if (station.StationFreeEnabled)
                sb.Append($"\t\t\tOn {progUpper}_O_{stationUpper}_FREE\n\n");

            return InsertBeforeStandaloneElse(content, marker, sb.ToString());
        }

        private static string PatchAdditionalPositions(string content, ProgramModel program, StationModel station)
        {
            string marker = $"Function {program.ProgramNameLowerChar}_setPowerMode";
            int idx = content.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
                throw new EpsonUpdateException($"Funkcije '{marker}' ni bilo mogoče najti - potrebna za vstavljanje dodatnih pozicij.");

            string s = station.RobotStationName;
            var sb = new StringBuilder();

            sb.Append($"Function go{s}MaxZHeight\n\tSelect additionalPos\n");
            for (int j = 1; j <= station.Positions; j++)
                sb.Append($"\t\tCase {j}\n\t\t\tGo p{s}{j} :Z(ROBOT_MAX_Z_HEIGHT)\n\n");
            sb.Append("\t\tDefault\n\t\t\tError POSITION_ERROR\n\tSend\nFend\n\n");

            sb.Append($"Function go{s}NearStation\n\tSelect additionalPos\n");
            for (int j = 1; j <= station.Positions; j++)
                sb.Append($"\t\tCase {j}\n\t\t\tGo p{s}{j} +Z(ROBOT_STATION_Z_OFFSET)\n\n");
            sb.Append("\t\tDefault\n\t\t\tError POSITION_ERROR\n\tSend\nFend\n\n");

            sb.Append($"Function go{s}\n\tSelect additionalPos\n");
            for (int j = 1; j <= station.Positions; j++)
                sb.Append($"\t\tCase {j}\n\t\t\tGo p{s}{j}\n\n");
            sb.Append("\t\tDefault\n\t\t\tError POSITION_ERROR\n\tSend\nFend\n\n");

            return content.Substring(0, idx) + sb.ToString() + content.Substring(idx);
        }

        #endregion
    }
}
