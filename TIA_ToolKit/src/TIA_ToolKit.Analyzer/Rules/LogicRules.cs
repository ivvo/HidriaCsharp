using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// Razčlenitev logičnih verig (STL in LAD) na AND-člene. Najde:
    ///  - isti operand dvakrat v istem AND-členu ali dva enaka OR-člena (odvečno, tipično kopiranje: "O X / O X"),
    ///  - isti operand z nasprotnim predznakom v istem AND-členu ("A X ... AN X" = vedno FALSE).
    /// STL: AND ima prednost pred OR, zato "O" (z ali brez operanda) začne nov člen; oklepaji so svoj nivo; X/XN, NOT, FP/FN
    /// nivo izločijo iz preverjanja. LAD: zaporedni kontakti od tuljave nazaj do napajalne letve.
    /// </summary>
    static class LogicChains
    {
        public sealed class Hit
        {
            public Network Nw;
            public bool Contra;       // true = A X / AN X, false = podvojeno
            public string Text;
        }

        static readonly Regex CompareRx = new Regex(@"^(EQ|NE|GT|GE|LT|LE)_");
        static readonly Regex IgnoreRx = new Regex(@"^(L|T|TAK|PUSH|POP|ENT|LEAVE|INC|DEC|COMMENT|EMPTY_LINE|\?|(ADD|SUB|MUL|DIV|MOD|NEG|ABS|SQRT|SQR|EXP|LN|SIN|COS|TAN)(_\w+)?)$");

        sealed class Frame
        {
            public Dictionary<string, bool> Term = new Dictionary<string, bool>();
            public List<string> Terms = new List<string>();
            public bool Abort;
            public int Tokens;
        }

        public static List<Hit> Analyze(Network nw)
        {
            var hits = new List<Hit>();
            if (nw.Stl.Count > 0) AnalyzeStl(nw, hits);
            if (nw.Lad != null) AnalyzeLad(nw, hits);
            return hits;
        }

        static void AnalyzeStl(Network nw, List<Hit> hits)
        {
            var stack = new Stack<Frame>();
            stack.Push(new Frame());
            foreach (var s in nw.Stl)
            {
                string op = s.Op ?? "";
                var f = stack.Peek();
                switch (op)
                {
                    case "A": case "AN":
                        if (s.Operand != null) Add(f, s.Operand, op == "AN", nw, hits);
                        break;
                    case "O": case "ON":
                        CloseTerm(f);
                        if (s.Operand != null) Add(f, s.Operand, op == "ON", nw, hits);
                        break;
                    case "X": case "XN": case "NOT": case "FP": case "FN":
                        f.Abort = true;
                        break;
                    case "A_BRACK": case "AN_BRACK": case "O_BRACK": case "ON_BRACK": case "X_BRACK": case "XN_BRACK":
                        if (op.StartsWith("O")) CloseTerm(f);
                        if (op.StartsWith("X")) f.Abort = true;
                        f.Term["(bracket#" + (++f.Tokens) + ")"] = false;
                        stack.Push(new Frame());
                        break;
                    case "BRACKET":
                        if (stack.Count > 1) Finish(stack.Pop(), nw, hits);
                        break;
                    default:
                        if (CompareRx.IsMatch(op)) { f.Term["(cmp#" + (++f.Tokens) + ")"] = false; break; }
                        if (IgnoreRx.IsMatch(op)) break;
                        // Assign, S, R, SET, CLR, CALL, skoki, časovniki ... končajo verigo
                        while (stack.Count > 0) Finish(stack.Pop(), nw, hits);
                        stack.Push(new Frame());
                        break;
                }
            }
            while (stack.Count > 0) Finish(stack.Pop(), nw, hits);
        }

        static void Add(Frame f, string operand, bool neg, Network nw, List<Hit> hits)
        {
            if (f.Term.TryGetValue(operand, out bool prev))
            {
                if (f.Abort) return;
                if (prev == neg)
                    hits.Add(new Hit { Nw = nw, Text = (neg ? "AN " : "A ") + operand + " 2× v istem AND-členu" });
                else
                    hits.Add(new Hit { Nw = nw, Contra = true, Text = "A " + operand + " in AN " + operand + " v istem AND-členu" });
                return;
            }
            f.Term[operand] = neg;
        }

        static void CloseTerm(Frame f)
        {
            if (f.Term.Count > 0)
                f.Terms.Add(string.Join(" ", f.Term.OrderBy(k => k.Key).Select(k => (k.Value ? "N" : "") + k.Key)));
            f.Term = new Dictionary<string, bool>();
        }

        static void Finish(Frame f, Network nw, List<Hit> hits)
        {
            CloseTerm(f);
            if (f.Abort) return;
            foreach (var g in f.Terms.GroupBy(t => t).Where(g => g.Count() > 1 && !g.Key.Contains("#")))
                hits.Add(new Hit { Nw = nw, Text = "OR-člen " + g.Key.Replace("N\"", "NOT \"") + " " + g.Count() + "×" });
        }

        static void AnalyzeLad(Network nw, List<Hit> hits)
        {
            var seen = new HashSet<string>();
            foreach (var coil in nw.Lad.Parts.Values.Where(p => p.IsCoil))
            {
                var chain = new Dictionary<string, bool>();
                var cur = coil;
                for (int guard = 0; guard < 200; guard++)
                {
                    var src = nw.Lad.SourceOf(cur.Uid);
                    if (src == null || src.Powerrail || !nw.Lad.Parts.TryGetValue(src.Uid, out var part) || part.Name != "Contact" || part.Operand == null) break;
                    if (chain.TryGetValue(part.Operand, out bool prev))
                    {
                        string key = part.Operand + "|" + (prev != part.Negated);
                        if (seen.Add(key))
                            hits.Add(prev == part.Negated
                                ? new Hit { Nw = nw, Text = "kontakt " + part.Operand + " 2× zaporedno" }
                                : new Hit { Nw = nw, Contra = true, Text = "kontakt " + part.Operand + " in negiran " + part.Operand + " zaporedno (vedno FALSE)" });
                    }
                    else chain[part.Operand] = part.Negated;
                    cur = part;
                }
            }
        }
    }

    /// <summary>DUP-OPERAND: isti operand 2× v istem AND-členu ali enak OR-člen 2× (O X / O X) - tipično kopiranje, manjka drug signal (LEFT/RIGHT, GOOD/BAD).</summary>
    public sealed class DuplicateOperandRule : IRule
    {
        public string Id => "DUP-OPERAND";
        public string Title => "Isti operand dvakrat v isti logični verigi";

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            foreach (var b in plc.Blocks.Where(x => !x.IsDb))
            {
                var hits = b.Networks.SelectMany(LogicChains.Analyze).Where(h => !h.Contra).ToList();
                if (hits.Count == 0) continue;
                var f = new Finding
                {
                    RuleId = Id, Category = Categories.CopyIndex, Severity = Severity.Medium, Confidence = "Preveriti",
                    Title = Title,
                    Description = string.Join("; ", hits.Select(h => "NW" + h.Nw.Index + ": " + h.Text).Distinct().Take(8)) + (hits.Count > 8 ? "; …" : "") +
                                  ". Podvojen pogoj je odveč - najpogosteje je pri kopiranju ostal isti signal namesto sosednjega (LEFT/RIGHT, GOOD/BAD, 1/2).",
                };
                f.Locations.Add(FindingLocation.Of(b, hits.Select(h => h.Nw).Distinct().ToArray()));
                yield return f;
            }
        }
    }

    /// <summary>CONTRA: A X in AN X v istem AND-členu (ali zaporedna kontakta X in /X) - pogoj je vedno FALSE.</summary>
    public sealed class ContradictionRule : IRule
    {
        public string Id => "CONTRA";
        public string Title => "Protisloven pogoj (X AND NOT X) – vedno FALSE";

        public IEnumerable<Finding> Check(ExportProject project, PlcProgram plc)
        {
            foreach (var b in plc.Blocks.Where(x => !x.IsDb))
                foreach (var g in b.Networks.SelectMany(LogicChains.Analyze).Where(h => h.Contra).GroupBy(h => h.Nw))
                {
                    var f = new Finding
                    {
                        RuleId = Id, Category = b.IsFailsafe ? Categories.Safety : Categories.Functional, Severity = Severity.High,
                        Title = Title,
                        Description = string.Join("; ", g.Select(h => h.Text).Distinct()) +
                                      ". Veriga z X in NOT X ne more postati TRUE, zato se izhod nikoli ne postavi (verjetno je mišljen drug signal ali obraten kontakt).",
                    };
                    f.Locations.Add(FindingLocation.Of(b, g.Key));
                    yield return f;
                }
        }
    }
}
