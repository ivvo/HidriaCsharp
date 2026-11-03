using System.Collections.Generic;

namespace TIA_ToolKit.Analyzer
{
    public static class Categories
    {
        public const string Safety = "Safety";
        public const string Functional = "Funkcionalno";
        public const string CopyIndex = "Kopiranje / indeksi";
        public const string Helpers = "Pomožni bloki / OB / SCADA";
        public const string Tags = "PLC tagi";
        public const string Hmi = "HMI";

        /// <summary>Kategorija → črka ID-ja (S1, F1, C1, H1, T1, M1).</summary>
        public static string Prefix(string category)
        {
            switch (category)
            {
                case Safety: return "S";
                case Functional: return "F";
                case CopyIndex: return "C";
                case Helpers: return "H";
                case Tags: return "T";
                case Hmi: return "M";
                default: return "X";
            }
        }

        public static readonly string[] Order = { Safety, Functional, CopyIndex, Helpers, Tags, Hmi };
    }

    public enum Severity { Critical = 1, High = 2, Medium = 3, Low = 4 }

    public sealed class Finding
    {
        public string Id;               // dodeli poročilo (S1, F1 ...)
        public string RuleId;           // npr. "SAF-ACK-REI"
        public string Category;
        public Severity Severity;
        /// <summary>"Preverjeno" ali "Preveriti" (pravilo je našlo kandidata, namen presodi človek/LLM).</summary>
        public string Confidence = "Preverjeno";
        public string Title;
        public string Description;
        public List<FindingLocation> Locations = new List<FindingLocation>();

        /// <summary>Ključ za ohranjanje Statusa/Opombe med zagoni (ID se lahko premakne, ta ključ ne).</summary>
        public string StableKey
        {
            get
            {
                var parts = new List<string> { RuleId };
                foreach (var l in Locations) parts.Add(l.Path + "#" + l.Networks);
                return string.Join("|", parts);
            }
        }
    }

    public sealed class FindingLocation
    {
        public string Path;             // "Machine/Rotor/1_R_GT2_Robot/Unit_R_GT2_Robot [FB20]" ali HMI datoteka
        public string Networks = "";    // "334" ali "13, 20–36"
        public string NetworkTitle = "";

        public static FindingLocation Of(Block b, params Network[] nws)
        {
            var loc = new FindingLocation { Path = b.DisplayPath };
            var nums = new List<string>(); var titles = new List<string>();
            foreach (var n in nws)
            {
                if (n == null) continue;
                if (!nums.Contains(n.Index.ToString())) nums.Add(n.Index.ToString());
                if (!string.IsNullOrEmpty(n.Title) && !titles.Contains(n.Title) && titles.Count < 3) titles.Add(n.Title);
            }
            loc.Networks = string.Join(", ", nums);
            loc.NetworkTitle = string.Join(" | ", titles);
            return loc;
        }
    }
}
