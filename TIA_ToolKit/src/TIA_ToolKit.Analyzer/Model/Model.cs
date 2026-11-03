using System.Collections.Generic;
using System.Linq;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>En izvožen TIA projekt (mapa z manifest.json, PLC\..., HMI\...).</summary>
    public sealed class ExportProject
    {
        public string Root;
        public string Name;
        public string ExportedAt;
        public List<PlcProgram> Plcs = new List<PlcProgram>();
        public List<string> HmiRoots = new List<string>();
        public List<HmiTarget> Hmis = new List<HmiTarget>();
        /// <summary>Kar export ni mogel izvoziti (know-how zaščita, nekonsistentne watch tabele ...) - za razdelek "Česa ni bilo mogoče preveriti".</summary>
        public List<string> NotExported = new List<string>();
    }

    public sealed class PlcProgram
    {
        public string Name;
        public string Root;
        public List<Block> Blocks = new List<Block>();
        public List<PlcTag> Tags = new List<PlcTag>();
        /// <summary>PLC podatkovni tipi (UDT): ime → element Sections. Rezerva, če DB člana tipa ne razširi.</summary>
        public Dictionary<string, System.Xml.Linq.XElement> Types = new Dictionary<string, System.Xml.Linq.XElement>();
    }

    public enum BlockKind { OB, FB, FC, GlobalDB, InstanceDB, Other }

    public sealed class Block
    {
        public string Name;
        public BlockKind Kind;
        public int Number;
        public string Language;
        /// <summary>Mapa v drevesu "Program blocks", npr. "Machine/Rotor/1_R_GT2_Robot".</summary>
        public string Folder;
        public List<Network> Networks = new List<Network>();
        public string SclSource;   // celoten SCL vir (iz Sources\), če obstaja
        /// <summary>Samo DB: vmesnik (element Sections) in postavitev "Standard"/"Optimized" - za izračun odmikov (DbLayout).</summary>
        public System.Xml.Linq.XElement Interface;
        public string MemoryLayout;
        public string InstanceOfName;   // instance DB: ime FB-ja (postavitev se podeduje od FB)

        public bool IsFailsafe => Language != null && Language.StartsWith("F_");
        public bool IsDb => Kind == BlockKind.GlobalDB || Kind == BlockKind.InstanceDB;
        /// <summary>"FB20", "DB1010", "OB1" - kot v TIA.</summary>
        public string TiaId => (IsDb ? "DB" : Kind.ToString()) + Number;
        public string DisplayPath => (string.IsNullOrEmpty(Folder) ? "" : Folder + "/") + Name + " [" + TiaId + "]";
        public override string ToString() => DisplayPath;
    }

    public sealed class Network
    {
        public int Index;          // 1-based, kot številka networka v TIA
        public string Title = "";
        public string Language;
        public List<StlStatement> Stl = new List<StlStatement>();
        public LadNetwork Lad;     // LAD/FBD/F_LAD
        public string Scl;         // SCL network kot tekst
        /// <summary>Vse globalne reference v networku (tudi parametri klicev in SCL), za navzkrižne reference (XREF).</summary>
        public List<string> Refs = new List<string>();

        public IEnumerable<string> AllOperands()
        {
            foreach (var s in Stl) if (s.Operand != null) yield return s.Operand;
            if (Lad != null) foreach (var p in Lad.Parts.Values) if (p.Operand != null) yield return p.Operand;
        }
    }

    public sealed class StlStatement
    {
        public string Op;          // "A", "AN", "O", "S", "R", "Assign", "L", "T", "SET", "CALL", "COMMENT", "EMPTY_LINE", ...
        public string Operand;     // "\"Global.Member\"", "#Local", konstanta ali null
        public override string ToString() => Op + (Operand == null ? "" : " " + Operand);
    }

    /// <summary>LAD/FBD omrežje kot graf delov in povezav.</summary>
    public sealed class LadNetwork
    {
        public Dictionary<string, LadPart> Parts = new Dictionary<string, LadPart>();
        /// <summary>Žica: vir (pin dela ali "PWR") -> cilji.</summary>
        public List<LadWire> Wires = new List<LadWire>();

        /// <summary>Dejanski vhod dela (ime pina "in"): vrne "PWR", drug del ali null.</summary>
        public LadEnd SourceOf(string uid, string pin = "in")
        {
            foreach (var w in Wires)
                if (w.Targets.Any(t => t.Uid == uid && t.Pin == pin)) return w.Source;
            return null;
        }
    }

    public sealed class LadPart
    {
        public string Uid;
        public string Name;        // "Contact", "Coil", "SCoil", "RCoil", "O", "Call ..." ...
        public bool Negated;
        public string Operand;     // operand kontakta/tuljave
        public bool IsCoil => Name == "Coil" || Name == "SCoil" || Name == "RCoil";
    }

    public sealed class LadWire
    {
        public LadEnd Source;
        public List<LadEnd> Targets = new List<LadEnd>();
    }

    public sealed class LadEnd
    {
        public bool Powerrail;
        public string Uid;
        public string Pin;
        public static readonly LadEnd Pwr = new LadEnd { Powerrail = true };
    }

    public sealed class PlcTag
    {
        public string Name;
        public string DataType;
        public string Address;     // "%M14.3", "%IW100" ...
        public string Table;
        public char Area;          // 'I', 'Q', 'M' ali '\0'
        public int BitLo = -1, BitHi = -1;
    }
}
