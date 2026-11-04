using System;
using System.Collections.Generic;
using System.Linq;
using TIA_Generator.Model;

namespace TIA_Generator.Gui
{
    // Vrstice za DataGridView: lastnosti pišejo neposredno v objekte modela.
    // Ročno spremenjeno besedilo dobi izvor "rocno".

    abstract class TextRow
    {
        protected abstract LangText Text { get; set; }
        protected abstract void SetSource(string s);

        string Get(string lang) => Text != null && Text.TryGetValue(lang, out var v) ? v : "";
        void Set(string lang, string value)
        {
            if (Text == null) Text = new LangText();
            value = value ?? "";
            if (Get(lang) == value) return;
            Text[lang] = value;
            SetSource(TextSource.Manual);
        }

        public string Sl { get => Get("sl-SI"); set => Set("sl-SI", value); }
        public string En { get => Get("en-US"); set => Set("en-US", value); }
        public string De { get => Get("de-DE"); set => Set("de-DE", value); }
    }

    sealed class TimerRow : TextRow
    {
        public readonly TimerDef T;
        public TimerRow(TimerDef t) { T = t; }
        protected override LangText Text { get => T.Text; set => T.Text = value; }
        protected override void SetSource(string s) => T.TextSource = s;
        public int Index => T.Index;
        public string Member => T.Member;
        public string Element => T.ArrayElement ?? T.Member;
        public bool Show { get => T.Show; set => T.Show = value; }
        public string Source => T.TextSource ?? "";
        public string UsedIn => T.UsedIn == null ? "" : string.Join("; ", T.UsedIn);
    }

    sealed class ParamRow : TextRow
    {
        public readonly ParamDef P;
        public ParamRow(ParamDef p) { P = p; }
        protected override LangText Text { get => P.Text; set => P.Text = value; }
        protected override void SetSource(string s) => P.TextSource = s;
        public string Member => P.Member;
        public string DataType => P.DataType;
        public string Access { get => P.Access ?? "input"; set => P.Access = value; }
        public string Unit { get => P.Unit ?? ""; set => P.Unit = string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
        public string Source => P.TextSource ?? "";
    }

    sealed class CylinderRow : TextRow
    {
        public readonly CylinderDef C;
        public CylinderRow(CylinderDef c) { C = c; }
        protected override LangText Text { get => C.Text; set => C.Text = value; }
        protected override void SetSource(string s) => C.TextSource = s;
        public string Name => C.Name;
        public string FwdCmd => C.Bits.FwdCmd ?? "";
        public string BwdCmd => C.Bits.BwdCmd ?? "";
        public string IndexF => C.ArrayIndex?.ToString() ?? "?";
        public string IndexB => C.ArrayIndexBwd?.ToString() ?? "?";
        public string Source => C.TextSource ?? "";
    }

    sealed class AlarmRow : TextRow
    {
        public readonly AlarmDef A;
        public AlarmRow(AlarmDef a) { A = a; }
        protected override LangText Text { get => A.Text; set => A.Text = value; }
        protected override void SetSource(string s) => A.TextSource = s;
        public string Bit => A.Bit;
        public string Class { get => A.Class; set => A.Class = value; }
        public string Unit => A.Unit ?? "";
        public string Offset => A.BitOffset.HasValue ? A.BitOffset + "  (DBX" + A.BitOffset / 8 + "." + A.BitOffset % 8 + ")" : "";
        public string Source => A.TextSource ?? "";
    }

    sealed class WarningRow
    {
        public readonly ModelWarning W;
        public WarningRow(ModelWarning w) { W = w; }
        public string Unit => W.Unit ?? "";
        public string Code => W.Code;
        public string Message => W.Message;
    }

    sealed class UnitRow
    {
        public readonly Unit U;
        readonly int _warnings;
        public UnitRow(Unit u, IEnumerable<ModelWarning> warnings)
        {
            U = u;
            _warnings = warnings.Count(w => w.Unit != null && w.Unit.Split(new[] { ", " }, StringSplitOptions.None).Contains(u.Id));
        }
        public string Id => U.Id;
        public string Part => U.Part ?? U.Group ?? "";
        public string Plc => U.PlcIndex?.ToString() ?? "?";
        public string Hmi => U.HmiIndex?.ToString() ?? "?";
        public string Settings => U.SettingsDb != null ? "DB" + U.SettingsDb.Number : "-";
        public string Timers => U.Timers.Count(t => t.Show) + "/" + U.Timers.Count;
        public int Params => U.Parameters.Count;
        public int Recipe => U.RecipeParameters.Count;
        public int Cylinders => U.Cylinders.Count;
        public int Alarms => U.Alarms.Count;
        public int Warnings => _warnings;
        public string Text => U.Text != null && U.Text.TryGetValue("sl-SI", out var s) ? s : "";
    }
}
