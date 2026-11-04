using System.Collections.Generic;
using Newtonsoft.Json;

namespace TIA_Generator.Model
{
    // Model stroja za generator HMI (HMI_GENERATOR_SPEC.md, pogl. 4.1).
    // Generator ga izpelje iz TIA exporta, inženir ga dopolni (besedila, showOn ...), nato iz njega nastanejo XML datoteke za uvoz.
    // Imena lastnosti v JSON so camelCase (glej ModelJson). Polja "_vir" povedo, od kod je vrednost.

    /// <summary>Besedilo v jezikih stroja (sl-SI, en-US, de-DE). Ključ = koda jezika.</summary>
    public sealed class LangText : SortedDictionary<string, string>
    {
        public LangText() { }
        public LangText(string sl, string en, string de)
        {
            if (sl != null) this["sl-SI"] = sl;
            if (en != null) this["en-US"] = en;
            if (de != null) this["de-DE"] = de;
        }
    }

    /// <summary>Izvor besedila: "slovar" (cela fraza v slovarju), "sestavljeno" (iz besed – predlog, preveri), "manjka" (ročni vnos), "rocno".</summary>
    public static class TextSource
    {
        public const string Dictionary = "slovar";
        public const string Composed = "sestavljeno";
        public const string Missing = "manjka";
        public const string Manual = "rocno";
    }

    public sealed class MachineModelFile
    {
        public string SchemaVersion = "0.1";
        public string Generator;
        public string GeneratedAt;
        public string SourceExport;
        public Machine Machine = new Machine();
        public List<Unit> Units = new List<Unit>();
        public GlobalData Global = new GlobalData();
        /// <summary>Opozorila izpeljave (indeks izven razpona standarda, časovniki niso polje, manjkajoči DB ...).</summary>
        public List<ModelWarning> Warnings = new List<ModelWarning>();
        public ModelStatistics Statistics = new ModelStatistics();
    }

    public sealed class Machine
    {
        public string Name;
        public string Project;
        /// <summary>"single" (Unit_01 …) ali "parts" (Unit_A01 …).</summary>
        public string Type;
        public List<string> Parts = new List<string>();
        /// <summary>Opisi sektorjev (SL), npr. A → "Montaža" – iz konfiguratorja strukture (za predlogo Nastavitve_Sektor_&lt;del&gt;).</summary>
        public SortedDictionary<string, string> PartTexts = new SortedDictionary<string, string>();
        public List<string> Lines = new List<string>();
        public List<string> Languages = new List<string> { "sl-SI", "en-US", "de-DE" };
        public PlcRef Plc = new PlcRef();
        public List<HmiDevice> HmiDevices = new List<HmiDevice>();
        public DbMap DbMap = new DbMap();
    }

    public sealed class PlcRef
    {
        public string Name;
        [JsonProperty("_vir")] public string Source;
    }

    public sealed class HmiDevice
    {
        /// <summary>"PC" ali "Panel".</summary>
        public string Kind;
        public string Target;
        public string Model;
        public string Resolution;
        public string Connection;
        [JsonProperty("_vir")] public string Source;
    }

    public sealed class DbMap
    {
        public string Counters;
        public string ServiceSettings;
        public string ErrorMessages;
        public string Takt;
        public string Recipe;
        public string CylinderControl;
        /// <summary>Ime bloka za vsako vlogo (npr. serviceSettings → DB_Service_Settings).</summary>
        public SortedDictionary<string, string> Names = new SortedDictionary<string, string>();
        [JsonProperty("_vir")] public string Source = "ime DB-ja (DB_Service_Settings, DB_Error_Messages, CYLINDER_CONTROL_DB ...) + številka iz Blocks XML";
    }

    public sealed class Unit
    {
        public string Id;
        /// <summary>"export" (izpeljano iz PLC) ali "struktura" (nova postaja iz konfiguratorja – zanjo se generira PLC ogrodje).</summary>
        public string Origin;
        public string Fb;
        public string Part;
        /// <summary>Skupina iz mape, kadar ni del stroja (npr. "Dosing" za Machine/Dosing/D1_Electrode).</summary>
        public string Group;
        public string Parent;
        public int? SubIndex;
        public string Line;
        public string Folder;
        /// <summary>Zaporedna številka iz imena mape "Unit 2 020 - …" (PHEV Stator); v DB4000 se pojavi kot "U_02_".</summary>
        public int? FolderNo;
        public LangText Text = new LangText();
        public string TextSource;
        public int? PlcIndex;
        public int? HmiIndex;
        /// <summary>Indeks nadrejene postaje, s katerim FB bere le OFF/ENABLE_HMI_COMMAND/RESET_OFF (podpostaje, dozirni moduli).</summary>
        public int? InheritsOffFromIndex;
        [JsonProperty("_vir_index")] public string IndexSource;
        public List<string> ShowOn = new List<string>();
        public SettingsDb SettingsDb;
        /// <summary>"array" (standard 2.0: Array[0..9] of S5Time) ali "members" (posamezni člani S5Time – stari projekti).</summary>
        public string TimerLayout;
        public List<TimerDef> Timers = new List<TimerDef>();
        public UnitHmiTags HmiTags = new UnitHmiTags();
        public PartBadCount PartBadCount;
        public List<ParamDef> Parameters = new List<ParamDef>();
        public List<ParamDef> RecipeParameters = new List<ParamDef>();
        public List<CylinderDef> Cylinders = new List<CylinderDef>();
        public List<string> StationFunctions = new List<string>();
        public List<string> Alarms = new List<string>();
        public List<string> CustomScreens = new List<string>();
    }

