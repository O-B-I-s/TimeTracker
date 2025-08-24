using ClosedXML.Excel;
using Core.Entities;

namespace WebApi.Services
{
    public class ExcelExporter
    {
        public byte[] Generate(IEnumerable<TimesheetEntry> entries, DateOnly weekStart)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Timesheet");


            // Headers
            ws.Cell("A1").Value = "DAY";
            ws.Cell("B1").Value = "DATE";
            ws.Cell("C1").Value = "START";
            ws.Cell("D1").Value = "END";
            ws.Cell("E1").Value = "HOURS";
            ws.Cell("F1").Value = "ODO START";
            ws.Cell("G1").Value = "ODO END";
            ws.Cell("H1").Value = "KM";


            var byDate = entries.ToDictionary(e => e.Date, e => e);
            int row = 2;
            for (int i = 0; i < 7; i++)
            {
                var d = weekStart.AddDays(i);
                byDate.TryGetValue(d, out var e);


                ws.Cell(row, 1).Value = d.ToDateTime(TimeOnly.MinValue).ToString("dddd");
                ws.Cell(row, 2).Value = d.ToDateTime(TimeOnly.MinValue).ToString("MMM dd, yyyy");


                // Write real Excel times
                if (e is not null)
                {
                    ws.Cell(row, 3).Value = e.StartTime.ToTimeSpan();
                    ws.Cell(row, 4).Value = e.EndTime.ToTimeSpan();
                    ws.Cell(row, 6).Value = e.OdometerStart;
                    ws.Cell(row, 7).Value = e.OdometerEnd;
                }
                ws.Cell(row, 3).Style.NumberFormat.Format = "hh:mm";
                ws.Cell(row, 4).Style.NumberFormat.Format = "hh:mm";


                // Formulas
                ws.Cell(row, 5).FormulaA1 = $"=IF(AND(C{row}>0,D{row}>0),(D{row}-C{row})*24,0)"; // hours
                ws.Cell(row, 8).FormulaA1 = $"=IF(AND(G{row}>0,F{row}>0),G{row}-F{row},0)"; // km
                row++;
            }


            // Totals
            ws.Cell(row, 4).Value = "TOTALS";
            ws.Cell(row, 5).FormulaA1 = $"=SUM(E2:E{row - 1})";
            ws.Cell(row, 8).FormulaA1 = $"=SUM(H2:H{row - 1})";
            ws.Range($"A1:H1").Style.Font.SetBold();
            ws.Columns().AdjustToContents();


            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
