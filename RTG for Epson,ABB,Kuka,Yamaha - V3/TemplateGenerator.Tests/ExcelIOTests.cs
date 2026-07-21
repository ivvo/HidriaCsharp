using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Xunit;

namespace TemplateGenerator.Tests
{
    // Regresija za popravek Excel IO tabele: pri InlineString celicah je prej ostal nastavljen tudi
    // <v> (CellValue), kar je Excel javljal kot "nečitljiva vsebina / popravljeno". Poleg tega so bili
    // byte.bit naslovi (npr. "3.0") zapisani kot Number in skrčeni na "3". Popravek: InlineString +
    // CellValue = null; byte.bit gre skozi UpdateCellText (ostane besedilo).
    public class ExcelIOTests : System.IDisposable
    {
        private readonly string _outDir = TestHelpers.CreateTempDir();
        public void Dispose() => TestHelpers.DeleteDirSafely(_outDir);

        [Fact]
        public void GeneratedIOTable_HasNoInlineStringCellWithStaleValue()
        {
            // ABB zapiše IOTable.xlsx neposredno v izbrano mapo in uporablja byte.bit naslove.
            TestHelpers.BuildAndGenerate("ABB Hidria", _outDir, ("Station1", true, 1), ("Station2", true, 1));
            string xlsx = Path.Combine(_outDir, "IOTable.xlsx");
            Assert.True(File.Exists(xlsx), "IOTable.xlsx bi morala obstajati.");

            int inlineCells = 0;
            bool foundByteBit = false;

            using (var doc = SpreadsheetDocument.Open(xlsx, false))
            {
                foreach (var wsPart in doc.WorkbookPart!.WorksheetParts)
                {
                    foreach (var cell in wsPart.Worksheet.Descendants<Cell>())
                    {
                        if (cell.DataType != null && cell.DataType.Value == CellValues.InlineString)
                        {
                            inlineCells++;
                            // Ključ popravka: InlineString celica NE sme imeti hkrati <v> (CellValue).
                            Assert.True(cell.CellValue == null,
                                "InlineString celica ima še vedno nastavljen CellValue (<v>) - to je vzrok korupcije.");

                            string? text = cell.InlineString?.Text?.Text;
                            if (text == "3.0") foundByteBit = true; // byte.bit ostane besedilo, ne število
                        }
                    }
                }
            }

            Assert.True(inlineCells > 0, "Pričakovali smo vsaj nekaj InlineString celic (imena signalov ipd.).");
            Assert.True(foundByteBit, "Byte.bit naslov \"3.0\" bi moral biti zapisan kot besedilo (InlineString).");
        }
    }
}
