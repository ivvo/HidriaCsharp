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
    public class EpsonUpdateException : Exception
    {
        public EpsonUpdateException(string message) : base(message) { }
    }

    // Posodobitev Epson projekta (V3) z na novo dodanimi postajami.
    //
    // V3-jev Epson generator se strukturno razlikuje od referenčnega (robot_inStation, gripper/testAll,
    // homing z "For i/For j", shema premikov prek pAboveStation), zato kirurško vstavljanje po odsekih
    // ni prenosljivo. Namesto tega celoten projekt RE-GENERIRAMO iz modela (z že dodanimi postajami) -
    // enako kot Generate - in NAUČENE koordinate v ".pts" OHRANIMO tako, da jih po IMENU točke
    // (sLabel="p<Postaja>") prekrijemo nazaj. Tako je koda vedno pravilna in "na način V3", naučene
    // koordinate pa se ne izgubijo.
    //
    // Opomba/omejitev: koda se re-generira, zato morebitni ročni popravki v generirani .prg kodi ob
    // posodobitvi niso ohranjeni; naučene koordinate (.pts) pa SO. Slog "simple program" je v V3 privzet
    // (brez UI stikala), stanje "simulation" pa se zazna iz uvožene datoteke. En program na projekt.
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

        public static void UpdateProgram(ProgramModel program, List<StationModel> newStations, string path)
        {
            if (newStations == null || newStations.Count == 0) return;

            // Ciljaj robotsko datoteko "<program>.prg" (v mapi so lahko še Main.prg in pomožni programi
            // kot getOrientation.prg/HandIO.prg, ki jih NE spreminjamo).
            string progLower = program.ProgramName.ToLower();
            string prgPath = Directory.GetFiles(path, "*.prg")
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(progLower, StringComparison.OrdinalIgnoreCase))
                ?? Directory.GetFiles(path, "*.prg")
                    .FirstOrDefault(f => !Path.GetFileNameWithoutExtension(f).Equals("Main", StringComparison.OrdinalIgnoreCase));
            if (prgPath == null)
                throw new EpsonUpdateException($"V mapi '{path}' ni robotske .prg datoteke - ali je bila izbrana pravilna mapa projekta?");

            // Slog "simulation" zaznaj iz obstoječega .prg (edina razlika, ki jo prinese: "Or 1 = 1" v homingu).
            string oldPrg = File.ReadAllText(prgPath);
            bool simulation = oldPrg.Contains("Or 1 = 1");

            // Naučene koordinate obstoječih točk po imenu (sLabel). Ciljamo natanko primarno točkovno
            // datoteko "<program>1.pts" (projekt ima lahko še druge, npr. robot2.pts, ki jih ne diramo).
            string preferredPts = program.ProgramName.ToLower() + "1.pts";
            string ptsPath = Directory.GetFiles(path, "*.pts")
                .FirstOrDefault(f => Path.GetFileName(f).Equals(preferredPts, StringComparison.OrdinalIgnoreCase))
                ?? Directory.GetFiles(path, "*.pts").FirstOrDefault();
            var taughtByLabel = ptsPath != null ? ParsePtsByLabel(File.ReadAllText(ptsPath)) : new Dictionary<string, string>();

            // Re-generiraj vse Epson datoteke iz modela (z novimi postajami) v ISTO mapo.
            var progs = new ObservableCollection<ProgramModel> { program };

            File.WriteAllText(Path.Combine(path, "Main.prg"),
                Template.GetMainFunc(progs) + Template.GetErrorHandlingFunc(progs) + Template.GetEstopHandlingFunc(progs));

            var sb = new StringBuilder();
            sb.Append(Template.GetHeader(program));
            sb.Append(Template.GetPalletsDefinitions(program));
            sb.Append(Template.GetInitFunc(program));
            sb.Append(Template.GetHomingFunc(program, simulation));
            sb.Append(Template.GetTestFunc(program));
            sb.Append(Template.GetOperationFunc(program));
            // V3 privzeto generira "simple program" (brez UI stikala za drugačen slog).
            sb.Append(Template.GetMovementsSimpleProgramFunc(program));
            sb.Append(Template.GetMoveOnStationSimpleProgramFunc(program));
            sb.Append(Template.GetMoveAwaySimpleProgramFunc(program));
            sb.Append(Template.GetPowerModeFunc(program));
            sb.Append(Template.GetFullPowerModeFunc(program));
            sb.Append(Template.GetSlowPowerModeFunc(program));
            sb.Append(Template.GetCpBarrierFunc(program));
            sb.Append(Template.GetStationsFreeFunc(program));
            sb.Append(Template.GetStationsBusyFunc(program));
            sb.Append(Template.GetResetDepartLocationsFunc(program));
            sb.Append(Template.GetResetDestLocationsFunc(program));
            sb.Append(Template.GetResetProfibusOutputsFunc(program));
            sb.Append(Template.GetResetFlagsFunc(program));
            File.WriteAllText(Path.Combine(path, $"{program.ProgramName.ToLower()}.prg"), sb.ToString());

            File.WriteAllText(Path.Combine(path, $"{program.ProgramName}.io"), Template.GetAllIOLabels(program));
            File.WriteAllText(Path.Combine(path, "IOLabels.dat"), Template.GetIOLablesFunc(program));
            File.WriteAllText(Path.Combine(path, "UserErrors.dat"), Template.GetUserError());

            // .pts: re-generiraj (ničle) in prekrij z naučenimi koordinatami po imenu točke.
            string regenPts = Template.GeneratePointsFunc(program);
            regenPts = OverlayTaughtPts(regenPts, taughtByLabel);
            string ptsOut = ptsPath ?? Path.Combine(path, $"{program.ProgramName}1.pts");
            File.WriteAllText(ptsOut, regenPts);

            try { new GenerateExcelIO().GenerateIO("Epson", program, path); } catch { /* IO tabela ni ključna za .prg */ }
        }

        #region ////// .pts - ohrani naučene koordinate po imenu (sLabel) //////

        // Blok točke: "Point<k> {\r\n\tnNumber=<n>\r\n<rest ...>\r\n}". "rest" (od sLabel do "}") vsebuje
        // vse naučene vrednosti; ključ je sLabel.
        private static readonly Regex PointBlockRegex = new Regex(
            @"(?<head>Point\d+\s*\{\r?\n\tnNumber=\d+\r?\n)(?<rest>[\s\S]*?\r?\n\})",
            RegexOptions.Compiled);
        private static readonly Regex LabelRegex = new Regex("sLabel=\"(?<lbl>[^\"]*)\"", RegexOptions.Compiled);

        private static Dictionary<string, string> ParsePtsByLabel(string ptsContent)
        {
            var map = new Dictionary<string, string>();
            foreach (Match m in PointBlockRegex.Matches(ptsContent))
            {
                var lbl = LabelRegex.Match(m.Groups["rest"].Value);
                if (lbl.Success) map[lbl.Groups["lbl"].Value] = m.Groups["rest"].Value;
            }
            return map;
        }

        private static string OverlayTaughtPts(string regenPts, Dictionary<string, string> taughtByLabel)
        {
            if (taughtByLabel.Count == 0) return regenPts;
            return PointBlockRegex.Replace(regenPts, m =>
            {
                var lbl = LabelRegex.Match(m.Groups["rest"].Value);
                if (lbl.Success && taughtByLabel.TryGetValue(lbl.Groups["lbl"].Value, out var taughtRest))
                    return m.Groups["head"].Value + taughtRest; // ohrani na novo generirano glavo (index) + naučeno telo
                return m.Value;
            });
        }

        #endregion
    }
}
