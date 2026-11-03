using System.Collections.Generic;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>En HMI target iz exporta (HMI\&lt;ime&gt;\Screens, Templates, Tags, Scripts) - WinCC Comfort/Advanced.</summary>
    public sealed class HmiTarget
    {
        public string Name;
        public string Root;
        public List<HmiTag> Tags = new List<HmiTag>();
        public List<HmiScreen> Screens = new List<HmiScreen>();   // zasloni in predloge (IsTemplate)
        public List<HmiScript> Scripts = new List<HmiScript>();
        /// <summary>Funkcije/skripte, ki jih prožijo dogodki tagov (ValueChange ...).</summary>
        public HashSet<string> TagEventFunctions = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, HmiTag> TagByName = new Dictionary<string, HmiTag>(System.StringComparer.OrdinalIgnoreCase);
    }

    public sealed class HmiTag
    {
        public string Name;
        public string Address;        // "%DB1010.DBX24.0", "%M4.2" (absolutno) ali null
        public string PlcTag;         // simbolno naslavljanje: ime PLC taga
        public string DataType;       // "Bool", "Int", "Array[0..9] of S5Time" ...
        public int Length;            // bajti
        public string File;           // relativno na HMI target, npr. "Tags/Robot/Robots.xml"
    }

    public sealed class HmiScreen
    {
        public string Name;
        public string File;           // relativno na HMI target
        public bool IsTemplate;
        public string Template;
        public List<HmiItem> Items = new List<HmiItem>();
    }

    public sealed class HmiItem
    {
        public string Kind;           // "IOField", "Button", "Switch", ...
        public string Name;           // ObjectName
        public int Left, Top, Width, Height;
        public string Mode;           // IOField: Input/Output/InOutput; Switch: Switch ...
        public string Authorization;
        public bool HiddenInput;
        public int TabIndex = -1;
        /// <summary>Vezave lastnosti: ProcessValue → tag, ...</summary>
        public List<(string Property, string Tag)> Bindings = new List<(string, string)>();
        /// <summary>Dogodki: (Click, SetBit, [tagi/parametri]).</summary>
        public List<HmiAction> Actions = new List<HmiAction>();
        /// <summary>Tagi v animacijah (vidnost, izgled) - samo prikaz.</summary>
        public List<string> AnimationTags = new List<string>();

        public bool IsInput => Kind == "Switch" || Kind == "SymbolicIOField" ||
                               Kind == "IOField" && (Mode == "Input" || Mode == "InOutput");
        public override string ToString() => Name + " (" + Kind + ")";
    }

    public sealed class HmiAction
    {
        public string Event;          // Click, Press, Release, Activated ...
        public string Function;       // SetBit, ResetBit, InvertBit, ActivateScreen, StopRuntime, ime skripte ...
        public List<string> Tags = new List<string>();
        public List<string> Values = new List<string>();
    }

    public sealed class HmiScript
    {
        public string Name;
        public string File;
        public string Code;
    }
}
