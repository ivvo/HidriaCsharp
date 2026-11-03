using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TIA_ToolKit
{
    /// <summary>
    /// Poenostavljen izris exportiranega WinCC Comfort/Advanced zaslona (XML) v sliko.
    /// Podprto: Rectangle, Circle, Line, TextField, IOField, Button, Switch (pozicije, barve, gradient, pisave, besedila).
    /// NI podprto: slike iz grafičnih knjižnic (narisan je samo okvir z imenom), predloge (Template), animacije, faceplati.
    /// Namen: hiter vizualni pregled barv (npr. pred/po zamenjavi), ne natančna kopija runtime izgleda.
    /// </summary>
    static class ScreenRenderer
    {
        public static string PreferredCulture = "sl-SI";

        public static Bitmap Render(string xmlFile, float scale, out string info)
        {
            return Render(xmlFile, scale, null, out info);
        }

        /// <summary>
        /// Izris z označitvijo: elementi, katerih ObjectName, procesni tag ali besedilo je v <paramref name="highlight"/>,
        /// dobijo rdeč polprozoren okvir (kot ročne oznake v navodilih).
        /// </summary>
        public static Bitmap Render(string xmlFile, float scale, ICollection<string> highlight, out string info)
        {
            return Render(xmlFile, null, scale, highlight, out info);
        }

        /// <summary>
        /// Izris zaslona skupaj z njegovo predlogo (Screen Template). Predloga se išče kot &lt;ime predloge&gt;.xml
        /// (rekurzivno) v <paramref name="templateDir"/>; njeni elementi se narišejo pod elemente zaslona,
        /// tako kot na panelu (glava, spodnja menijska vrstica, stranski meni ...).
        /// </summary>
        public static Bitmap Render(string xmlFile, string templateDir, float scale, ICollection<string> highlight, out string info)
        {
            var doc = XDocument.Load(xmlFile);
            var screen = doc.Descendants().First(e => e.Name.LocalName == "Hmi.Screen.Screen" || e.Name.LocalName == "Hmi.Screen.ScreenTemplate");
            int w = AttrInt(screen, "Width", 800), h = AttrInt(screen, "Height", 480);

            var template = screen.Elements("LinkList").Elements("Template").Elements("Name").Select(e => e.Value).FirstOrDefault();
            XElement templateEl = null;
            if (template != null && templateDir != null && System.IO.Directory.Exists(templateDir))
            {
                var tf = System.IO.Directory.GetFiles(templateDir, template + ".xml", System.IO.SearchOption.AllDirectories).FirstOrDefault();
                if (tf != null)
                    templateEl = XDocument.Load(tf).Descendants().FirstOrDefault(e => e.Name.LocalName == "Hmi.Screen.ScreenTemplate");
            }
            info = Attr(screen, "Name") + " " + w + "x" + h +
                   (template == null ? "" : ", predloga: " + template + (templateEl != null ? "" : " (ni izrisana - ni najdena)"));

            var bmp = new Bitmap((int)Math.Round(w * scale), (int)Math.Round(h * scale), PixelFormat.Format32bppArgb);
            bmp.SetResolution(96, 96);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.ScaleTransform(scale, scale);
                g.Clear(AttrColor(screen, "BackColor") ?? Color.White);

                if (templateEl != null)
                    foreach (var layer in Children(templateEl, "Layers").OrderBy(l => AttrInt(l, "Index", 0)))
                        foreach (var item in Children(layer, "ScreenItems"))
                        {
                            try { DrawItem(g, item); }
                            catch { }
                        }

                var marked = new List<RectangleF>();
                var layers = Children(screen, "Layers").OrderBy(l => AttrInt(l, "Index", 0));
                foreach (var layer in layers)
                    foreach (var item in Children(layer, "ScreenItems"))
                    {
                        try { DrawItem(g, item); }
                        catch { /* posamezen element ne sme podreti celega izrisa */ }
                        if (highlight != null && highlight.Count > 0 && IsHighlighted(item, highlight))
                            marked.Add(new RectangleF(AttrInt(item, "Left", 0), AttrInt(item, "Top", 0), AttrInt(item, "Width", 0), AttrInt(item, "Height", 0)));
                    }

                foreach (var r in marked)
                    using (var path = RoundedRect(RectangleF.Inflate(r, 4, 4), 5))
                    using (var fill = new SolidBrush(Color.FromArgb(35, 255, 0, 0)))
                    using (var pen = new Pen(Color.Red, 2.5f))
                    {
                        g.FillPath(fill, path);
                        g.DrawPath(pen, path);
                    }
            }
            return bmp;
        }

        static bool IsHighlighted(XElement it, ICollection<string> keys)
        {
            if (keys.Contains(Attr(it, "ObjectName") ?? "")) return true;
            var pv = Children(it, "Properties").Where(p => Attr(p, "Name") == "ProcessValue")
                .SelectMany(p => Children(p, "Dynamic"))
                .SelectMany(d => d.Elements("LinkList").Elements("Tag").Elements("Name"))
                .Select(n => n.Value).FirstOrDefault();
            if (pv != null && keys.Contains(pv)) return true;
            var txt = GetText(it, "Text") ?? GetText(it, "TextOff");
            return txt != null && keys.Contains(txt.Replace("\n", " "));
        }

        // ================================================================== Elementi

        static void DrawItem(Graphics g, XElement it)
        {
            string type = it.Name.LocalName.Replace("Hmi.Screen.", "");
            var r = new RectangleF(AttrInt(it, "Left", 0), AttrInt(it, "Top", 0), AttrInt(it, "Width", 0), AttrInt(it, "Height", 0));

            switch (type)
            {
                case "Line":
                    DrawLine(g, it);
                    return;

                case "Circle":
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(r);
                        FillAndBorder(g, it, path, r);
                    }
                    return;

                case "Rectangle":
                    using (var path = RoundedRect(r, Math.Max(AttrInt(it, "RoundCornerWidth", 0), AttrInt(it, "RoundCornerHeight", 0)) / 2f))
                        FillAndBorder(g, it, path, r);
                    return;

                case "TextField":
                    using (var path = RoundedRect(r, AttrInt(it, "CornerRadius", 0)))
                        FillAndBorder(g, it, path, r);
                    DrawText(g, it, r, GetText(it, "Text"));
                    return;

                case "IOField":
                    using (var path = RoundedRect(r, AttrInt(it, "CornerRadius", 0)))
                        FillAndBorder(g, it, path, r);
                    DrawText(g, it, r, IoFieldSample(it));
                    return;

                case "Button":
                case "Switch":
                    using (var path = RoundedRect(r, AttrInt(it, "CornerRadius", 0)))
                        FillAndBorder(g, it, path, r);
                    string mode = Attr(it, "Mode") ?? "Text";
                    if (type == "Switch" && mode == "GraphicSwitchButton")
                        DrawSwitchTile(g, it, r);
                    else if (mode.StartsWith("Graphic") && CallsFunction(it, "StopRuntime"))
                        DrawPowerIcon(g, r);
                    else if (mode.StartsWith("Graphic"))
                        DrawPicturePlaceholder(g, it, r);
                    else
                        DrawText(g, it, r, GetText(it, "TextOff") ?? GetText(it, "TextOn"));
                    return;

                case "DateTimeField":
                    using (var path = RoundedRect(r, AttrInt(it, "CornerRadius", 0)))
                        FillAndBorder(g, it, path, r);
                    var now = DateTime.Now;
                    bool showDate = Attr(it, "ShowDate") != "false", showTime = Attr(it, "ShowTime") != "false";
                    string dt = (showDate ? now.ToString("M/d/yyyy", CultureInfo.InvariantCulture) : "") +
                                (showDate && showTime ? " " : "") +
                                (showTime ? now.ToString("h:mm:ss tt", CultureInfo.InvariantCulture) : "");
                    DrawText(g, it, r, dt);
                    return;

                case "GraphicView":
                    var pic = it.Elements("LinkList").Elements("Picture").Elements("Name").Select(e => e.Value).FirstOrDefault() ?? "";
                    if (pic.IndexOf("Hidria", StringComparison.OrdinalIgnoreCase) >= 0)
                        DrawHidriaLogo(g, r);   // slika logotipa prekrije ozadje - ozadja ne rišemo
                    else
                    {
                        using (var path = RoundedRect(r, 0))
                            FillAndBorder(g, it, path, r);
                        DrawPicturePlaceholder(g, it, r);
                    }
                    return;

                default:
                    // neznan tip - samo okvir, da je vidno, da nekaj tam je
                    using (var pen = new Pen(Color.Magenta) { DashStyle = DashStyle.Dash })
                        g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                    return;
            }
        }

        static void DrawLine(Graphics g, XElement it)
        {
            var c = AttrColor(it, "Color") ?? Color.Black;
            float lw = Math.Max(1, AttrInt(it, "LineWidth", 1));
            using (var pen = new Pen(c, lw))
            {
                if (Attr(it, "Style") == "Dash") pen.DashStyle = DashStyle.Dash;
                else if (Attr(it, "Style") == "Dot") pen.DashStyle = DashStyle.Dot;
                g.DrawLine(pen, AttrInt(it, "StartLeft", 0), AttrInt(it, "StartTop", 0), AttrInt(it, "EndLeft", 0), AttrInt(it, "EndTop", 0));
            }
        }

        static void FillAndBorder(Graphics g, XElement it, GraphicsPath path, RectangleF r)
        {
            string fill = Attr(it, "BackFillStyle") ?? Attr(it, "FillStyle") ?? "Solid";
            var back = AttrColor(it, "BackColor") ?? Color.White;

            if (fill == "Solid")
            {
                using (var b = new SolidBrush(back)) g.FillPath(b, path);
            }
            else if (fill.StartsWith("Gradient") && r.Width > 0 && r.Height > 0)
            {
                using (var b = GradientBrush(it, r, back, fill == "GradientHorizontal")) g.FillPath(b, path);
            }
            // Transparent -> brez polnila

            int bw = AttrInt(it, "BorderWidth", 0);
            if (bw > 0)
                using (var pen = new Pen(AttrColor(it, "BorderColor") ?? Color.Black, bw) { Alignment = PenAlignment.Inset })
                    g.DrawPath(pen, path);
        }

        /// <summary>
        /// WinCC gradient: FirstGradientColor (rob, širina FirstGradientOffset %) -> MiddleGradientColor (sredina)
        /// -> SecondGradientColor (nasprotni rob). BackColor se pri gradientu NE uporablja (velja le za Solid).
        /// </summary>
        static Brush GradientBrush(XElement it, RectangleF r, Color back, bool horizontal)
        {
            back = AttrColor(it, "MiddleGradientColor") ?? back;
            var first = AttrBool(it, "UseFirstGradient", true) ? (AttrColor(it, "FirstGradientColor") ?? back) : back;
            var second = AttrBool(it, "UseSecondGradient", true) ? (AttrColor(it, "SecondGradientColor") ?? back) : back;
            float o1 = Clamp(AttrInt(it, "FirstGradientOffset", 15) / 100f, 0.01f, 0.49f);
            float o2 = Clamp(AttrInt(it, "SecondGradientOffset", 15) / 100f, 0.01f, 0.49f);

            var rr = new RectangleF(r.X, r.Y - 0.5f, r.Width, r.Height + 1f);
            var brush = new LinearGradientBrush(rr, first, second, horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical);
            brush.InterpolationColors = new ColorBlend
            {
                Colors = new[] { first, back, back, second },
                Positions = new[] { 0f, 0.5f, 0.5f, 1f }   // gladek prehod rob -> sredina -> rob (tako ga riše panel)
            };
            return brush;
        }

        /// <summary>
        /// Slikovno stikalo (slike BtnOn/BtnOff iz knjižnice niso v exportu): narisano kot drsno stikalo
        /// v izklopljenem stanju, kot ga prikaže panel - temna kapsula, napis TextOff levo, bel drsnik desno.
        /// </summary>
        static void DrawSwitchTile(Graphics g, XElement it, RectangleF r)
        {
            var pill = RectangleF.Inflate(r, -3, -6);
            float rad = pill.Height / 2f;
            using (var path = RoundedRect(pill, rad))
            using (var b = new SolidBrush(Color.FromArgb(24, 24, 24)))
            using (var pen = new Pen(Color.FromArgb(70, 70, 70), 1.5f))
            {
                g.FillPath(b, path);
                g.DrawPath(pen, path);
            }
            var knob = new RectangleF(pill.Right - pill.Height + 2, pill.Y + 2, pill.Height - 4, pill.Height - 4);
            using (var path = RoundedRect(knob, knob.Height / 3f))
            using (var b = new SolidBrush(Color.White))
                g.FillPath(b, path);
            string text = GetText(it, "TextOff") ?? "OFF";
            var textArea = new RectangleF(pill.X + 2, pill.Y, pill.Width - knob.Width - 4, pill.Height);
            using (var f = new Font("Tahoma", Math.Max(8f, Math.Min(pill.Height * 0.55f, textArea.Width / 2.2f)), FontStyle.Regular, GraphicsUnit.Pixel))
            using (var tb = new SolidBrush(Color.White))
            using (var sf = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(text, f, tb, textArea, sf);
        }

        /// <summary>Slika logotipa ni v exportu - nadomestek: bel napis "Hidria" (kot na panelu).</summary>
        static void DrawHidriaLogo(Graphics g, RectangleF r)
        {
            using (var f = new Font("Arial", Math.Max(8f, r.Height * 0.72f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (var b = new SolidBrush(Color.White))
            using (var sf = new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("Hidria", f, b, r, sf);
        }

        static bool CallsFunction(XElement it, string function)
        {
            return it.Descendants().Any(e => (string)e.Attribute("CompositionName") == "FunctionListEntries" && Attr(e, "Name") == function);
        }

        /// <summary>Gumb za izklop runtime-a (StopRuntime) - na panelu ima ikono "power".</summary>
        static void DrawPowerIcon(Graphics g, RectangleF r)
        {
            var box = RectangleF.Inflate(r, -2, -2);
            using (var path = RoundedRect(box, 4))
            using (var b = new LinearGradientBrush(box, Color.FromArgb(0, 89, 198), Color.FromArgb(0, 48, 107), LinearGradientMode.Vertical))
                g.FillPath(b, path);
            float d = Math.Min(box.Width, box.Height) * 0.45f;
            var c = new RectangleF(box.X + (box.Width - d) / 2, box.Y + (box.Height - d) / 2, d, d);
            using (var pen = new Pen(Color.FromArgb(150, 190, 240), Math.Max(2f, d / 9f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawArc(pen, c, -60, 300);
                g.DrawLine(pen, c.X + d / 2, c.Y - d * 0.12f, c.X + d / 2, c.Y + d * 0.42f);
            }
        }

        static void DrawPicturePlaceholder(Graphics g, XElement it, RectangleF r)
        {
            string pic = it.Elements("LinkList").Elements()
                .Where(e => e.Name.LocalName == "PictureOff" || e.Name.LocalName == "Picture" || e.Name.LocalName == "PictureOn")
                .Select(e => (string)e.Element("Name")).FirstOrDefault();
            var inner = RectangleF.Inflate(r, -3, -3);
            using (var pen = new Pen(Color.FromArgb(120, 128, 128, 128)) { DashStyle = DashStyle.Dot })
            {
                g.DrawRectangle(pen, inner.X, inner.Y, inner.Width, inner.Height);
                g.DrawLine(pen, inner.Left, inner.Top, inner.Right, inner.Bottom);
                g.DrawLine(pen, inner.Right, inner.Top, inner.Left, inner.Bottom);
            }
            if (pic != null)
                using (var f = new Font("Tahoma", 7f))
                using (var b = new SolidBrush(Color.FromArgb(200, 60, 60, 60)))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far, Trimming = StringTrimming.EllipsisCharacter })
                    g.DrawString("[" + pic + "]", f, b, inner, sf);
        }

        static string IoFieldSample(XElement it)
        {
            string pattern = Attr(it, "FormatPattern");
            string s = string.IsNullOrEmpty(pattern) ? "0" : Regex.Replace(pattern, "[9s]", "0");
            if (Attr(it, "DataFormat") == "String") s = "abc";
            string unit = Attr(it, "Unit");
            return string.IsNullOrEmpty(unit) ? s : s + " " + unit;
        }

        static void DrawText(Graphics g, XElement it, RectangleF r, string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var fontEl = Children(it, "Font").FirstOrDefault();
            var fi = PickCulture(fontEl);
            string family = fi != null ? (string)fi.Element("AttributeList").Element("FontFamily") : "Tahoma";
            float size = fi != null ? ParseFloat((string)fi.Element("AttributeList").Element("FontSize"), 12f) : 12f;
            string style = fi != null ? (string)fi.Element("AttributeList").Element("FontStyle") ?? "" : "";
            var fs = FontStyle.Regular;
            if (style.Contains("Bold")) fs |= FontStyle.Bold;
            if (style.Contains("Italic")) fs |= FontStyle.Italic;
            if (style.Contains("Underline")) fs |= FontStyle.Underline;

            var tr = new RectangleF(
                r.X + AttrInt(it, "LeftMargin", 0) + AttrInt(it, "TextAreaLeftMargin", 0),
                r.Y + AttrInt(it, "TopMargin", 0) + AttrInt(it, "TextAreaTopMargin", 0),
                r.Width - AttrInt(it, "LeftMargin", 0) - AttrInt(it, "RightMargin", 0) - AttrInt(it, "TextAreaLeftMargin", 0) - AttrInt(it, "TextAreaRightMargin", 0),
                r.Height - AttrInt(it, "TopMargin", 0) - AttrInt(it, "BottomMargin", 0) - AttrInt(it, "TextAreaTopMargin", 0) - AttrInt(it, "TextAreaBottomMargin", 0));

            using (var font = SafeFont(family, size, fs))
            using (var brush = new SolidBrush(AttrColor(it, "ForeColor") ?? Color.Black))
            using (var sf = (StringFormat)StringFormat.GenericTypographic.Clone())
            {
                sf.Alignment = Align(Attr(it, "HorizontalAlignment"));
                sf.LineAlignment = Align(Attr(it, "VerticalAlignment"));
                sf.Trimming = StringTrimming.None;
                sf.FormatFlags |= StringFormatFlags.NoWrap;   // WinCC lomi samo na eksplicitnih <br>
                if (Attr(it, "TextOrientation") == "Vertical") sf.FormatFlags |= StringFormatFlags.DirectionVertical;
                var state = g.Save();
                g.DrawString(text, font, brush, tr, sf);
                g.Restore(state);
            }
        }

        static Font SafeFont(string family, float size, FontStyle style)
        {
            try { return new Font(family, size, style, GraphicsUnit.Pixel); }
            catch { return new Font("Tahoma", size, style, GraphicsUnit.Pixel); }
        }

        static StringAlignment Align(string a)
        {
            switch (a)
            {
                case "Left": case "Top": return StringAlignment.Near;
                case "Right": case "Bottom": return StringAlignment.Far;
                default: return StringAlignment.Center;
            }
        }

        // ================================================================== Besedila

        /// <summary>Besedilo iz MultilingualText (CompositionName = Text/TextOff/TextOn) v preferenčnem jeziku.</summary>
        static string GetText(XElement it, string composition)
        {
            var mt = Children(it, composition).FirstOrDefault();
            var item = PickCulture(mt);
            if (item == null) return null;
            var textEl = item.Element("AttributeList")?.Element("Text");
            if (textEl == null) return null;

            var sb = new StringBuilder();
            bool firstP = true;
            foreach (var p in textEl.Descendants().Where(e => e.Name.LocalName == "p"))
            {
                if (!firstP) sb.Append('\n');
                firstP = false;
                AppendInline(p, sb);
            }
            if (firstP) sb.Append(textEl.Value);
            return sb.ToString().Trim();
        }

        static void AppendInline(XElement el, StringBuilder sb)
        {
            foreach (var n in el.Nodes())
            {
                var t = n as XText;
                if (t != null) { sb.Append(Regex.Replace(t.Value, @"\s+", " ")); continue; }
                var e = n as XElement;
                if (e == null) continue;
                if (e.Name.LocalName == "br") sb.Append('\n');
                else if (e.Name.LocalName == "field") sb.Append("###");
                else AppendInline(e, sb);
            }
        }

        /// <summary>Iz vsebnika z Items (MultilingualText / MultiLingualFont) izbere element za preferenčni jezik.</summary>
        static XElement PickCulture(XElement container)
        {
            if (container == null) return null;
            var items = Children(container, "Items").ToList();
            return items.FirstOrDefault(i => (string)i.Element("AttributeList")?.Element("Culture") == PreferredCulture)
                ?? items.FirstOrDefault(i => (string)i.Element("AttributeList")?.Element("Culture") == "en-US")
                ?? items.FirstOrDefault();
        }

        // ================================================================== XML pomožno

        static IEnumerable<XElement> Children(XElement el, string compositionName)
        {
            var ol = el.Element("ObjectList");
            if (ol == null) return Enumerable.Empty<XElement>();
            return ol.Elements().Where(e => (string)e.Attribute("CompositionName") == compositionName);
        }

        static string Attr(XElement el, string name)
        {
            var al = el.Element("AttributeList");
            var a = al == null ? null : al.Element(name);
            return a == null ? null : a.Value;
        }

        static int AttrInt(XElement el, string name, int def)
        {
            int v;
            return int.TryParse(Attr(el, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : def;
        }

        static bool AttrBool(XElement el, string name, bool def)
        {
            var s = Attr(el, name);
            return s == null ? def : s.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        static Color? AttrColor(XElement el, string name)
        {
            RgbColor c;
            return RgbColor.TryParse(Attr(el, name), out c) ? c.ToColor() : (Color?)null;
        }

        static float ParseFloat(string s, float def)
        {
            float v;
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : def;
        }

        static float Clamp(float v, float lo, float hi) { return v < lo ? lo : v > hi ? hi : v; }

        static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            if (radius <= 0 || r.Width <= 0 || r.Height <= 0) { p.AddRectangle(r); return p; }
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
