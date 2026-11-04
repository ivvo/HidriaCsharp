using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TIA_Generator.Model;

namespace TIA_Generator.Extract
{
    /// <summary>
    /// Slovar besedil (odločitev 2026-11-04: slovar po imenu člana + ročni popravek v modelu).
    /// 1) cela fraza (ime člana) → izvor "slovar"; 2) vse besede imena znane → sestavljen predlog, izvor "sestavljeno";
    /// 3) sicer ime člana s presledki v vseh jezikih, izvor "manjka" (inženir dopolni).
    /// Vgrajeni Data\slovar.json + neobvezne lokalne datoteke (dopolnijo/prepišejo vgrajene vnose).
    /// </summary>
    public sealed class TextDictionary
    {
        static readonly string[] Langs = { "sl-SI", "en-US", "de-DE" };
        readonly Dictionary<string, string[]> _phrases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, string[]> _words = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        public int PhraseCount => _phrases.Count;
        public int WordCount => _words.Count;

        public static TextDictionary LoadDefault(IEnumerable<string> extraFiles = null)
        {
            var d = new TextDictionary();
            using (var s = typeof(TextDictionary).Assembly.GetManifestResourceStream("TIA_Generator.slovar.json"))
            using (var r = new StreamReader(s, Encoding.UTF8))
                d.Merge(r.ReadToEnd());
            if (extraFiles != null)
                foreach (var f in extraFiles.Where(File.Exists))
                    d.Merge(File.ReadAllText(f, Encoding.UTF8));
            return d;
        }

        public void Merge(string json)
        {
            var o = JObject.Parse(json);
            void Load(string key, Dictionary<string, string[]> into)
            {
                if (!(o[key] is JObject obj)) return;
                foreach (var p in obj.Properties())
                    if (p.Value is JArray a) into[p.Name] = a.Select(x => (string)x ?? "").ToArray();
            }
            Load("phrases", _phrases);
            Load("words", _words);
        }

        /// <summary>Razbije ime člana na besede: podčrtaji in camelCase ("Delay_GripperClose" → Delay, Gripper, Close).</summary>
        public static List<string> Tokens(string name) =>
            Regex.Split(Regex.Replace(name ?? "", "([a-z])([A-Z])", "$1_$2"), "[_\\.\\s]+").Where(t => t.Length > 0).ToList();

        /// <summary>Besedilo za ime člana. ignoreTokens = besede, ki se izpustijo (npr. oznaka enote B02, 1 pri alarmih).</summary>
        public (LangText text, string source) Lookup(string member, Func<string, bool> ignoreToken = null)
        {
            string leaf = member.Contains('.') ? member.Substring(member.LastIndexOf('.') + 1) : member;
            if (_phrases.TryGetValue(member, out var ph) || _phrases.TryGetValue(leaf, out ph)) return (ToText(ph), TextSource.Dictionary);

            var tokens = Tokens(member).Where(t => ignoreToken == null || !ignoreToken(t)).ToList();
            if (tokens.Count == 0) return (Same(member), TextSource.Missing);
            var parts = new List<string[]>();
            bool allKnown = true;
            foreach (var t in tokens)
            {
                if (_words.TryGetValue(t, out var w)) parts.Add(w);
                else if (Regex.IsMatch(t, "^\\d+$")) parts.Add(new[] { t, t, t });
                else { allKnown = false; parts.Add(new[] { t, t, t }); }
            }
            if (!allKnown) return (Same(string.Join(" ", tokens)), TextSource.Missing);
            var res = new string[3];
            for (int i = 0; i < 3; i++)
                res[i] = Capitalize(string.Join(" ", parts.Select(p => p.Length > i ? p[i] : p[0]).Where(s => s.Length > 0)));
            return (ToText(res), TextSource.Composed);
        }

        static LangText ToText(string[] a) => new LangText(a.ElementAtOrDefault(0), a.ElementAtOrDefault(1) ?? a[0], a.ElementAtOrDefault(2) ?? a[0]);
        static LangText Same(string s) { s = s.Replace('_', ' '); return new LangText(s, s, s); }
        static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
