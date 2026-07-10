using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateExcelFile
{
    public class GenerateExcelIO
    {
        string path = "";
        _Application excel = new Application();
        Workbook wb;
        Worksheet ws;

        public GenerateExcelIO(string path, int Sheet)
        {
            this.path = path;
            wb = excel.Workbooks.Open(path);
            ws = wb.Worksheets[Sheet];
        }

        public string ReadCell(int i, int j)
        {
            i++;
            j++;

            if (ws.Cells[i, j].Value2 != null)
                return ws.Cells[i, j].Value2;
            else
                return "";
        }

        public void GenerateIO(RobotModel program)
        {
            int firstFreeByte = 2;
            int n = 0;
            int k = 0;

            for (int i = 0; i < program.Stations.Count; i++)
            {

                if (program.Stations[i].StationFreeEnabled)
                {
                    ws.Cells[18 + k, 1].Value2 = $"ROBOT_O_{program.Stations[i].RobotStationNameToUpper}_FREE";
                    ws.Cells[18 + k, 2].Value2 = "Bool";
                    ws.Cells[18 + k, 3].Value2 = $"{firstFreeByte}.{n}";

                    n++;
                    k++;
                    if (n == 8)
                    {
                        n = 0;
                        firstFreeByte++;
                    }

                }
                ws.Cells[28 + i, 7].Value2 = $"{i}: " + program.Stations[i].RobotStationName;
            }
            ws.Cells[18 + 3 + k, 1].Value2 = "Bytes";
            ws.Cells[18 + 4 + k, 1].Value2 = "ROBOT_O_POSITION_AT";
            ws.Cells[18 + 4 + k, 2].Value2 = "Byte";
            ws.Cells[18 + 4 + k, 3].Value2 = "4.0";
            ws.Cells[18 + 5 + k, 1].Value2 = "ROBOT_O_ADDITIONAL_POS";
            ws.Cells[18 + 5 + k, 2].Value2 = "Byte";
            ws.Cells[18 + 5 + k, 3].Value2 = "5.0";

        }
        public void WriteToCell(int i, int j, string s)
        {
            i++;
            j++;
            ws.Cells[i, j].Value2 = s;
        }

        public void SaveAs(string path)
        {

            wb.SaveAs(path);

        }

        public void Close()
        {
            wb.Close();
        }
    }
}
