using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>En element DB-ja z odmikom (bit = -1 za ne-Bool).</summary>
    public sealed class DbLeaf
    {
        public string Path;        // "TURCK_RIGHT.READED_STRING", "OFF[49]"
        public string Type;        // "Bool", "Int", "String[20]" ...
        public int Byte;
        public int Bit = -1;
        public int Size;           // bajti (Bool: 0)
        public bool IsBool => Bit >= 0;
        public string Address => IsBool ? "DBX" + Byte + "." + Bit : (Size == 1 ? "DBB" : Size == 2 ? "DBW" : Size == 4 ? "DBD" : "DB?") + Byte;
        public override string ToString() => Address + " " + Path + " : " + Type;
    }

    /// <summary>
    /// Odmiki članov DB-ja s postavitvijo "Standard" (ne-optimiziran, kot S7-300/400): Bool-i se pakirajo po bitih, 1-bajtni tipi
    /// na bajt, vse ostalo na sodi bajt; Struct/UDT/instanca FB in polja začnejo na sodem bajtu in se zaključijo na sodem bajtu;
    /// v FB instanci se vsak razdelek (Input/Output/InOut/Static) začne na sodem bajtu, InOut strukturiranega tipa je 6-bajtni kazalec.
    /// Optimizirani DB-ji nimajo absolutnih naslovov - zanje vrne null.
    /// </summary>
    public sealed class DbLayout
    {
        public Block Db;
        public List<DbLeaf> Leaves = new List<DbLeaf>();
        public int Size;
        /// <summary>false = naletel na neznan tip; odmiki za njim niso znani (Leaves vsebuje samo del pred njim).</summary>
        public bool Complete = true;
        public string Problem;
        /// <summary>
        /// Odmik, od katerega naprej postavitev ni zanesljiva (-1 = vsa je zanesljiva). Neumerjeni konstrukti: InOut strukturiranega tipa
        /// (String/Struct kot kazalec) in R_TRIG/F_TRIG. Umerjeno na HMI tagih 122A: globalni DB-ji, IEC_TIMER = 16 B, CTD_INT, InOut osnovnih tipov.
        /// </summary>
        public int UncertainFrom = -1;
        public bool IsCertain(int byteOff) => (Complete || byteOff < Size) && (UncertainFrom < 0 || byteOff < UncertainFrom);
        static readonly HashSet<string> Uncalibrated = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "R_TRIG", "F_TRIG" };

        static readonly Dictionary<string, int> Sizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Byte"] = 1, ["Char"] = 1, ["SInt"] = 1, ["USInt"] = 1,
            ["Word"] = 2, ["Int"] = 2, ["UInt"] = 2, ["S5Time"] = 2, ["Date"] = 2, ["WChar"] = 2, ["Timer"] = 2, ["Counter"] = 2,
            ["Block_DB"] = 2, ["Block_FB"] = 2, ["Block_FC"] = 2, ["DB_ANY"] = 2,
            ["DWord"] = 4, ["DInt"] = 4, ["UDInt"] = 4, ["Real"] = 4, ["Time"] = 4, ["Time_Of_Day"] = 4, ["TOD"] = 4,
            ["LReal"] = 8, ["LInt"] = 8, ["ULInt"] = 8, ["LWord"] = 8, ["LTime"] = 8, ["Date_And_Time"] = 8, ["DT"] = 8,
            ["LTime_Of_Day"] = 8, ["LTOD"] = 8, ["LDT"] = 8,
            ["DTL"] = 12, ["Pointer"] = 6, ["Any"] = 10,
            // IEC časovniki: export pokaže le PT/ET/IN/Q, v standardnem pomnilniku pa zasedejo 16 B (LTIME različice 32 B)
            ["IEC_TIMER"] = 16, ["TON_TIME"] = 16, ["TOF_TIME"] = 16, ["TP_TIME"] = 16, ["TONR_TIME"] = 16,
            ["TON"] = 16, ["TOF"] = 16, ["TP"] = 16, ["TONR"] = 16,
            ["IEC_LTIMER"] = 32, ["TON_LTIME"] = 32, ["TOF_LTIME"] = 32, ["TP_LTIME"] = 32, ["TONR_LTIME"] = 32,
        };
        static readonly Regex ArrayRx = new Regex(@"^Array\[(.+?)\]\s+of\s+(.+)$", RegexOptions.IgnoreCase);
        static readonly Regex StringRx = new Regex(@"^(W?String)(?:\[(\d+)\])?$", RegexOptions.IgnoreCase);
        static readonly HashSet<string> SkipSections = new HashSet<string> { "Temp", "Constant", "Return" };

        sealed class UnknownType : Exception { public UnknownType(string t) : base(t) { } }

        readonly PlcProgram _plc;
        int _byte, _bit;

        DbLayout(PlcProgram plc) { _plc = plc; }

        static readonly Dictionary<Block, DbLayout> Cache = new Dictionary<Block, DbLayout>();

        public static DbLayout For(PlcProgram plc, Block db)
        {
            if (db == null || db.Interface == null) return null;
            string layout = db.MemoryLayout;
            if (string.IsNullOrEmpty(layout) && db.InstanceOfName != null)
                layout = plc.Blocks.FirstOrDefault(b => b.Kind == BlockKind.FB && b.Name == db.InstanceOfName)?.MemoryLayout;
            if (!string.Equals(layout, "Standard", StringComparison.OrdinalIgnoreCase)) return null;
            lock (Cache)
            {
                if (Cache.TryGetValue(db, out var cached)) return cached;
                var l = new DbLayout(plc) { Db = db };
                try { l.Sections(db.Interface, ""); l.AlignEven(); }
                catch (UnknownType ex) { l.Complete = false; l.Problem = "neznan tip " + ex.Message; }
                l.Size = l._byte;
                Cache[db] = l;
                return l;
            }
        }

        /// <summary>Elementi, ki pokrivajo naslov (bajt, bit ali -1, širina v bajtih).</summary>
        public List<DbLeaf> At(int byteOff, int bit, int width)
        {
            int lo = byteOff, hi = byteOff + Math.Max(width, 1) - 1;
            return Leaves.Where(x => x.IsBool
                    ? x.Byte >= lo && x.Byte <= hi && (bit < 0 || x.Bit == bit)
                    : x.Byte <= hi && x.Byte + x.Size - 1 >= lo).ToList();
        }

        void AlignByte() { if (_bit > 0) { _byte++; _bit = 0; } }
        void AlignEven() { AlignByte(); if ((_byte & 1) == 1) _byte++; }

        static string L(XElement e) => e.Name.LocalName;

        void Sections(XElement sections, string prefix)
        {
            foreach (var sec in sections.Elements().Where(x => L(x) == "Section"))
            {
                string name = (string)sec.Attribute("Name");
                if (SkipSections.Contains(name)) continue;
                AlignEven();
                Members(sec.Elements().Where(x => L(x) == "Member"), prefix, name == "InOut");
            }
        }

        void Members(IEnumerable<XElement> members, string prefix, bool inOut)
        {
            foreach (var m in members)
            {
                string name = (string)m.Attribute("Name");
                string dt = ((string)m.Attribute("Datatype") ?? "").Trim();
                string path = prefix.Length == 0 ? name : prefix + "." + name;
                var am = ArrayRx.Match(dt);
                if (inOut && (am.Success || IsStructured(m, dt))) { AlignEven(); Uncertain(); Leaf(path, "InOut " + dt, 6); continue; }
                if (Uncalibrated.Contains(dt)) { AlignEven(); Uncertain(); }
                if (am.Success) Array(m, path, am.Groups[1].Value, am.Groups[2].Value.Trim());
                else Type(m, dt, path);
            }
        }

        bool IsStructured(XElement m, string dt) =>
            dt == "Struct" || dt.StartsWith("\"") || StringRx.IsMatch(dt) || m.Elements().Any(x => L(x) == "Sections" || L(x) == "Member");

        void Array(XElement m, string path, string dims, string elemType)
        {
            var ranges = dims.Split(',').Select(d =>
            {
                var p = d.Split(new[] { ".." }, StringSplitOptions.None);
                return (lo: int.Parse(p[0].Trim()), hi: int.Parse(p[1].Trim()));
            }).ToList();
            var indices = new List<string> { "" };
            foreach (var r in ranges)
                indices = indices.SelectMany(i => Enumerable.Range(r.lo, r.hi - r.lo + 1).Select(k => i.Length == 0 ? k.ToString() : i + "," + k)).ToList();

            AlignEven();
            if (elemType.Equals("Bool", StringComparison.OrdinalIgnoreCase))
                foreach (var i in indices) Bool(path + "[" + i + "]");
            else if (Sizes.TryGetValue(elemType, out int s) && s == 1)
                foreach (var i in indices) Leaf(path + "[" + i + "]", elemType, 1);
            else
                foreach (var i in indices) { AlignEven(); Type(m, elemType, path + "[" + i + "]"); }
            AlignEven();
        }

        void Type(XElement m, string dt, string path)
        {
            if (dt.Equals("Bool", StringComparison.OrdinalIgnoreCase)) { Bool(path); return; }
            if (Sizes.TryGetValue(dt, out int size))
            {
                if (size == 1) AlignByte(); else AlignEven();
                Leaf(path, dt, size);
                return;
            }
            var sm = StringRx.Match(dt);
            if (sm.Success)
            {
                int n = sm.Groups[2].Success ? int.Parse(sm.Groups[2].Value) : 254;
                AlignEven();
                Leaf(path, dt, sm.Groups[1].Value.Equals("WString", StringComparison.OrdinalIgnoreCase) ? 2 * n + 4 : n + 2);
                return;
            }
            // Struct / UDT / instanca FB / sistemski tip - razširjen v exportu (Sections ali Member)
            var nested = m.Elements().FirstOrDefault(x => L(x) == "Sections");
            var direct = m.Elements().Where(x => L(x) == "Member").ToList();
            XElement fromTypes = null;
            if (nested == null && direct.Count == 0 && dt.StartsWith("\"")) _plc.Types.TryGetValue(dt.Trim('"'), out fromTypes);
            if (nested == null && direct.Count == 0 && fromTypes == null) throw new UnknownType(dt + " (" + path + ")");
            AlignEven();
            if (nested != null) Sections(nested, path);
            else if (fromTypes != null) Sections(fromTypes, path);
            else Members(direct, path, false);
            AlignEven();
        }

        void Uncertain() { if (UncertainFrom < 0) UncertainFrom = _byte; }

        void Bool(string path)
        {
            Leaves.Add(new DbLeaf { Path = path, Type = "Bool", Byte = _byte, Bit = _bit });
            if (++_bit == 8) { _byte++; _bit = 0; }
        }

        void Leaf(string path, string type, int size)
        {
            Leaves.Add(new DbLeaf { Path = path, Type = type, Byte = _byte, Size = size });
            _byte += size;
        }
    }
}