    public sealed class SettingsDb
    {
        public string Name;
        public int Number;
        public string Layout;
    }

    public sealed class TimerDef
    {
        public int Index;
        public string Member;
        /// <summary>Pot elementa po standardu 2.0 (Delay[i]); pri starih projektih nov naziv, ki zamenja Member.</summary>
        public string ArrayElement;
        /// <summary>Bloki, ki v PLC uporabljajo Member (za preimenovanje Member → ArrayElement); samo pri TimerLayout=members.</summary>
        public List<string> UsedIn;
        public bool Show;
        public LangText Text;
        public string TextSource;
    }

    public sealed class UnitHmiTags
    {
        public HmiTagDef Timers;
    }

    public sealed class HmiTagDef
    {
        public string Pc;
        public string Panel;
        public string PlcTag;
        public string DataType;
        /// <summary>true = PlcTag še ne obstaja; v PLC je treba posamezne člane S5Time zamenjati s poljem (odločitev 2026-11-04).</summary>
        public bool RequiresPlcChange;
        public string Note;
    }

    public sealed class PartBadCount
    {
        public string Member;
        public string DataType;
        public LangText Text;
    }

    public sealed class ParamDef
    {
        /// <summary>Pot člana v DB-ju (struct člani s piko: CRIMP_POSITION.Value_Max).</summary>
        public string Member;
        public string DataType;
        public string Unit;
        /// <summary>"input" (temno modro polje – vnos) ali "display" (turkizno – prikaz). Hevristika po imenu, preveri.</summary>
        public string Access;
        public bool Recipe;
        public double? Min;
        public double? Max;
        public string PlcTag;
        public LangText Text;
        public string TextSource;
    }

    public sealed class CylinderDef
    {
        public string Name;
        public CylinderBits Bits = new CylinderBits();
        /// <summary>Indeks bita F_CMD v polju Cylinder_Control (bitni odmik v DB4000, izračunan iz Standard layouta); null = ni znan.</summary>
        public int? ArrayIndex;
        public int? ArrayIndexBwd;
        public LangText Text;
        public string TextSource;
    }

    public sealed class CylinderBits
    {
        public string FwdCmd;
        public string BwdCmd;
        public string Fwd;
        public string Bwd;
    }

    public sealed class GlobalData
    {
        public List<string> ServiceSettingsArrays = new List<string>();
        public List<string> ServiceSettingsBools = new List<string>();
        public string ManualControlEnable;
        /// <summary>Potrebna dolžina HMI polja Cylinder_Control (zadnji bit DB4000 + 1).</summary>
        public int? CylinderControlArraySize;
        [JsonProperty("_vir_cylinderControl")] public string CylinderControlSource;
        public List<AlarmDef> Alarms = new List<AlarmDef>();
        public List<ParamDef> RecipeOther = new List<ParamDef>();
        [JsonProperty("_vir_alarms")] public string AlarmSource = "biti DB_Error_Messages; razred iz predpone (Error_/Warning_); enota iz oznake v imenu; besedila iz slovarja + ročno";
    }

    public sealed class AlarmDef
    {
        public string Bit;
        /// <summary>"Errors" ali "Warnings".</summary>
        public string Class;
        public string Unit;
        public int? BitOffset;
        public LangText Text;
        public string TextSource;
    }

    public sealed class ModelWarning
    {
        public string Unit;
        public string Code;
        public string Message;
        public override string ToString() => (Unit == null ? "" : Unit + ": ") + Code + " – " + Message;
    }

    public sealed class ModelStatistics
    {
        public int Units;
        public int Timers;
        public int TimersShown;
        public int Parameters;
        public int RecipeParameters;
        public int Cylinders;
        public int Alarms;
        public int TextsFromDictionary;
        public int TextsComposed;
        public int TextsMissing;
    }
}
