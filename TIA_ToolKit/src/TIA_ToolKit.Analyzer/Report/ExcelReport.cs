using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace TIA_ToolKit.Analyzer
{
    /// <summary>
    /// Excel poročilo v enaki obliki kot ročne analize: Povzetek (formule), Napake (tabela tblNapake), Legenda,
    /// skrit list Seznami z imenovanim obsegom SeznamStatus (spustni seznam deluje ne glede na ločilo seznama v Windows nastavitvah).
    /// Ob ponovnem zagonu se Status in Opomba ohranita po stabilnem ključu (pravilo + lokacija).
    /// </summary>
    public static class ExcelReport
    {
        static readonly string[] Statuses = { "Odprto", "V delu", "Popravljeno", "Ni napaka", "Odloženo" };
        static readonly string[] Headers =
            { "ID", "Kategorija", "Prioriteta", "Zaupanje", "Lokacija v projektu", "Network", "Naslov networka", "Opis napake", "Pravilo", "Status", "Opomba / popravek", "Ključ" };

        public static string PriorityText(Severity s)
        {
            switch (s)
            {
                case Severity.Critical: return "1 - Kritično";
                case Severity.High: return "2 - Visoka";
                case Severity.Medium: return "3 - Srednja";
                default: return "4 - Nizka";
            }
        }

        public static int Write(AnalysisResult res, string path)
        {
            var keep = ReadExisting(path);
            int kept = 0;
            using (var wb = new XLWorkbook())
            {
                var sum = wb.Worksheets.Add("Povzetek");
                var ws = wb.Worksheets.Add("Napake");
                var leg = wb.Worksheets.Add("Legenda");
                var lst = wb.Worksheets.Add("Seznami");

                // --- Seznami (skrit) + imenovan obseg
                for (int i = 0; i < Statuses.Length; i++) lst.Cell(i + 1, 1).Value = Statuses[i];
                wb.DefinedNames.Add("SeznamStatus", lst.Range(1, 1, Statuses.Length, 1));
                lst.Visibility = XLWorksheetVisibility.Hidden;

                // --- Napake
                for (int c = 0; c < Headers.Length; c++) ws.Cell(1, c + 1).Value = Headers[c];
                int r = 2;
                foreach (var f in res.Findings)
                {
                    string status = "Odprto", note = "";
                    if (keep.TryGetValue(f.StableKey, out var k)) { status = k.Item1 ?? status; note = k.Item2 ?? ""; if (status != "Odprto" || note != "") kept++; }
                    ws.Cell(r, 1).Value = f.Id;
                    ws.Cell(r, 2).Value = f.Category;
                    ws.Cell(r, 3).Value = PriorityText(f.Severity);
                    ws.Cell(r, 4).Value = f.Confidence;
                    ws.Cell(r, 5).Value = string.Join("\n", f.Locations.Select(l => l.Path));
                    ws.Cell(r, 6).SetValue(string.Join("\n", f.Locations.Select(l => l.Networks)));   // tekst, da "20–36" ne postane datum
                    ws.Cell(r, 7).Value = string.Join("\n", f.Locations.Select(l => l.NetworkTitle));
                    ws.Cell(r, 8).Value = f.Title + ": " + f.Description;
                    ws.Cell(r, 9).Value = f.RuleId;
                    ws.Cell(r, 10).Value = status;
                    ws.Cell(r, 11).Value = note;
                    ws.Cell(r, 12).Value = f.StableKey;
                    r++;
                }
                int last = Math.Max(2, r - 1);
                var table = ws.Range(1, 1, last, Headers.Length).CreateTable("tblNapake");
                table.Theme = XLTableTheme.TableStyleMedium2;
                double[] widths = { 6, 16, 13, 12, 58, 10, 34, 80, 15, 13, 40, 10 };
                for (int c = 0; c < widths.Length; c++) ws.Column(c + 1).Width = widths[c];
                ws.Column(12).Hide();
                ws.Range(2, 5, last, 8).Style.Alignment.WrapText = true;
                ws.Range(2, 11, last, 11).Style.Alignment.WrapText = true;
                ws.Range(1, 1, last, Headers.Length).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                ws.Range(1, 1, last, Headers.Length).Style.Font.FontName = "Arial";
                ws.Range(1, 1, last, Headers.Length).Style.Font.FontSize = 10;
                ws.Range(2, 5, last, 5).Style.Font.FontName = "Consolas";
                ws.Range(2, 5, last, 5).Style.Font.FontSize = 9;
                ws.SheetView.FreezeRows(1);
                ws.SheetView.FreezeColumns(1);

                var dv = ws.Range(2, 10, last, 10).CreateDataValidation();
                dv.List("=SeznamStatus", true);

                var prio = ws.Range(2, 3, last, 3);
                prio.AddConditionalFormat().WhenEquals("\"1 - Kritično\"").Fill.SetBackgroundColor(XLColor.FromArgb(255, 153, 153));
                prio.AddConditionalFormat().WhenEquals("\"2 - Visoka\"").Fill.SetBackgroundColor(XLColor.FromArgb(255, 204, 153));
                prio.AddConditionalFormat().WhenEquals("\"3 - Srednja\"").Fill.SetBackgroundColor(XLColor.FromArgb(255, 255, 153));
                prio.AddConditionalFormat().WhenEquals("\"4 - Nizka\"").Fill.SetBackgroundColor(XLColor.FromArgb(204, 255, 204));
                var st = ws.Range(2, 10, last, 10);
                st.AddConditionalFormat().WhenEquals("\"Popravljeno\"").Fill.SetBackgroundColor(XLColor.FromArgb(198, 239, 206));
                st.AddConditionalFormat().WhenEquals("\"V delu\"").Fill.SetBackgroundColor(XLColor.FromArgb(255, 235, 156));
                st.AddConditionalFormat().WhenEquals("\"Ni napaka\"").Fill.SetBackgroundColor(XLColor.FromArgb(217, 217, 217));
                st.AddConditionalFormat().WhenEquals("\"Odloženo\"").Fill.SetBackgroundColor(XLColor.FromArgb(217, 217, 217));
                ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                ws.PageSetup.FitToPages(1, 0);
                ws.PageSetup.SetRowsToRepeatAtTop(1, 1);

                // --- Povzetek
                WriteSummary(sum, res);

                // --- Legenda
                var rows = new List<string[]>
                {
                    new[] { "Stolpec / vrednost", "Pomen" },
                    new[] { "Prioriteta 1 - Kritično", "Safety – pregled z varnostnim inženirjem in ocena tveganja pred popravkom." },
                    new[] { "Prioriteta 2 - Visoka", "Vpliva na kakovost izdelka ali varnost delovanja stroja." },
                    new[] { "Prioriteta 3 - Srednja", "Napaka indeksa / kopiranja, diagnostika – hiter popravek." },
                    new[] { "Prioriteta 4 - Nizka", "Kozmetično ali brez trenutnega učinka." },
                    new[] { "Zaupanje: Preverjeno", "Pravilo je napako zaznalo nedvoumno." },
                    new[] { "Zaupanje: Preveriti", "Pravilo je našlo kandidata; ali je to namerno, presodi človek (ali LLM)." },
                    new[] { "Lokacija / Network", "Pot v drevesu Program blocks + [tip in številka bloka]; številka in naslov networka kot v TIA. Prazen network = velja za cel blok." },
                    new[] { "Pravilo", "ID pravila analizatorja (opis: docs\\ANALYZER_SPEC.md v TIA_ToolKit)." },
                    new[] { "Status / Opomba", "Urejaš ti. Ob ponovni analizi istega exporta se ohranita (po skritem stolpcu Ključ)." },
                    new[] { "Česa ni bilo mogoče preveriti", res.Project.NotExported.Count == 0 ? "–" : string.Join("; ", res.Project.NotExported) },
                };
                for (int i = 0; i < rows.Count; i++) { leg.Cell(i + 1, 1).Value = rows[i][0]; leg.Cell(i + 1, 2).Value = rows[i][1]; }
                leg.Range(1, 1, 1, 2).Style.Font.Bold = true;
                leg.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 79, 127);
                leg.Range(1, 1, 1, 2).Style.Font.FontColor = XLColor.White;
                leg.Column(1).Width = 30; leg.Column(2).Width = 110;
                leg.Range(1, 2, rows.Count, 2).Style.Alignment.WrapText = true;
                leg.Style.Font.FontName = "Arial";

                ws.SetTabActive();
                wb.SaveAs(path);
            }
            return kept;
        }

        static void WriteSummary(IXLWorksheet s, AnalysisResult res)
        {
            s.Style.Font.FontName = "Arial";
            s.Cell(1, 1).Value = "Analiza napak – " + res.Project.Name + " (TIA_ToolKit, samodejna analiza)";
            s.Cell(1, 1).Style.Font.FontSize = 14; s.Cell(1, 1).Style.Font.Bold = true;
            s.Cell(2, 1).Value = "Vir: " + res.Project.Root + (res.Project.ExportedAt != null ? " (export " + res.Project.ExportedAt + ")" : "") + ". Analiza: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + ".";
            s.Cell(2, 1).Style.Font.Italic = true;

            string[] hdr = { "Kategorija", "Skupaj", "Odprto", "V delu", "Popravljeno", "Ni napaka", "Odloženo", "% zaključeno" };
            for (int c = 0; c < hdr.Length; c++) s.Cell(4, c + 1).Value = hdr[c];
            int r = 5;
            foreach (var cat in Categories.Order)
            {
                s.Cell(r, 1).Value = cat;
                s.Cell(r, 2).FormulaA1 = "COUNTIF(tblNapake[Kategorija],$A" + r + ")";
                for (int j = 0; j < 5; j++)
                {
                    string col = ((char)('C' + j)).ToString();
                    s.Cell(r, 3 + j).FormulaA1 = "COUNTIFS(tblNapake[Kategorija],$A" + r + ",tblNapake[Status]," + col + "$4)";
                }
                s.Cell(r, 8).FormulaA1 = "IF(B" + r + "=0,0,(E" + r + "+F" + r + ")/B" + r + ")";
                r++;
            }
            s.Cell(r, 1).Value = "SKUPAJ";
            for (int c = 2; c <= 7; c++) { string col = ((char)('A' + c - 1)).ToString(); s.Cell(r, c).FormulaA1 = "SUM(" + col + "5:" + col + (r - 1) + ")"; }
            s.Cell(r, 8).FormulaA1 = "IF(B" + r + "=0,0,(E" + r + "+F" + r + ")/B" + r + ")";
            s.Range(5, 8, r, 8).Style.NumberFormat.Format = "0%";
            Header(s.Range(4, 1, 4, 8));
            s.Range(r, 1, r, 8).Style.Font.Bold = true;

            int p0 = r + 2;
            s.Cell(p0, 1).Value = "Prioriteta"; s.Cell(p0, 2).Value = "Skupaj"; s.Cell(p0, 3).Value = "Še odprto";
            Header(s.Range(p0, 1, p0, 3));
            string[] pr = { "1 - Kritično", "2 - Visoka", "3 - Srednja", "4 - Nizka" };
            for (int i = 0; i < pr.Length; i++)
            {
                int rr = p0 + 1 + i;
                s.Cell(rr, 1).Value = pr[i];
                s.Cell(rr, 2).FormulaA1 = "COUNTIF(tblNapake[Prioriteta],$A" + rr + ")";
                s.Cell(rr, 3).FormulaA1 = "COUNTIFS(tblNapake[Prioriteta],$A" + rr + ",tblNapake[Status],\"Odprto\")+COUNTIFS(tblNapake[Prioriteta],$A" + rr + ",tblNapake[Status],\"V delu\")";
            }
            s.Column(1).Width = 28;
            for (int c = 2; c <= 8; c++) s.Column(c).Width = 13;
        }

        static void Header(IXLRange r)
        {
            r.Style.Font.Bold = true;
            r.Style.Fill.BackgroundColor = XLColor.FromArgb(31, 79, 127);
            r.Style.Font.FontColor = XLColor.White;
        }

        /// <summary>Status/Opomba iz obstoječega poročila, po ključu.</summary>
        static Dictionary<string, Tuple<string, string>> ReadExisting(string path)
        {
            var d = new Dictionary<string, Tuple<string, string>>();
            if (!File.Exists(path)) return d;
            try
            {
                using (var wb = new XLWorkbook(path))
                {
                    if (!wb.TryGetWorksheet("Napake", out var ws)) return d;
                    var hdr = ws.Row(1).CellsUsed().ToDictionary(c => c.GetString(), c => c.Address.ColumnNumber);
                    if (!hdr.TryGetValue("Ključ", out var kc) || !hdr.TryGetValue("Status", out var sc)) return d;
                    hdr.TryGetValue("Opomba / popravek", out var oc);
                    foreach (var row in ws.RowsUsed().Skip(1))
                    {
                        string key = row.Cell(kc).GetString();
                        if (key.Length > 0) d[key] = Tuple.Create(row.Cell(sc).GetString(), oc > 0 ? row.Cell(oc).GetString() : "");
                    }
                }
            }
            catch { /* odprto v Excelu ali poškodovano - začnemo na novo */ }
            return d;
        }
    }
}
