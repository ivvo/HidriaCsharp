using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using TemplateGenerator.Core.Models;

namespace TemplateGenerator.Core.Classes
{
    public class GenerateExcelIO
    {
        //public GenerateExcelIO(string path)
        //{
        //    File.Copy("./Templates/templateIO.xlsx", "C:/Users/ivoval/Desktop/aa/templateIO.xlsx",true);
        //}
        public void generatefile (string template, string path)
        {
            
            switch (template)
            {
                case "Epson":
                    File.Copy("./Templates/Epson/templateIO.xlsx", path, true);
                    break;

                case "ABB":
                    File.Copy("./Templates/ABB Hidria/templateIO.xlsx", path, true);
                    break;

                default:
                    //exception, error
                    break;
            }
            
        }

        public void GenerateIO(string template, ProgramModel program, string path)
        {
            // Kawasaki ima svojo IO predlogo z drugačnim listom (Sheet1) in postavitvijo stolpcev kot
            // Epson/ABB (List1), zato ga obravnavamo posebej.
            if (template == "Kawasaki") { GenerateKawasakiIO(program, path); return; }

            uint firstFreeByte = 0;
            uint n = 0;
            uint k = 0;
            uint line = 0;
            uint robPos = 0;

            generatefile(template,path + "//" + "templateIO.xlsx");
            
            switch (template)
            {
                case "Epson":
                    firstFreeByte = 2;
                    line = 18;
                    robPos = 28;
                    break;

                case "ABB":
                    firstFreeByte = 3;
                    line = 22;
                    robPos = 33;
                    break;
                    
                default:
                    //exception, error
                    break;
            }
            
            for (int i = 0; i < program.Stations.Count; i++)
            {

                if (program.Stations[i].StationFreeEnabled)
                {
                    UpdateCell(path + "//" + "templateIO.xlsx", $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE", line + k, "A");
                    UpdateCell(path + "//" + "templateIO.xlsx", $"Bool", line + k, "B");
                    UpdateCell(path + "//" + "templateIO.xlsx", $"{firstFreeByte}.{n}", line + k, "C");

                    n++;
                    k++;
                    if (n == 8)
                    {
                        n = 0;
                        firstFreeByte++;
                    }
                }
                UpdateCell(path + "//" + "templateIO.xlsx", $"{i}: " + program.Stations[i].RobotStationName, robPos + (uint)i, "G");
                // ws.Cells[28 + i, 7].Value2 = $"{i}: " + program.Stations[i].RobotStationName;
            }

            UpdateCell(path + "//" + "templateIO.xlsx", "Bytes", line + 3 + k, "A");
            UpdateCell(path + "//" + "templateIO.xlsx", "ROBOT_O_POSITION_AT", line + 4 + k, "A");
            UpdateCell(path + "//" + "templateIO.xlsx", "Byte", line + 4 + k, "B");
            UpdateCell(path + "//" + "templateIO.xlsx", "4.0" , line + 4 + k, "C");
            UpdateCell(path + "//" + "templateIO.xlsx", "ROBOT_O_ADDITIONAL_POS", line + 5 + k, "A");
            UpdateCell(path + "//" + "templateIO.xlsx", "Byte", line + 5 + k, "B");
            UpdateCell(path + "//" + "templateIO.xlsx", "5.0", line + 5 + k, "C");
        }

        // Kawasaki IO tabela: svoja predloga (IO_template_KAWASAKI.xlsx -> IOTable.xlsx), list "Sheet1",
        // stolpci C/D (free signali) in P/Q/R (ime postaje / indeks / št. pozicij). Postavitev je enaka
        // kot v V3, da je izhod skladen med projektoma.
        private void GenerateKawasakiIO(ProgramModel program, string path)
        {
            string file = path + "//" + "IOTable.xlsx";
            File.Copy("./Templates/Kawasaki/IO_template_KAWASAKI.xlsx", file, true);

            uint line = 59;
            uint robPos = 3;
            uint k = 0;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].StationFreeEnabled)
                {
                    UpdateCellOnSheet(file, "Sheet1", $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE", line + k, "C");
                    UpdateCellOnSheet(file, "Sheet1", "Bool", line + k, "D");
                    k++;
                }

                if (i > 0)
                {
                    UpdateCellOnSheet(file, "Sheet1", program.Stations[i].RobotStationName, robPos + (uint)i, "P");
                    UpdateCellOnSheet(file, "Sheet1", $"{i}", robPos + (uint)i, "Q");
                }

                if (program.Stations[i].Positions == 1)
                    UpdateCellOnSheet(file, "Sheet1", "/", robPos + (uint)i, "R");
                else
                    UpdateCellOnSheet(file, "Sheet1", $"{program.Stations[i].Positions}", robPos + (uint)i, "R");
            }
        }

        public static void UpdateCell(string docName, string text, uint rowIndex, string columnName)
            => UpdateCellOnSheet(docName, "List1", text, rowIndex, columnName);

        public static void UpdateCellOnSheet(string docName, string sheetName, string text, uint rowIndex, string columnName)
        {

            // Open the document for editing.
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                WorksheetPart worksheetPart = GetWorksheetPartByName(spreadSheet, sheetName);

                if (worksheetPart != null)
                {
                    Cell cell = GetCell(worksheetPart.Worksheet, columnName, rowIndex);

                    // Vse vrednosti tu (imena signalov, "0: Home", naslovi v obliki byte.bit kot
                    // "2.1"/"4.0") so besedilo, ne pravo število - zapis kot CellValues.Number je
                    // povzročal opozorilo o poškodovani datoteki (Excel poskusi razčleniti npr.
                    // "0: Home" kot število) in prikaz "2.1" po regionalnih nastavitvah kot "2,1".
                    cell.CellValue = null;
                    cell.DataType = new EnumValue<CellValues>(CellValues.InlineString);
                    cell.InlineString = new InlineString(new Text(text));

                    // Save the worksheet.
                    worksheetPart.Worksheet.Save();
                }
            }
        }

        private static WorksheetPart GetWorksheetPartByName(SpreadsheetDocument document, string sheetName)
        {
            IEnumerable<Sheet> sheets =
               document.WorkbookPart.Workbook.GetFirstChild<Sheets>().
               Elements<Sheet>().Where(s => s.Name == sheetName);

            if (sheets.Count() == 0)
            {
                // The specified worksheet does not exist.
                return null;
            }

            string relationshipId = sheets.First().Id.Value;
            WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
            return worksheetPart;
        }

        // Given a worksheet, a column name, and a row index, 
        // gets the cell at the specified column and 
        private static Cell GetCell(Worksheet worksheet, string columnName, uint rowIndex)
        {
            Row row = GetRow(worksheet, rowIndex);

            if (row == null)
                return null;

            return row.Elements<Cell>().Where(c => string.Compare(c.CellReference.Value, columnName + rowIndex, true) == 0).First();
        }

        // Given a worksheet and a row index, return the row.
        private static Row GetRow(Worksheet worksheet, uint rowIndex)
        {
            return worksheet.GetFirstChild<SheetData>().
            Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
        }
    }
}

