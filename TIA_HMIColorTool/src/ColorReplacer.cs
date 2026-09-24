using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TIA_HMIColorTool
{
    /// <summary>Ena uporabljena barva v exportu (za prikaz uporabniku).</summary>
    class UsedColor
    {
        public RgbColor Color;
        public string Role;          // ime XML elementa: BackColor, BorderColor, ForeColor, ...
        public int ActiveCount;      // dejansko vidne uporabe
        public int InactiveCount;    // BackColor pri Transparent / BorderColor pri BorderWidth=0
        public HashSet<string> Files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    class FileReplaceResult
    {
        public string File;
        public int Replacements;
        public Dictionary<RgbColor, int> PerColor = new Dictionary<RgbColor, int>();
    }

    /// <summary>
    /// Text-based zamenjava barv v exportiranih TIA HMI XML datotekah. Brez Siemens odvisnosti.
    /// Zamenja SAMO vsebino elementov, katerih ime se konča s "Color" (BackColor, BorderColor, ForeColor, ...),
    /// v enem prehodu (brez kaskade A->B->C), z ohranjenim formatom "R, G, B" in kodiranjem datoteke.
    /// </summary>
    static class ColorReplacer
    {
        // <BackColor>24, 106, 154</BackColor> - ločila med števili se ohranijo takšna, kot so.
        static readonly Regex ColorElementRx = new Regex(
            @"<(?<tag>\w*Color)>(?<pre>\s*)(?<r>\d{1,3})(?<s1>\s*,\s*)(?<g>\d{1,3})(?<s2>\s*,\s*)(?<b>\d{1,3})(?<post>\s*)</\k<tag>>",
            RegexOptions.Compiled);

        static readonly Regex RgbValueRx = new Regex(@"^\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*$", RegexOptions.Compiled);

        static readonly Regex BackupDirRx = new Regex(@"_backup_\d{8}_\d{6}$", RegexOptions.Compiled);

        /// <summary>Vse *.xml v mapi (rekurzivno), BREZ varnostnih kopij (*_backup_yyyyMMdd_HHmmss).</summary>
        public static string[] GetXmlFiles(string dir)
        {
            return Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)
                .Where(f => !Path.GetDirectoryName(MakeRelative(dir, f) ?? "")
                              .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                              .Any(seg => BackupDirRx.IsMatch(seg)))
                .ToArray();
        }

        // ------------------------------------------------------------------ Analiza

        public static List<UsedColor> Analyze(IList<string> files, Action<string> log)
        {
            var result = new Dictionary<string, UsedColor>();
            log("Analiza: " + files.Count + " XML datotek.");

            foreach (var file in files)
            {
                XDocument doc;
                try { doc = XDocument.Load(file); }
                catch (Exception ex) { log("  NAPAKA pri branju " + file + ": " + ex.Message); continue; }

                foreach (var el in doc.Descendants().Where(e => e.Name.LocalName.EndsWith("Color") && !e.HasElements))
                {
                    var m = RgbValueRx.Match(el.Value);
                    if (!m.Success) continue;
                    int r = int.Parse(m.Groups[1].Value), g = int.Parse(m.Groups[2].Value), b = int.Parse(m.Groups[3].Value);
                    if (r > 255 || g > 255 || b > 255) continue;

                    var color = new RgbColor(r, g, b);
                    string role = el.Name.LocalName;
                    string key = role + "|" + color;
                    UsedColor uc;
                    if (!result.TryGetValue(key, out uc))
                        result[key] = uc = new UsedColor { Color = color, Role = role };

                    if (IsActive(el)) uc.ActiveCount++; else uc.InactiveCount++;
                    uc.Files.Add(file);
                }
            }

            return result.Values
                .OrderByDescending(u => u.ActiveCount)
                .ThenBy(u => u.Role)
                .ToList();
        }

        /// <summary>
        /// BackColor ni viden pri BackFillStyle=Transparent, BorderColor ni viden pri BorderWidth=0.
        /// Lastnosti iščemo med sosednjimi elementi (isti AttributeList).
        /// </summary>
        static bool IsActive(XElement colorEl)
        {
            var parent = colorEl.Parent;
            if (parent == null) return true;
            string role = colorEl.Name.LocalName;

            if (role == "BackColor")
            {
                var fill = SiblingValue(parent, "BackFillStyle");
                if (fill != null && fill.Trim().Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            else if (role == "BorderColor")
            {
                var width = SiblingValue(parent, "BorderWidth");
                int w;
                if (width != null && int.TryParse(width.Trim(), out w) && w <= 0)
                    return false;
            }
            return true;
        }

        static string SiblingValue(XElement parent, string localName)
        {
            var el = parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
            return el == null ? null : el.Value;
        }

        // ------------------------------------------------------------------ Zamenjava

        /// <summary>
        /// Zamenja barve po mapiranju v podanih datotekah (vse pod <paramref name="dir"/>).
        /// Pri dryRun=false najprej kopira datoteke, ki se bodo spremenile, v &lt;dir&gt;_backup_&lt;čas&gt;
        /// (z ohranjeno relativno potjo).
        /// </summary>
        public static List<FileReplaceResult> Replace(string dir, IList<string> files, Dictionary<RgbColor, RgbColor> map,
            bool dryRun, Action<string> log)
        {
            log((dryRun ? "DRY-RUN" : "ZAMENJAVA") + ": " + files.Count + " XML datotek, " + map.Count + " parov barv.");

            // 1. prehod: izračun (nič ne pišemo)
            var pending = new List<Tuple<FileReplaceResult, string, Encoding>>();
            foreach (var file in files)
            {
                Encoding enc;
                string content = ReadWithEncoding(file, out enc);
                var res = new FileReplaceResult { File = file };

                string replaced = ColorElementRx.Replace(content, m =>
                {
                    int r = int.Parse(m.Groups["r"].Value), g = int.Parse(m.Groups["g"].Value), b = int.Parse(m.Groups["b"].Value);
                    if (r > 255 || g > 255 || b > 255) return m.Value;
                    var old = new RgbColor(r, g, b);
                    RgbColor nw;
                    if (!map.TryGetValue(old, out nw)) return m.Value;

                    res.Replacements++;
                    int c; res.PerColor.TryGetValue(old, out c); res.PerColor[old] = c + 1;
                    string tag = m.Groups["tag"].Value;
                    return "<" + tag + ">" + m.Groups["pre"].Value +
                           nw.R + m.Groups["s1"].Value + nw.G + m.Groups["s2"].Value + nw.B +
                           m.Groups["post"].Value + "</" + tag + ">";
                });

                if (res.Replacements > 0)
                    pending.Add(Tuple.Create(res, replaced, enc));
            }

            if (pending.Count == 0)
            {
                log("Ni najdenih barv za zamenjavo.");
                return new List<FileReplaceResult>();
            }

            // 2. varnostna kopija + pisanje
            if (!dryRun)
            {
                string backupDir = dir.TrimEnd('\\', '/') + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                foreach (var p in pending)
                {
                    string dest = Path.Combine(backupDir, MakeRelative(dir, p.Item1.File));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(p.Item1.File, dest, true);
                }
                log("Varnostna kopija (" + pending.Count + " datotek): " + backupDir);
            }

            foreach (var p in pending)
            {
                if (!dryRun)
                    File.WriteAllText(p.Item1.File, p.Item2, p.Item3);
                log("  " + MakeRelative(dir, p.Item1.File) + ": " + p.Item1.Replacements + " zamenjav (" +
                    string.Join("; ", p.Item1.PerColor.Select(kv => kv.Key + " -> " + map[kv.Key] + " x" + kv.Value)) + ")");
            }

            var totals = new Dictionary<RgbColor, int>();
            foreach (var p in pending)
                foreach (var kv in p.Item1.PerColor)
                {
                    int c; totals.TryGetValue(kv.Key, out c); totals[kv.Key] = c + kv.Value;
                }

            log("Skupaj po barvah:");
            foreach (var kv in map)
            {
                int c; totals.TryGetValue(kv.Key, out c);
                log("  " + kv.Key + " -> " + kv.Value + ": " + c + "x");
            }
            log((dryRun ? "DRY-RUN končan (nič zapisano)" : "Zamenjava končana") + ". Datotek: " + pending.Count +
                ", zamenjav: " + pending.Sum(p => p.Item1.Replacements));

            return pending.Select(p => p.Item1).ToList();
        }

        // ------------------------------------------------------------------ Pomožno

        /// <summary>Prebere datoteko in zazna kodiranje (BOM), da se zapiše nazaj v enakem.</summary>
        static string ReadWithEncoding(string file, out Encoding enc)
        {
            byte[] bytes = File.ReadAllBytes(file);
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                enc = new UTF8Encoding(true);
            else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                enc = new UnicodeEncoding(false, true);
            else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                enc = new UnicodeEncoding(true, true);
            else
                enc = new UTF8Encoding(false);

            int bomLen = enc.GetPreamble().Length;
            return enc.GetString(bytes, bomLen, bytes.Length - bomLen);
        }

        public static void CopyDirectory(string source, string dest)
        {
            Directory.CreateDirectory(dest);
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string destFile = Path.Combine(dest, MakeRelative(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                File.Copy(file, destFile, true);
            }
        }

        /// <summary>.NET 4.8 nima Path.GetRelativePath.</summary>
        public static string MakeRelative(string baseDir, string fullPath)
        {
            string b = Path.GetFullPath(baseDir).TrimEnd('\\', '/') + "\\";
            string f = Path.GetFullPath(fullPath);
            return f.StartsWith(b, StringComparison.OrdinalIgnoreCase) ? f.Substring(b.Length) : f;
        }
    }
}
