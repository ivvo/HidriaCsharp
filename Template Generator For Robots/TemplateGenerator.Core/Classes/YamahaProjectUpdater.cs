using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class YamahaUpdateException : Exception
    {
        public YamahaUpdateException(string message) : base(message) { }
    }

    // Kirurško vstavljanje na koncu dodanih postaj v že generiran Yamaha projekt. Vsa Yamaha koda,
    // naučene točke in IO oznake so v ENI ".all" datoteki, zato se vse spremembe izvedejo nad enim
    // nizom. Za razliko od KUKA/ABB pri Yamahi ni omejitve "en robot na projekt" (datoteke so
    // neodvisne), a velja ista predpostavka kot povsod: nove postaje se dodajajo SAMO na konec.
    //
    // Kritična past (enaka kot pri Epsonu): homing (*findClosestPoint:) išče najbližjo NAUČENO točko
    // po ZAPOREDNEM številčnem indeksu (FOR i = 0 TO N, P[i]), zato mora primarna točka novo dodane
    // postaje priti na indeks = njeno mesto v seznamu postaj, morebitne obstoječe dodatne
    // (večpozicijske) točke pa se morajo prenumerirati navzgor. Sekcija [PNT] (naučene koordinate) se
    // NIKOLI ne prepiše v celoti - obstoječe koordinate se ohranijo dobesedno.
    public static class YamahaProjectUpdater
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

        private const string PlaceholderCoords = "0.000 0.000 0.000 0.000 0.000 0.000 1 0 0";
        private const string PlaceholderComment = "ni ustimano !!!";
        private const string MoveToMarker = "'------------------------------------------------------------MOVE TO:";

        // newStations: postaje, dodane na konec program.Stations po zadnjem uvozu/posodobitvi.
        public static void UpdateProgram(ProgramModel program, List<StationModel> newStations, string path)
        {
            if (newStations == null || newStations.Count == 0) return;

            string allPath = Path.Combine(path, $"{program.ProgramName}.all");
            if (!File.Exists(allPath))
                throw new YamahaUpdateException($"Datoteka '{allPath}' ne obstaja - ali je bila izbrana pravilna mapa projekta?");

            string content = File.ReadAllText(allPath);
            int previousCount = program.Stations.Count - newStations.Count;

            // Nove postaje so že v modelu; njihovih go-funkcij (zgrajenih v celoti) NE popravljamo kot
            // "obstoječih" - dodajamo jih le kot izvor v go-funkcije PRVOTNIH postaj.
            var newSet = new HashSet<StationModel>(newStations);

            for (int idx = 0; idx < newStations.Count; idx++)
            {
                var station = newStations[idx];
                int stationIndex = previousCount + idx;

                content = PatchHeaderVar(content, program, station, stationIndex);
                content = PatchHomingCase(content, station, stationIndex);
                content = PatchMainTaskCase(content, station);
                content = PatchExistingGoFunctionsForNewOrigin(content, program, station, newSet);
                content = PatchNewGoFunction(content, program, station);
                content = PatchMoveOnStation(content, station);
                content = PatchMoveAway(content, station);
                if (station.StationFreeEnabled)
                {
                    content = PatchSignalList(content, "*robot_stationsBusy:", "RESET", station);
                    content = PatchSignalList(content, "*robot_stationsFree:", "SET", station);
                    content = PatchIoSonm4(content, station);
                }
            }

            // FOR i = 0 TO N v *findClosestPoint: - N mora biti (skupno število točk - 1).
            content = PatchFindClosestPointBound(content, program);

            // [PNT]/[PCM]/[PNM] - ohrani obstoječe (naučene) točke, vstavi nove na PRAVA mesta.
            content = UpdatePointsSections(content, previousCount, newStations);

            File.WriteAllText(allPath, content);
        }

        #region ////// Iskanje blokov "*robot_xxx: ... RETURN" //////

        private static (int start, int end) FindLabelBlock(string content, string label)
        {
            int start = content.IndexOf(label, StringComparison.Ordinal);
            if (start < 0)
                throw new YamahaUpdateException(
                    $"Oznake '{label}' ni bilo mogoče najti - datoteka morda ni bila generirana s tem orodjem ali je bila preveč spremenjena.");

            int ret = content.IndexOf("\nRETURN", start, StringComparison.Ordinal);
            if (ret < 0)
                throw new YamahaUpdateException($"Manjkajoč zaključek 'RETURN' za '{label}'.");

            return (start, ret);
        }

        // Premakne idx nazaj čez vodilne presledke/tabe do začetka vrstice, da vstavek pristane PRED
        // celo izvirno vrstico (ne sredi njenega zamika).
        private static int BackUpToStartOfLine(string text, int idx)
        {
            while (idx > 0 && (text[idx - 1] == ' ' || text[idx - 1] == '\t'))
                idx--;
            return idx;
        }

        // Predloge imajo za "{0}" (seznamom vej) prazno ločilno vrstico TIK PRED zaključnim markerjem
        // (npr. "...'\n\n        CASE ELSE"). Nova veja mora priti ZA zadnjo pravo vrstico in PRED to
        // prazno ločilno vrstico (tako kot pri svežem generiranju), zato se z začetka markerjeve
        // vrstice premaknemo nazaj čez morebitne prazne vrstice. Če prazne vrstice ni (npr. veriga
        // IF/ELSE znotraj go-funkcije), idx ostane nespremenjen.
        private static int BackUpOverBlankLines(string text, int idx)
        {
            while (idx >= 2 && text[idx - 1] == '\n' && text[idx - 2] == '\n')
                idx--;
            return idx;
        }

        private static string InsertBeforeMarkerInBlock(string content, string label, string insideMarker, string insertion)
        {
            var (start, end) = FindLabelBlock(content, label);
            string block = content.Substring(start, end - start);
            int markerIdx = block.IndexOf(insideMarker, StringComparison.Ordinal);
            if (markerIdx < 0)
                throw new YamahaUpdateException(
                    $"V '{label}' ni bilo mogoče najti pričakovanega mesta za vstavljanje ('{insideMarker.Trim()}').");
            markerIdx = BackUpToStartOfLine(block, markerIdx);

            int absoluteIdx = BackUpOverBlankLines(content, start + markerIdx);
            return content.Substring(0, absoluteIdx) + insertion + content.Substring(absoluteIdx);
        }

        // Zaključni "ELSE" veje IF/ELSEIF (tisti, ki mu sledi "'output error") - loči ga od notranjih
        // ELSE (npr. additional_pos izbor), ki jim sledi "SET O_PROGRAM_ERROR" brez komentarja.
        private static readonly Regex ErrorElseRegex = new Regex(@"\n[ \t]*ELSE\r?\n[ \t]*'output error", RegexOptions.Compiled);

        private static string InsertBeforeErrorElseInBlock(string content, string label, string insertion)
        {
            var (start, end) = FindLabelBlock(content, label);
            string block = content.Substring(start, end - start);
            var m = ErrorElseRegex.Match(block);
            if (!m.Success)
                throw new YamahaUpdateException($"V '{label}' ni bilo mogoče najti zaključnega 'ELSE' (pred \"'output error\").");

            int absoluteIdx = BackUpOverBlankLines(content, start + m.Index + 1); // pred prazno ločilno vrstico + "ELSE"
            return content.Substring(0, absoluteIdx) + insertion + content.Substring(absoluteIdx);
        }

        #endregion

        #region ////// Posamezni popravki //////

        // Glava: "<Postaja> = <indeks>" - vstavi novo spremenljivko pozicije za zadnjo obstoječo.
        private static string PatchHeaderVar(string content, ProgramModel program, StationModel station, int stationIndex)
        {
            string prevName = program.Stations[stationIndex - 1].RobotStationName;
            string prevLine = $"{prevName} = {stationIndex - 1}";
            int idx = content.IndexOf(prevLine, StringComparison.Ordinal);
            if (idx < 0)
                throw new YamahaUpdateException($"V glavi ni bilo mogoče najti spremenljivke pozicije '{prevLine}'.");

            int lineEnd = content.IndexOf('\n', idx);
            if (lineEnd < 0)
                throw new YamahaUpdateException($"Nepričakovan konec datoteke pri spremenljivki pozicije '{prevLine}'.");

            string insertion = $"{station.RobotStationName} = {stationIndex}\n";
            return content.Substring(0, lineEnd + 1) + insertion + content.Substring(lineEnd + 1);
        }

        private static string PatchHomingCase(string content, StationModel station, int stationIndex)
        {
            return InsertBeforeMarkerInBlock(content, "*robot_homing:", "CASE ELSE",
                Template.BuildYamahaHomingCase(station, stationIndex));
        }

        private static string PatchMainTaskCase(string content, StationModel station)
        {
            return InsertBeforeMarkerInBlock(content, "*robot_mainTask:", "CASE ELSE",
                Template.BuildYamahaMainTaskCase(station));
        }

        // Vsaka obstoječa "*robot_go<Postaja>:" funkcija ima notranjo verigo "IF PositionFrom = ...",
        // ki mora poznati VSE postaje kot možen izvor ("origin completeness"). Nove go-funkcije so
        // zgrajene že v celoti (GetYamahaGoFunction dobi cel program), zato jih tu preskočimo -
        // dodajamo novo postajo kot izvor le v funkcije PRVOTNIH postaj.
        private static string PatchExistingGoFunctionsForNewOrigin(
            string content, ProgramModel program, StationModel newStation, HashSet<StationModel> newSet)
        {
            string branch =
                $"    ELSEIF PositionFrom = {newStation.RobotStationName} THEN\n" +
                "        MOVE P, pAboveStation, CONT\n" +
                "        '\n";

            foreach (var existingStation in program.Stations)
            {
                if (newSet.Contains(existingStation)) continue;
                content = InsertBeforeErrorElseInBlock(content, $"*robot_go{existingStation.RobotStationName}:", branch);
            }
            return content;
        }

        private static string PatchNewGoFunction(string content, ProgramModel program, StationModel station)
        {
            int markerIdx = content.IndexOf(MoveToMarker, StringComparison.Ordinal);
            if (markerIdx < 0)
                throw new YamahaUpdateException($"Ni bilo mogoče najti oznake '{MoveToMarker.Trim()}' za vstavljanje nove go-funkcije.");

            string newFunc = Template.GetYamahaGoFunction(program, station) + "\n";
            return content.Substring(0, markerIdx) + newFunc + content.Substring(markerIdx);
        }

        private static string PatchMoveOnStation(string content, StationModel station)
        {
            return InsertBeforeErrorElseInBlock(content, "*robot_moveOnStation:",
                Template.BuildYamahaMoveOnStationBranch(station, false));
        }

        private static string PatchMoveAway(string content, StationModel station)
        {
            return InsertBeforeErrorElseInBlock(content, "*robot_moveAway:",
                Template.BuildYamahaMoveAwayBranch(station, false));
        }

        // *robot_stationsBusy: (RESET) / *robot_stationsFree: (SET) - doda vrstico za novo "free" postajo
        // za zadnjo obstoječo RESET/SET vrstico (oz. za komentar, če je seznam še prazen).
        private static string PatchSignalList(string content, string label, string verb, StationModel station)
        {
            var (start, end) = FindLabelBlock(content, label);
            string block = content.Substring(start, end - start);

            string newLine = $"    {verb} O_{station.RobotStationName}_FREE\n";

            var lineRegex = new Regex($@"\n    {verb} O_\w+_FREE");
            var matches = lineRegex.Matches(block);
            int insertAt;
            if (matches.Count > 0)
            {
                var last = matches[matches.Count - 1];
                int lineEnd = block.IndexOf('\n', last.Index + 1);
                insertAt = lineEnd >= 0 ? lineEnd + 1 : block.Length;
            }
            else
            {
                // Prazen seznam: vstavi za komentarsko vrstico "'Set all stations ...".
                int commentIdx = block.IndexOf("'Set all stations", StringComparison.Ordinal);
                if (commentIdx < 0)
                    throw new YamahaUpdateException($"V '{label}' ni bilo mogoče najti komentarja 'Set all stations ...'.");
                int commentEnd = block.IndexOf('\n', commentIdx);
                insertAt = commentEnd >= 0 ? commentEnd + 1 : block.Length;
            }

            int absoluteIdx = start + insertAt;
            return content.Substring(0, absoluteIdx) + newLine + content.Substring(absoluteIdx);
        }

        private static readonly Regex Sonm4Regex = new Regex(@"SONM4\((\d+)\)=", RegexOptions.Compiled);

        private static string PatchIoSonm4(string content, StationModel station)
        {
            var matches = Sonm4Regex.Matches(content);
            if (matches.Count > 0)
            {
                var last = matches[matches.Count - 1];
                int nextK = int.Parse(last.Groups[1].Value) + 1;
                int lineEnd = content.IndexOf('\n', last.Index);
                int insertAt = lineEnd >= 0 ? lineEnd + 1 : content.Length;
                string line = $"SONM4({nextK})=O_{station.RobotStationName}_FREE\n";
                return content.Substring(0, insertAt) + line + content.Substring(insertAt);
            }

            // Prazen SONM4 seznam: predloga je na mesto "{0}" pustila prazno vrstico med
            // "SONM2(5)=O_PROGRAM_ERROR" in "SINM2(0)". Prvi SONM4 signal to prazno vrstico
            // NADOMESTI (kot pri svežem generiranju), ne doda povrh nje.
            const string anchor = "SONM2(5)=O_PROGRAM_ERROR";
            int errIdx = content.IndexOf(anchor, StringComparison.Ordinal);
            if (errIdx < 0)
                throw new YamahaUpdateException("V [ION] ni bilo mogoče najti 'SONM2(5)=O_PROGRAM_ERROR' za vstavljanje prvega SONM4 signala.");
            int errLineEnd = content.IndexOf('\n', errIdx);
            if (errLineEnd < 0)
                throw new YamahaUpdateException("Nepričakovan konec [ION] sekcije.");

            int at = errLineEnd + 1;
            string newLine = $"SONM4(0)=O_{station.RobotStationName}_FREE\n";
            // Če takoj sledi prazna ločilna vrstica, jo nadomesti (ne podvoji).
            int restStart = (at < content.Length && content[at] == '\n') ? at + 1 : at;
            return content.Substring(0, at) + newLine + content.Substring(restStart);
        }

        private static readonly Regex ForBoundRegex = new Regex(@"FOR i = 0 TO \d+", RegexOptions.Compiled);

        private static string PatchFindClosestPointBound(string content, ProgramModel program)
        {
            int totalPoints = program.Stations.Count + program.Stations.Sum(s => s.Positions != 1 ? s.Positions - 1 : 0);
            int newBound = totalPoints - 1;

            if (!ForBoundRegex.IsMatch(content))
                throw new YamahaUpdateException("V '*findClosestPoint:' ni bilo mogoče najti 'FOR i = 0 TO N' zanke za homing.");

            return ForBoundRegex.Replace(content, $"FOR i = 0 TO {newBound}", 1);
        }

        #endregion

        #region ////// [PNT]/[PCM]/[PNM] - ohrani naučene točke, vstavi nove na PRAVA mesta //////

        private static string UpdatePointsSections(string content, int previousStationCount, List<StationModel> newStations)
        {
            int pntIdx = content.IndexOf("[PNT]", StringComparison.Ordinal);
            int pcmIdx = content.IndexOf("[PCM]", StringComparison.Ordinal);
            int pnmIdx = content.IndexOf("[PNM]", StringComparison.Ordinal);
            int sftIdx = content.IndexOf("[SFT]", StringComparison.Ordinal);
            if (pntIdx < 0 || pcmIdx < 0 || pnmIdx < 0 || sftIdx < 0 || !(pntIdx < pcmIdx && pcmIdx < pnmIdx && pnmIdx < sftIdx))
                throw new YamahaUpdateException("Sekcij [PNT]/[PCM]/[PNM]/[SFT] ni bilo mogoče najti v pričakovanem vrstnem redu.");

            var pnt = ParseIndexedSection(content.Substring(pntIdx, pcmIdx - pntIdx), "P");
            var pcm = ParseIndexedSection(content.Substring(pcmIdx, pnmIdx - pcmIdx), "PC");
            var pnm = ParseIndexedSection(content.Substring(pnmIdx, sftIdx - pnmIdx), "PN");

            if (pnt.Count != pcm.Count || pnt.Count != pnm.Count)
                throw new YamahaUpdateException(
                    $"Neskladje v številu točk: [PNT]={pnt.Count}, [PCM]={pcm.Count}, [PNM]={pnm.Count}.");

            int shift = newStations.Count; // toliko novih PRIMARNIH točk se vrine na mesta [previousStationCount..]

            var newPnt = new SortedDictionary<int, string>();
            var newPcm = new SortedDictionary<int, string>();
            var newPnm = new SortedDictionary<int, string>();

            foreach (var kv in pnt)
            {
                int ni = kv.Key >= previousStationCount ? kv.Key + shift : kv.Key;
                newPnt[ni] = kv.Value;
                newPcm[ni] = pcm[kv.Key];
                newPnm[ni] = pnm[kv.Key];
            }

            // Primarne točke novih postaj na mesta [previousStationCount, +1, ...].
            int nextPrimary = previousStationCount;
            foreach (var station in newStations)
            {
                newPnt[nextPrimary] = PlaceholderCoords;
                newPcm[nextPrimary] = PlaceholderComment;
                newPnm[nextPrimary] = "p" + Template.PrimaryPointName(station);
                nextPrimary++;
            }

            // Dodatne (večpozicijske) točke novih postaj na konec.
            int nextExtra = newPnt.Keys.Max() + 1;
            foreach (var station in newStations)
            {
                if (station.Positions == 1) continue;
                for (int j = 2; j <= station.Positions; j++)
                {
                    newPnt[nextExtra] = PlaceholderCoords;
                    newPcm[nextExtra] = PlaceholderComment;
                    newPnm[nextExtra] = $"p{station.RobotStationName}{j}";
                    nextExtra++;
                }
            }

            var sb = new StringBuilder();
            sb.Append("[PNT]\n");
            foreach (var kv in newPnt) sb.Append($"P{kv.Key}={kv.Value}\n");
            sb.Append("[PCM]\n");
            foreach (var kv in newPcm) sb.Append($"PC{kv.Key}={kv.Value}\n");
            sb.Append("[PNM]\n");
            foreach (var kv in newPnm) sb.Append($"PN{kv.Key}={kv.Value}\n");

            return content.Substring(0, pntIdx) + sb + content.Substring(sftIdx);
        }

        // Prebere sekcijo oblike "<prefix><N>=<vrednost>" v slovar N -> vrednost.
        private static SortedDictionary<int, string> ParseIndexedSection(string section, string prefix)
        {
            var result = new SortedDictionary<int, string>();
            var regex = new Regex($@"^{Regex.Escape(prefix)}(\d+)=(.*)$", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(section))
                result[int.Parse(m.Groups[1].Value)] = m.Groups[2].Value.TrimEnd('\r');
            return result;
        }

        #endregion
    }
}
