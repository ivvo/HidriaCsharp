using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class KawasakiUpdateException : Exception
    {
        public KawasakiUpdateException(string message) : base(message) { }
    }

    // Posodobitev Kawasaki projekta (V3) z na novo dodanimi postajami.
    //
    // Enako kot pri Yamaha/Epson (V3): celoten "ProgramFile.as" RE-GENERIRAMO iz modela (z dodanimi
    // postajami) - enako kot Generate - in NAUČENE koordinate točk OHRANIMO tako, da jih po IMENU
    // (p<Postaja>, p<Postaja>N, p<Postaja>Final, #pHome) prekrijemo nazaj. Kawasaki najbližjo točko
    // išče po imenu (ne po indeksu), zato ni številčne pasti.
    //
    // Opomba/omejitev: koda se re-generira, zato ročni popravki v .as kodi niso ohranjeni; naučene
    // koordinate točk pa SO. En robot na projekt.
    public static class KawasakiProjectUpdater
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

        // Definicijska vrstica točke v stolpcu 0: "p<Ime> x y z ..." ali "#pHome ...".
        private static readonly Regex PointDefRegex = new Regex(@"(?m)^(#?p\w+) (-?\d[-\d. ]*)$", RegexOptions.Compiled);

        public static void UpdateProgram(ProgramModel program, List<StationModel> newStations, string path)
        {
            if (newStations == null || newStations.Count == 0) return;

            var asFiles = Directory.GetFiles(path, "*.as");
            if (asFiles.Length == 0)
                throw new KawasakiUpdateException($"V mapi '{path}' ni datoteke '.as' (ProgramFile.as) - ali je bila izbrana pravilna mapa projekta?");
            if (asFiles.Length > 1)
                throw new KawasakiUpdateException($"V mapi '{path}' je več '.as' datotek - pričakovana je ena (ProgramFile.as).");

            string asPath = asFiles[0];
            string oldContent = File.ReadAllText(asPath);

            // Naučene koordinate obstoječih točk po imenu.
            var taughtByName = new Dictionary<string, string>();
            foreach (Match m in PointDefRegex.Matches(oldContent))
                taughtByName[m.Groups[1].Value] = m.Groups[2].Value.TrimEnd();

            // Re-generiraj celoten ProgramFile.as iz modela (z novimi postajami).
            string newContent = RegenerateAs(program);

            // Prekrij naučene koordinate po imenu točke.
            newContent = PointDefRegex.Replace(newContent, m =>
            {
                string name = m.Groups[1].Value;
                return taughtByName.TryGetValue(name, out var coords) ? name + " " + coords : m.Value;
            });

            File.WriteAllText(asPath, newContent);

            try { new GenerateExcelIO().GenerateIO("Kawasaki", program, path); } catch { /* IO tabela ni ključna za .as */ }
        }

        // Natančna replika zaporedja pisanja iz ShellViewModel.GenerateProject (veja "Kawasaki Hidria"),
        // brez stranskih učinkov (Excel). Dokumentacijski odsek in "[END]" se (kot v Generate) ne pišeta.
        private static string RegenerateAs(ProgramModel robot)
        {
            var sb = new StringBuilder();
            sb.Append(Template.GetKawasakiTesting(robot));
            sb.Append(Template.GetKawasakiMain(robot));
            sb.Append(Template.GetKawasakiInit(robot));
            sb.Append(Template.GetKawasakiHoming(robot));
            sb.Append(Template.GetKawasakiMainTask(robot));
            sb.Append(Template.GetKawasakiMoveAway(robot));
            sb.Append(Template.GetKawasakiMoveOnStation(robot));
            sb.Append(Template.GetKawasakiGoHome(robot));
            for (int n = 1; n < robot.Stations.Count; n++)
                sb.Append(Template.GetKawasakiGoStation(robot, n));
            sb.Append(Template.GetKawasakiFunctions(robot));
            sb.Append(Template.GetKawasakiCalibration(robot));
            sb.Append(Template.GetKawasakiPoints(robot));
            sb.Append(Template.GetKawasakiIO(robot));
            sb.Append(Template.GetKawasakiIOhandling(robot));
            sb.Append(Template.GetKawasakiComment(robot));
            return sb.ToString();
        }
    }
}
