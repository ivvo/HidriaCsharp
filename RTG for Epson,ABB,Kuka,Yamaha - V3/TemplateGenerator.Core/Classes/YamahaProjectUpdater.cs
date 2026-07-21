using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    // Posodobitev Yamaha projekta (V3) z na novo dodanimi postajami.
    //
    // Zaradi zapletene, a popolnoma DETERMINISTIČNE zgradbe Yamaha "BackupFile.all" (redek seznam
    // točk z vrzelmi, varni točki pSafeL/pSafeR, pAboveStation/pFinal, comma-list homing, valeča IO
    // shema) tu NE delamo kirurškega vstavljanja po odsekih, ampak celoten robotov del RE-GENERIRAMO
    // iz modela (z že dodanimi postajami) - enako kot Generate - in nato NAUČENE koordinate/komentarje
    // OHRANIMO tako, da jih po IMENU točke (pHome, pStation1, pSafeL, ...) prekrijemo nazaj v na novo
    // generirano datoteko. Tako:
    //   - koda (homing, go-funkcije, origin-completeness ...) je vedno pravilna in popolna,
    //   - vse naučene točke ([PNT]) in komentarji ([PCM]) se ohranijo dobesedno,
    //   - rezultat je bajt-identičen svežemu Generate celotnega seznama postaj (razen ohranjenih točk).
    //
    // Opomba/omejitev: ker se koda re-generira, morebitni ROČNI popravki v generirani .all kodi ob
    // posodobitvi NISO ohranjeni (za razliko od Epson/KUKA/ABB kirurške poti). Naučene točke SO
    // ohranjene - to je glavni namen. Podprt je en robot na projekt (glej Importer).
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

        public static void UpdateProgram(ProgramModel program, List<StationModel> newStations, string path)
        {
            if (newStations == null || newStations.Count == 0) return;

            var allFiles = Directory.GetFiles(path, "*.all");
            if (allFiles.Length == 0)
                throw new YamahaUpdateException($"V mapi '{path}' ni datoteke '.all' (BackupFile.all) - ali je bila izbrana pravilna mapa projekta?");
            if (allFiles.Length > 1)
                throw new YamahaUpdateException($"V mapi '{path}' je več '.all' datotek - pričakovana je ena (BackupFile.all).");

            string allPath = allFiles[0];
            string oldContent = File.ReadAllText(allPath);

            // Naučene koordinate/komentarji obstoječih točk (po imenu).
            var oldPnm = ParseIndexedSection(oldContent, "[PNM]", "PN");
            var oldPnt = ParseIndexedSection(oldContent, "[PNT]", "P");
            var oldPcm = ParseIndexedSection(oldContent, "[PCM]", "PC");
            var oldCoordsByName = new Dictionary<string, string>();
            var oldCommentByName = new Dictionary<string, string>();
            foreach (var kv in oldPnm)
            {
                if (oldPnt.TryGetValue(kv.Key, out var c)) oldCoordsByName[kv.Value] = c;
                if (oldPcm.TryGetValue(kv.Key, out var cm)) oldCommentByName[kv.Value] = cm;
            }

            // Re-generiraj celoten .all iz modela (z novimi postajami).
            string newContent = RegenerateAll(program);

            // Prekrij naučene koordinate/komentarje nazaj po IMENU točke.
            newContent = OverlayTaughtPoints(newContent, oldCoordsByName, oldCommentByName);

            File.WriteAllText(allPath, newContent);

            // Osveži še Excel IO tabelo (kot Generate).
            try { new GenerateExcelIO().GenerateIO("Yamaha", program, path); } catch { /* IO tabela ni ključna za .all */ }
        }

        // Natančna replika zaporedja pisanja iz ShellViewModel.GenerateProject (veja "Yamaha Hidria"),
        // brez stranskih učinkov (Excel). Podprt en robot na projekt.
        private static string RegenerateAll(ProgramModel program)
        {
            var progs = new ObservableCollection<ProgramModel> { program };
            var sb = new StringBuilder();

            sb.Append(Template.GetYamahaMainProg(progs));

            foreach (ProgramModel robot in progs)
            {
                sb.Append(Template.GetYamahaRobotProgHeader(robot));
                sb.Append(Template.GetYamahaRobotProgInit(robot));
                sb.Append(Template.GetYamahaRobotProgHoming(robot));
                sb.Append(Template.GetYamahaRobotProgTest(robot));
                sb.Append(Template.GetYamahaRobotProgMainTask(robot));
                sb.Append(Template.GetYamahaRobotProgGoHome(robot));
                for (int n = 1; n < robot.Stations.Count; n++)
                    sb.Append(Template.GetYamahaRobotProgGoStation(robot, n));
                sb.Append(Template.GetYamahaRobotProgMoveOnStation(robot));
                sb.Append(Template.GetYamahaRobotProgMoveAway(robot));
                sb.Append(Template.GetYamahaCommonProg(robot));
                sb.Append(Template.GetYamahaPointsFile(robot));
                sb.Append(Template.GetYamahaIoFile(robot));
            }
            sb.Append("\n" + "[END]");

            return sb.ToString();
        }

        // Za vsako na novo generirano točko, katere IME obstaja v stari datoteki, prekrij njene
        // koordinate ([PNT]) in komentar ([PCM]) s staro (naučeno) vrednostjo. Nove točke ostanejo
        // takšne, kot jih je generiral generator (ničle).
        private static string OverlayTaughtPoints(string content,
            Dictionary<string, string> oldCoordsByName, Dictionary<string, string> oldCommentByName)
        {
            int pntStart = content.IndexOf("[PNT]", StringComparison.Ordinal);
            if (pntStart < 0) return content; // ni sekcije točk - nič za prekriti

            string head = content.Substring(0, pntStart);
            string tail = content.Substring(pntStart);

            var regPnm = ParseIndexedSection(tail, "[PNM]", "PN"); // idx -> ime v na novo generirani datoteki
            foreach (var kv in regPnm)
            {
                int idx = kv.Key;
                string name = kv.Value;
                if (oldCoordsByName.TryGetValue(name, out var coords))
                    tail = ReplaceIndexedLine(tail, "P", idx, coords);
                if (oldCommentByName.TryGetValue(name, out var comment))
                    tail = ReplaceIndexedLine(tail, "PC", idx, comment);
            }

            return head + tail;
        }

        // Zamenja vrednost vrstice "<prefix><idx>=..." (npr. "P4=...") z dano vrednostjo, brez dotikanja
        // preloma vrstice. Uporabi MatchEvaluator, da se '$' v vrednosti ne interpretira.
        private static string ReplaceIndexedLine(string text, string prefix, int idx, string value)
        {
            var rx = new Regex("(?m)^" + Regex.Escape(prefix) + idx + "=[^\r\n]*$");
            return rx.Replace(text, m => prefix + idx + "=" + value, 1);
        }

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
    }
}
