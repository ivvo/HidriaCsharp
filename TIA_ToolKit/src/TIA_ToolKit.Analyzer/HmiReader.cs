using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// Prebere HMI target iz exporta (WinCC Comfort/Advanced SimaticML): tagi (Tags\), zasloni (Screens\), predloge (Templates\),
    /// VB skripte (Scripts\). Za vsak objekt na zaslonu: položaj, način, avtorizacija, vezave tagov in funkcijski seznami dogodkov.
    /// </summary>
    public static class HmiReader
    {
        static string L(XElement e) => e.Name.LocalName;
        static IEnumerable<XElement> Kids(XElement e, string name) => e == null ? Enumerable.Empty<XElement>() : e.Elements().Where(x => L(x) == name);
        static XElement Kid(XElement e, string name) => e?.Elements().FirstOrDefault(x => L(x) == name);
        static string Attr(XElement item, string name) => (string)Kid(Kid(item, "AttributeList"), name);
        static int Int(XElement item, string name) => int.TryParse(Attr(item, name), out int v) ? v : 0;
        static string LinkName(XElement item, string link) => (string)Kid(Kid(Kid(item, "LinkList"), link), "Name");

        public static HmiTarget Read(string root, Action<string> log)
        {
            var t = new HmiTarget { Name = Path.GetFileName(root.TrimEnd('\\', '/')), Root = root };
            foreach (var f in Files(root, "Tags")) Guard(log, f, () => ReadTags(t, f));
            foreach (var f in Files(root, "Screens")) Guard(log, f, () => ReadScreen(t, f, false));
            foreach (var f in Files(root, "Templates")) Guard(log, f, () => ReadScreen(t, f, true));
            foreach (var f in Files(root, "Scripts")) Guard(log, f, () => ReadScript(t, f));
            foreach (var tag in t.Tags) t.TagByName[tag.Name] = tag;
            log("HMI " + t.Name + ": " + t.Tags.Count + " tagov, " + t.Screens.Count + " zaslonov/predlog, " + t.Scripts.Count + " skript.");
            return t;
        }

        static IEnumerable<string> Files(string root, string sub)
        {
            string d = Path.Combine(root, sub);
            return Directory.Exists(d) ? Directory.EnumerateFiles(d, "*.xml", SearchOption.AllDirectories) : Enumerable.Empty<string>();
        }

        static void Guard(Action<string> log, string f, Action a)
        {
            try { a(); } catch (Exception ex) { log("  Napaka pri branju " + f + ": " + ex.Message); }
        }

        static string Rel(HmiTarget t, string f) => PathUtil.MakeRelative(t.Root, f).Replace('\\', '/');

        static void ReadTags(HmiTarget t, string f)
        {
            var doc = XDocument.Load(f);
            foreach (var tag in doc.Descendants().Where(x => L(x) == "Hmi.Tag.Tag"))
            {
                foreach (var fe in tag.Descendants().Where(x => L(x) == "Hmi.Event.FunctionListEntry"))
                {
                    string fn = Attr(fe, "Name") ?? LinkName(fe, "Script");
                    if (fn != null) t.TagEventFunctions.Add(fn);
                }
                bool absolute = Attr(tag, "AddressAccessMode") == "Absolute";
                t.Tags.Add(new HmiTag
                {
                    Name = Attr(tag, "Name"),
                    Address = absolute ? Attr(tag, "LogicalAddress") : null,
                    PlcTag = absolute ? null : LinkName(tag, "ControllerTag"),
                    DataType = LinkName(tag, "DataType"),
                    Length = Int(tag, "Length"),
                    File = Rel(t, f),
                });
            }
        }

        static void ReadScreen(HmiTarget t, string f, bool template)
        {
            var doc = XDocument.Load(f);
            var scr = doc.Root?.Elements().FirstOrDefault(x => L(x) == "Hmi.Screen.Screen" || L(x) == "Hmi.Screen.ScreenTemplate");
            if (scr == null) return;
            var s = new HmiScreen { Name = Attr(scr, "Name"), File = Rel(t, f), IsTemplate = template, Template = LinkName(scr, "Template") };
            foreach (var e in scr.Descendants().Where(x => L(x).StartsWith("Hmi.Screen.") && (string)x.Attribute("CompositionName") == "ScreenItems"))
            {
                var item = new HmiItem
                {
                    Kind = L(e).Substring("Hmi.Screen.".Length),
                    Name = Attr(e, "ObjectName"),
                    Left = Int(e, "Left"), Top = Int(e, "Top"), Width = Int(e, "Width"), Height = Int(e, "Height"),
                    Mode = Attr(e, "Mode"),
                    HiddenInput = Attr(e, "HiddenInput") == "true",
                    Authorization = LinkName(e, "Authorization"),
                };
                if (int.TryParse(Attr(e, "TabIndex"), out int ti)) item.TabIndex = ti;
                var own = Kid(e, "ObjectList");
                foreach (var p in Kids(own, "Hmi.Screen.Property"))
                {
                    string prop = Attr(p, "Name");
                    foreach (var tagName in p.Descendants().Where(x => L(x) == "Tag").Select(x => (string)Kid(x, "Name")).Where(n => n != null))
                        item.Bindings.Add((prop, tagName));
                }
                foreach (var ev in Kids(own, "Hmi.Event.Event"))
                    foreach (var fe in ev.Descendants().Where(x => L(x) == "Hmi.Event.FunctionListEntry"))
                    {
                        var a = new HmiAction { Event = Attr(ev, "Name"), Function = Attr(fe, "Name") ?? LinkName(fe, "Script") };
                        foreach (var par in fe.Descendants().Where(x => L(x) == "Hmi.Event.FunctionListEntryParameter"))
                        {
                            string tagName = LinkName(par, "Value");
                            if (tagName != null) a.Tags.Add(tagName);
                            else if (Attr(par, "Value") != null) a.Values.Add(Attr(par, "Value"));
                        }
                        item.Actions.Add(a);
                    }
                foreach (var anim in Kids(own, "Hmi.Dynamic.VisibilityAnimation").Concat(Kids(own, "Hmi.Dynamic.SingleBitVisibilityAnimation"))
                                     .Concat(Kids(own, "Hmi.Dynamic.RangeAppearanceAnimation")))
                    item.AnimationTags.AddRange(anim.Descendants().Where(x => L(x) == "Tag").Select(x => (string)Kid(x, "Name")).Where(n => n != null));
                s.Items.Add(item);
            }
            t.Screens.Add(s);
        }

        static void ReadScript(HmiTarget t, string f)
        {
            var doc = XDocument.Load(f);
            var sc = doc.Root?.Elements().FirstOrDefault(x => L(x).StartsWith("Hmi.RuntimeScripting."));
            if (sc == null) return;
            t.Scripts.Add(new HmiScript { Name = Attr(sc, "Name"), File = Rel(t, f), Code = Attr(sc, "Code") ?? "" });
        }
    }
}
