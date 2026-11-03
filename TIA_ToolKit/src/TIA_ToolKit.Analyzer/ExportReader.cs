using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// Prebere izvoz "Export all" (SimaticML V18-V20) v model: bloki z networki (STL, LAD/F_LAD, SCL), PLC tagi.
    /// Ne potrebuje TIA Portala.
    /// </summary>
    public static class ExportReader
    {
        /// <summary>Vse mape s projektom pod root (mapa z manifest.json ali s PLC\ podmapo).</summary>
        public static List<string> FindProjects(string root)
        {
            var result = new List<string>();
            if (File.Exists(Path.Combine(root, "manifest.json")) || Directory.Exists(Path.Combine(root, "PLC")))
            {
                result.Add(root);
                return result;
            }
            foreach (var m in Directory.EnumerateFiles(root, "manifest.json", SearchOption.AllDirectories))
                result.Add(Path.GetDirectoryName(m));
            return result;
        }

        public static ExportProject Read(string projectDir, Action<string> log = null)
        {
            log = log ?? (s => { });
            var p = new ExportProject { Root = projectDir, Name = Path.GetFileName(projectDir.TrimEnd('\\', '/')) };

            string manifest = Path.Combine(projectDir, "manifest.json");
            if (File.Exists(manifest))
            {
                string j = File.ReadAllText(manifest);
                var m = Regex.Match(j, "\"project\"\\s*:\\s*\"([^\"]*)\""); if (m.Success) p.Name = Unescape(m.Groups[1].Value);
                m = Regex.Match(j, "\"exportedAt\"\\s*:\\s*\"([^\"]*)\""); if (m.Success) p.ExportedAt = m.Groups[1].Value;
            }
            string exportLog = Path.Combine(projectDir, "export_log.txt");
            if (File.Exists(exportLog))
                foreach (var line in File.ReadAllLines(exportLog))
                {
                    var m = Regex.Match(line, @"(PRESKOČENO|NAPAKA)\s+(\S.*?)(?::\s+(.*))?$");
                    if (!m.Success || line.Contains("jezik nima tekstovnega vira")) continue;
                    string what = m.Groups[2].Value.Trim();
                    string why = m.Groups[3].Value;
                    if (why.Contains("inconsistent")) why = "nekonsistentna (ni izvožena)";
                    else if (why.Length > 60) why = why.Substring(0, 60) + "…";
                    p.NotExported.Add(what + (why.Length > 0 ? " – " + why : ""));
                }

            string plcDir = Path.Combine(projectDir, "PLC");
            if (Directory.Exists(plcDir))
                foreach (var d in Directory.GetDirectories(plcDir))
                {
                    var plc = new PlcProgram { Name = Path.GetFileName(d), Root = d };
                    ReadBlocks(plc, log);
                    ReadTags(plc);
                    ReadTypes(plc, log);
                    p.Plcs.Add(plc);
                    log("PLC " + plc.Name + ": " + plc.Blocks.Count + " blokov, " + plc.Tags.Count + " tagov.");
                }
            string hmiDir = Path.Combine(projectDir, "HMI");
            if (Directory.Exists(hmiDir)) p.HmiRoots.AddRange(Directory.GetDirectories(hmiDir));
            foreach (var h in p.HmiRoots) p.Hmis.Add(HmiReader.Read(h, log));
            return p;
        }

        static string Unescape(string s) => s.Replace("\\\\", "\\").Replace("\\\"", "\"");

        // ------------------------------------------------------------------ bloki

        static void ReadBlocks(PlcProgram plc, Action<string> log)
        {
            string blocksDir = Path.Combine(plc.Root, "Blocks");
            string srcDir = Path.Combine(plc.Root, "Sources");
            if (!Directory.Exists(blocksDir)) return;
            foreach (var f in Directory.EnumerateFiles(blocksDir, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    var b = ReadBlock(f);
                    if (b == null) continue;
                    string rel = PathUtil.MakeRelative(blocksDir, f);
                    string folder = Path.GetDirectoryName(rel) ?? "";
                    b.Folder = folder.Replace('\\', '/');
                    string scl = Path.Combine(srcDir, Path.ChangeExtension(rel, ".scl"));
                    if (File.Exists(scl)) b.SclSource = File.ReadAllText(scl);
                    plc.Blocks.Add(b);
                }
                catch (Exception ex) { log("  Napaka pri branju " + f + ": " + ex.Message); }
            }
        }

        static void ReadTypes(PlcProgram plc, Action<string> log)
        {
            string dir = Path.Combine(plc.Root, "Types");
            if (!Directory.Exists(dir)) return;
            foreach (var f in Directory.EnumerateFiles(dir, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    var t = XDocument.Load(f).Root?.Elements().FirstOrDefault(x => L(x) == "SW.Types.PlcStruct");
                    var attrs = Kid(t, "AttributeList");
                    string name = (string)Kid(attrs, "Name");
                    var sections = Kid(attrs, "Interface")?.Elements().FirstOrDefault();
                    if (name != null && sections != null) plc.Types[name] = sections;
                }
                catch (Exception ex) { log("  Napaka pri branju tipa " + f + ": " + ex.Message); }
            }
        }

        static string L(XElement e) => e.Name.LocalName;
        static IEnumerable<XElement> Kids(XElement e, string name) => e.Elements().Where(x => L(x) == name);
        static XElement Kid(XElement e, string name) => e?.Elements().FirstOrDefault(x => L(x) == name);
        static XElement Find(XElement e, string name) => e?.Descendants().FirstOrDefault(x => L(x) == name);

        public static Block ReadBlock(string file)
        {
            var doc = XDocument.Load(file);
            var blk = doc.Root?.Elements().FirstOrDefault(x => L(x).StartsWith("SW.Blocks."));
            if (blk == null) return null;
            var attrs = Kid(blk, "AttributeList");
            var b = new Block
            {
                Name = (string)Kid(attrs, "Name") ?? Path.GetFileNameWithoutExtension(file),
                Language = (string)Kid(attrs, "ProgrammingLanguage"),
            };
            int.TryParse((string)Kid(attrs, "Number"), out b.Number);
            b.MemoryLayout = (string)Kid(attrs, "MemoryLayout");
            if (L(blk) == "SW.Blocks.GlobalDB" || L(blk) == "SW.Blocks.InstanceDB")
            {
                b.Interface = Kid(attrs, "Interface")?.Elements().FirstOrDefault();
                b.InstanceOfName = (string)Kid(attrs, "InstanceOfName");
            }
            switch (L(blk).Substring("SW.Blocks.".Length))
            {
                case "OB": b.Kind = BlockKind.OB; break;
                case "FB": b.Kind = BlockKind.FB; break;
                case "FC": b.Kind = BlockKind.FC; break;
                case "GlobalDB": b.Kind = BlockKind.GlobalDB; break;
                case "InstanceDB": b.Kind = BlockKind.InstanceDB; break;
                default: b.Kind = BlockKind.Other; break;
            }
            int i = 0;
            foreach (var cu in blk.Descendants().Where(x => L(x) == "SW.Blocks.CompileUnit"))
            {
                var nw = new Network { Index = ++i };
                var a = Kid(cu, "AttributeList");
                nw.Language = (string)Kid(a, "ProgrammingLanguage");
                nw.Title = TitleOf(cu);
                var src = Kid(a, "NetworkSource");
                if (src != null)
                {
                    var sl = Kid(src, "StatementList"); var fn = Kid(src, "FlgNet"); var st = Kid(src, "StructuredText");
                    if (sl != null) nw.Stl = ReadStl(sl);
                    else if (fn != null) nw.Lad = ReadFlg(fn);
                    else if (st != null) nw.Scl = ReadScl(st);
                }
                // globalni dostopi kjerkoli v networku; indeksni dostopi znotraj simbola (Symbol/Component/Access) niso samostojne reference
                nw.Refs = cu.Descendants().Where(x => L(x) == "Access" && (string)x.Attribute("Scope") == "GlobalVariable"
                                                      && !x.Ancestors().TakeWhile(p => p != cu).Any(p => L(p) == "Symbol"))
                            .Select(AccessText).Where(t => t != null).ToList();
                b.Networks.Add(nw);
            }
            return b;
        }

        static string TitleOf(XElement cu)
        {
            foreach (var mt in cu.Descendants().Where(x => L(x) == "MultilingualText" && (string)x.Attribute("CompositionName") == "Title"))
                foreach (var t in mt.Descendants().Where(x => L(x) == "Text"))
                    if (!string.IsNullOrWhiteSpace(t.Value)) return t.Value.Trim();
            return "";
        }

        // ------------------------------------------------------------------ operandi

        public static string AccessText(XElement a)
        {
            string scope = (string)a.Attribute("Scope") ?? "";
            var ci = Kid(a, "CallInfo") ?? Kid(a, "Instruction");
            if (ci != null) return "CALL " + (string)ci.Attribute("Name");
            var sym = Kid(a, "Symbol");
            if (sym != null)
            {
                string t = SymbolText(sym);
                return scope == "GlobalVariable" ? "\"" + t + "\"" : scope == "LocalVariable" ? "#" + t : t;
            }
            var c = Kid(a, "Constant");
            if (c != null) return (string)Find(c, "ConstantValue") ?? (string)c.Attribute("Name") ?? "?";
            var ad = Kid(a, "Address");
            if (ad != null) return "%" + (string)ad.Attribute("Area") + (string)ad.Attribute("Type") + (string)ad.Attribute("BitOffset");
            var lbl = Kid(a, "Label"); if (lbl != null) return "LBL:" + (string)lbl.Attribute("Name");
            return null;
        }

        static string SymbolText(XElement sym) =>
            string.Join(".", Kids(sym, "Component").Select(c =>
            {
                string s = (string)c.Attribute("Name");
                var idx = Kids(c, "Access").Select(AccessText).ToList();
                return idx.Count > 0 ? s + "[" + string.Join(",", idx) + "]" : s;
            }));

        static List<StlStatement> ReadStl(XElement sl)
        {
            var list = new List<StlStatement>();
            foreach (var st in Kids(sl, "StlStatement"))
            {
                var tok = Kid(st, "StlToken");
                string op = tok != null ? (string)tok.Attribute("Text") : (Kid(st, "LineComment") != null || Kid(st, "Comment") != null ? "COMMENT" : "?");
                var acc = Kid(st, "Access");
                list.Add(new StlStatement { Op = op, Operand = acc != null ? AccessText(acc) : null });
            }
            return list;
        }

        static LadNetwork ReadFlg(XElement fn)
        {
            var net = new LadNetwork();
            var parts = Kid(fn, "Parts"); var wires = Kid(fn, "Wires");
            var access = new Dictionary<string, string>();
            if (parts != null)
                foreach (var e in parts.Elements())
                {
                    string uid = (string)e.Attribute("UId");
                    if (L(e) == "Access") access[uid] = AccessText(e);
                    else if (L(e) == "Part")
                        net.Parts[uid] = new LadPart { Uid = uid, Name = (string)e.Attribute("Name"), Negated = Kids(e, "Negated").Any() };
                    else if (L(e) == "Call")
                        net.Parts[uid] = new LadPart { Uid = uid, Name = "CALL " + (string)Kid(e, "CallInfo")?.Attribute("Name") };
                }
            if (wires != null)
                foreach (var w in Kids(wires, "Wire"))
                {
                    var ends = w.Elements().ToList();
                    if (ends.Count == 0) continue;
                    var first = ends[0];
                    // Operand: Access (IdentCon) -> pin "operand" dela
                    if (L(first) == "IdentCon" && access.TryGetValue((string)first.Attribute("UId"), out var opText))
                    {
                        foreach (var t in ends.Skip(1).Where(x => L(x) == "NameCon"))
                            if (net.Parts.TryGetValue((string)t.Attribute("UId"), out var part) && (string)t.Attribute("Name") == "operand")
                                part.Operand = opText;
                        continue;
                    }
                    var wire = new LadWire
                    {
                        Source = L(first) == "Powerrail" ? LadEnd.Pwr
                               : new LadEnd { Uid = (string)first.Attribute("UId"), Pin = (string)first.Attribute("Name") }
                    };
                    foreach (var t in ends.Skip(1).Where(x => L(x) == "NameCon"))
                        wire.Targets.Add(new LadEnd { Uid = (string)t.Attribute("UId"), Pin = (string)t.Attribute("Name") });
                    net.Wires.Add(wire);
                }
            return net;
        }

        static string ReadScl(XElement st)
        {
            var sb = new StringBuilder();
            foreach (var e in st.Elements())
                switch (L(e))
                {
                    case "Token": sb.Append((string)e.Attribute("Text")); break;
                    case "Blank": sb.Append(' ', Math.Max(1, (int?)e.Attribute("Num") ?? 1)); break;
                    case "NewLine": sb.Append('\n', Math.Max(1, (int?)e.Attribute("Num") ?? 1)); break;
                    case "Access": sb.Append(AccessText(e)); break;
                    case "LineComment": sb.Append("//").Append(string.Concat(e.Descendants().Where(x => L(x) == "Text").Select(x => x.Value))); break;
                    case "Text": sb.Append(e.Value); break;
                }
            return sb.ToString();
        }

        // ------------------------------------------------------------------ tagi

        static readonly Regex TagRx = new Regex(@"<SW\.Tags\.PlcTag [\s\S]*?</AttributeList>", RegexOptions.Compiled);
        static readonly Regex AddrRx = new Regex(@"^%([IQM])([BWDL]?)(\d+)(?:\.(\d+))?$", RegexOptions.Compiled);

        static void ReadTags(PlcProgram plc)
        {
            string tagsDir = Path.Combine(plc.Root, "Tags");
            if (!Directory.Exists(tagsDir)) return;
            foreach (var f in Directory.EnumerateFiles(tagsDir, "*.xml", SearchOption.AllDirectories))
            {
                string x = File.ReadAllText(f);
                foreach (Match m in TagRx.Matches(x))
                {
                    string Get(string t) { var mm = Regex.Match(m.Value, "<" + t + ">([^<]*)</" + t + ">"); return mm.Success ? System.Net.WebUtility.HtmlDecode(mm.Groups[1].Value) : null; }
                    var tag = new PlcTag { Name = Get("Name"), DataType = Get("DataTypeName"), Address = Get("LogicalAddress"), Table = PathUtil.MakeRelative(tagsDir, f) };
                    if (tag.Address == null) continue;
                    var a = AddrRx.Match(tag.Address);
                    if (a.Success)
                    {
                        tag.Area = a.Groups[1].Value[0];
                        int bits = a.Groups[2].Value == "" ? 1 : a.Groups[2].Value == "B" ? 8 : a.Groups[2].Value == "W" ? 16 : a.Groups[2].Value == "D" ? 32 : 64;
                        tag.BitLo = int.Parse(a.Groups[3].Value) * 8 + (a.Groups[4].Success ? int.Parse(a.Groups[4].Value) : 0);
                        tag.BitHi = tag.BitLo + bits - 1;
                    }
                    plc.Tags.Add(tag);
                }
            }
        }
    }
}
