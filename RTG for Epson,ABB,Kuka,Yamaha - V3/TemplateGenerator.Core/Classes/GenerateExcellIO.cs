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
        public void generatefile(string template, string path)
        {

            switch (template)
            {
                case "Epson":
                    //File.Copy("./Templates/Epson/templateIO.xlsx", path, true);
                    File.Copy("./Templates/Epson/IO_template_EPSON.xlsx", path, true);
                    break;

                case "ABB":
                    File.Copy("./Templates/ABB Hidria/templateIO.xlsx", path, true);
                    break;

                case "Yamaha":
                    File.Copy("./Templates/Yamaha/IO_template_YAMAHA.xlsx", path, true);
                    break;

                case "Kawasaki":
                    File.Copy("./Templates/Kawasaki/IO_template_KAWASAKI.xlsx", path, true);
                    break;

                default:
                    //exception, error
                    break;
            }

        }

        public void GenerateIO(string template, ProgramModel program, string path)
        {
            uint firstFreeByte = 0;
            uint n = 0;
            uint k = 0;
            uint line = 0;
            uint robPos = 0;

            generatefile(template, path + "//" + "IOTable.xlsx");

            switch (template)
            {
                case "Epson":

                    //firstFreeByte = 2;
                    line = 19;
                    robPos = 3;

                    for (int i = 0; i < program.Stations.Count; i++)
                    {

                        if (program.Stations[i].StationFreeEnabled)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE", line + k, "C");
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"Bool", line + k, "D");
                            //UpdateCell(path + "//" + "IOTable.xlsx", $"{firstFreeByte}.{n}", line + k, "C");

                            //n++;
                            k++;
                            //if (n == 8)
                            //{
                            //    n = 0;
                            //    firstFreeByte++;
                            //}
                        }

                        if (i > 0)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", program.Stations[i].RobotStationName, robPos + (uint)i, "T");
                            UpdateCellNumber(path + "//" + "IOTable.xlsx", $"{i}", robPos + (uint)i, "U");
                            // ws.Cells[28 + i, 7].Value2 = $"{i}: " + program.Stations[i].RobotStationName;
                        }

                        if (program.Stations[i].Positions == 1)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"/", robPos + (uint)i, "V");
                        }
                        else
                        {
                            UpdateCellNumber(path + "//" + "IOTable.xlsx", $"{program.Stations[i].Positions}", robPos + (uint)i, "V");
                        }

                    }

                    //UpdateCell(path + "//" + "IOTable.xlsx", "Bytes", line + 3 + k, "A");
                    //UpdateCell(path + "//" + "IOTable.xlsx", "ROBOT_O_POSITION_AT", line + 4 + k, "A");
                    //UpdateCell(path + "//" + "IOTable.xlsx", "Byte", line + 4 + k, "B");
                    //UpdateCell(path + "//" + "IOTable.xlsx", "4.0", line + 4 + k, "C");
                    //UpdateCell(path + "//" + "IOTable.xlsx", "ROBOT_O_ADDITIONAL_POS", line + 5 + k, "A");
                    //UpdateCell(path + "//" + "IOTable.xlsx", "Byte", line + 5 + k, "B");
                    //UpdateCell(path + "//" + "IOTable.xlsx", "5.0", line + 5 + k, "C");
                    break;

                case "ABB":

                    firstFreeByte = 3;
                    line = 22;
                    robPos = 33;

                    for (int i = 0; i < program.Stations.Count; i++)
                    {

                        if (program.Stations[i].StationFreeEnabled)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE", line + k, "A");
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"Bool", line + k, "B");
                            UpdateCellNumber(path + "//" + "IOTable.xlsx", $"{firstFreeByte}.{n}", line + k, "C");

                            n++;
                            k++;
                            if (n == 8)
                            {
                                n = 0;
                                firstFreeByte++;
                            }
                        }
                        UpdateCellText(path + "//" + "IOTable.xlsx", $"{i}: " + program.Stations[i].RobotStationName, robPos + (uint)i, "G");
                        // ws.Cells[28 + i, 7].Value2 = $"{i}: " + program.Stations[i].RobotStationName;
                    }

                    UpdateCellText(path + "//" + "IOTable.xlsx", "Bytes", line + 3 + k, "A");
                    UpdateCellText(path + "//" + "IOTable.xlsx", "ROBOT_O_POSITION_AT", line + 4 + k, "A");
                    UpdateCellText(path + "//" + "IOTable.xlsx", "Byte", line + 4 + k, "B");
                    UpdateCellText(path + "//" + "IOTable.xlsx", "4.0", line + 4 + k, "C");
                    UpdateCellText(path + "//" + "IOTable.xlsx", "ROBOT_O_ADDITIONAL_POS", line + 5 + k, "A");
                    UpdateCellText(path + "//" + "IOTable.xlsx", "Byte", line + 5 + k, "B");
                    UpdateCellText(path + "//" + "IOTable.xlsx", "5.0", line + 5 + k, "C");
                    break;

                case "Yamaha":

                    //firstFreeByte = 2;
                    line = 51;
                    robPos = 3;

                    for (int i = 0; i < program.Stations.Count; i++)
                    {

                        if (program.Stations[i].StationFreeEnabled)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE", line + k, "C");
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"Bool", line + k, "D");
                            //UpdateCell(path + "//" + "IOTable.xlsx", $"{firstFreeByte}.{n}", line + k, "C");

                            //n++;
                            k++;
                            //if (n == 8)
                            //{
                            //    n = 0;
                            //    firstFreeByte++;
                            //}
                        }

                        if (i > 0)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", program.Stations[i].RobotStationName, robPos + (uint)i, "N");
                            UpdateCellNumber(path + "//" + "IOTable.xlsx", $"{i}", robPos + (uint)i, "O");
                            // ws.Cells[28 + i, 7].Value2 = $"{i}: " + program.Stations[i].RobotStationName;
                        }

                        if (program.Stations[i].Positions == 1)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"/", robPos + (uint)i, "P"); 
                        }
                        else
                        {
                            UpdateCellNumber(path + "//" + "IOTable.xlsx", $"{program.Stations[i].Positions}", robPos + (uint)i, "P");
                        }

                    }

                    break;

                case "Kawasaki":

                    //firstFreeByte = 2;
                    line = 59;
                    robPos = 3;

                    for (int i = 0; i < program.Stations.Count; i++)
                    {

                        if (program.Stations[i].StationFreeEnabled)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE", line + k, "C");
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"Bool", line + k, "D");
                            //UpdateCell(path + "//" + "IOTable.xlsx", $"{firstFreeByte}.{n}", line + k, "C");

                            //n++;
                            k++;
                            //if (n == 8)
                            //{
                            //    n = 0;
                            //    firstFreeByte++;
                            //}
                        }

                        if (i > 0)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", program.Stations[i].RobotStationName, robPos + (uint)i, "P");
                            UpdateCellNumber(path + "//" + "IOTable.xlsx", $"{i}", robPos + (uint)i, "Q");
                            // ws.Cells[28 + i, 7].Value2 = $"{i}: " + program.Stations[i].RobotStationName;
                        }

                        if (program.Stations[i].Positions == 1)
                        {
                            UpdateCellText(path + "//" + "IOTable.xlsx", $"/", robPos + (uint)i, "R");
                        }
                        else
                        {
                            UpdateCellNumber(path + "//" + "IOTable.xlsx", $"{program.Stations[i].Positions}", robPos + (uint)i, "R");
                        }

                    }

                    break;

                default:
                    //exception, error
                    break;
            }

        }

        public static void UpdateCellText(string docName, string text, uint rowIndex, string columnName)
        {

            // Open the document for editing.
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                WorksheetPart worksheetPart = GetWorksheetPartByName(spreadSheet, "Sheet1");

                if (worksheetPart != null)
                {
                    Cell cell = GetCell(worksheetPart.Worksheet, columnName, rowIndex);

                    cell.CellValue = new CellValue(text);

                    //cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    cell.DataType = new EnumValue<CellValues>(CellValues.InlineString);
                    cell.InlineString = new InlineString() { Text = new Text(text) };

                    // Save the worksheet.
                    worksheetPart.Worksheet.Save();
                }
            }
        }

        public static void UpdateCellNumber(string docName, string text, uint rowIndex, string columnName)
        {

            // Open the document for editing.
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                WorksheetPart worksheetPart = GetWorksheetPartByName(spreadSheet, "Sheet1");

                if (worksheetPart != null)
                {
                    Cell cell = GetCell(worksheetPart.Worksheet, columnName, rowIndex);

                    cell.CellValue = new CellValue(text);

                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    //cell.DataType = new EnumValue<CellValues>(CellValues.InlineString);
                    //cell.InlineString = new InlineString() { Text = new Text(text) };

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

            return row.Elements<Cell>().Where(c => string.Compare(c.CellReference.Value, columnName + rowIndex, true) == 0).Single();
        }

        // Given a worksheet and a row index, return the row.
        private static Row GetRow(Worksheet worksheet, uint rowIndex)
        {
            return worksheet.GetFirstChild<SheetData>().
            Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
        }
    }



    //public class GenerateExcelIO
    //{
    //    //public GenerateExcelIO(string path)
    //    //{
    //    //    File.Copy("./Templates/templateIO.xlsx", "C:/Users/ivoval/Desktop/aa/templateIO.xlsx",true);
    //    //}
    //    public void generatefile(string template, string path)
    //    {

    //        switch (template)
    //        {
    //            case "Epson":
    //                File.Copy("./Templates/Epson/templateIO.xlsx", path, true);
    //                break;

    //            case "ABB":
    //                File.Copy("./Templates/ABB Hidria/templateIO.xlsx", path, true);
    //                break;

    //            default:
    //                //exception, error
    //                break;
    //        }

    //    }

    //    public void GenerateIO(string template, ProgramModel program, string path)
    //    {
    //        uint firstFreeByte = 0;
    //        uint n = 0;
    //        uint k = 0;
    //        uint line = 0;
    //        uint robPos = 0;

    //        generatefile(template, path + "//" + "templateIO.xlsx");

    //        switch (template)
    //        {
    //            case "Epson":
    //                firstFreeByte = 2;
    //                line = 18;
    //                robPos = 28;
    //                break;

    //            case "ABB":
    //                firstFreeByte = 3;
    //                line = 22;
    //                robPos = 33;
    //                break;

    //            default:
    //                //exception, error
    //                break;
    //        }

    //        for (int i = 0; i < program.Stations.Count; i++)
    //        {

    //            if (program.Stations[i].StationFreeEnabled)
    //            {
    //                UpdateCell(path + "//" + "templateIO.xlsx", $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE", line + k, "A");
    //                UpdateCell(path + "//" + "templateIO.xlsx", $"Bool", line + k, "B");
    //                UpdateCell(path + "//" + "templateIO.xlsx", $"{firstFreeByte}.{n}", line + k, "C");

    //                n++;
    //                k++;
    //                if (n == 8)
    //                {
    //                    n = 0;
    //                    firstFreeByte++;
    //                }
    //            }
    //            UpdateCell(path + "//" + "templateIO.xlsx", $"{i}: " + program.Stations[i].RobotStationName, robPos + (uint)i, "G");
    //            // ws.Cells[28 + i, 7].Value2 = $"{i}: " + program.Stations[i].RobotStationName;
    //        }

    //        UpdateCell(path + "//" + "templateIO.xlsx", "Bytes", line + 3 + k, "A");
    //        UpdateCell(path + "//" + "templateIO.xlsx", "ROBOT_O_POSITION_AT", line + 4 + k, "A");
    //        UpdateCell(path + "//" + "templateIO.xlsx", "Byte", line + 4 + k, "B");
    //        UpdateCell(path + "//" + "templateIO.xlsx", "4.0", line + 4 + k, "C");
    //        UpdateCell(path + "//" + "templateIO.xlsx", "ROBOT_O_ADDITIONAL_POS", line + 5 + k, "A");
    //        UpdateCell(path + "//" + "templateIO.xlsx", "Byte", line + 5 + k, "B");
    //        UpdateCell(path + "//" + "templateIO.xlsx", "5.0", line + 5 + k, "C");
    //    }

    //    public static void UpdateCell(string docName, string text, uint rowIndex, string columnName)
    //    {

    //        // Open the document for editing.
    //        using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
    //        {
    //            WorksheetPart worksheetPart = GetWorksheetPartByName(spreadSheet, "List1");

    //            if (worksheetPart != null)
    //            {
    //                Cell cell = GetCell(worksheetPart.Worksheet, columnName, rowIndex);

    //                cell.CellValue = new CellValue(text);

    //                cell.DataType = new EnumValue<CellValues>(CellValues.Number);

    //                // Save the worksheet.
    //                worksheetPart.Worksheet.Save();
    //            }
    //        }
    //    }

    //    private static WorksheetPart GetWorksheetPartByName(SpreadsheetDocument document, string sheetName)
    //    {
    //        IEnumerable<Sheet> sheets =
    //           document.WorkbookPart.Workbook.GetFirstChild<Sheets>().
    //           Elements<Sheet>().Where(s => s.Name == sheetName);

    //        if (sheets.Count() == 0)
    //        {
    //            // The specified worksheet does not exist.
    //            return null;
    //        }

    //        string relationshipId = sheets.First().Id.Value;
    //        WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
    //        return worksheetPart;
    //    }

    //    // Given a worksheet, a column name, and a row index, 
    //    // gets the cell at the specified column and 
    //    private static Cell GetCell(Worksheet worksheet, string columnName, uint rowIndex)
    //    {
    //        Row row = GetRow(worksheet, rowIndex);

    //        if (row == null)
    //            return null;

    //        return row.Elements<Cell>().Where(c => string.Compare(c.CellReference.Value, columnName + rowIndex, true) == 0).First();
    //    }

    //    // Given a worksheet and a row index, return the row.
    //    private static Row GetRow(Worksheet worksheet, uint rowIndex)
    //    {
    //        return worksheet.GetFirstChild<SheetData>().
    //        Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
    //    }
    //}
}

