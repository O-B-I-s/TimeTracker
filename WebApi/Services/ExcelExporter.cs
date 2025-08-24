using ClosedXML.Excel;
using Core.Entities;

namespace WebApi.Services
{
    public class ExcelExporter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExcelExporter> _logger;

        public ExcelExporter(IWebHostEnvironment env, ILogger<ExcelExporter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public byte[] Generate(IEnumerable<TimesheetEntry> entries, string employeeName, string employeeId, string location, string department)
        {
            // Load the template
            var templatePath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "templates", "TimesheetTemplate.xlsx");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Template not found at {Path}", templatePath);
                throw new FileNotFoundException("Excel template not found", templatePath);
            }

            using var wb = new XLWorkbook(templatePath);
            var ws = wb.Worksheet(1);

            // Fill employee info
            ws.Cell("B3").Value = employeeName;
            ws.Cell("F3").Value = location;
            ws.Cell("B4").Value = employeeId;
            ws.Cell("F4").Value = department;

            // Determine the week's Sunday from the entries
            if (!entries.Any())
            {
                _logger.LogWarning("No entries provided for export");
                using var emptyMs = new MemoryStream();
                wb.SaveAs(emptyMs);
                return emptyMs.ToArray();
            }

            // Get the Sunday of the week containing the first entry
            var firstDate = entries.Min(e => e.Date);
            var weekSunday = GetSundayOfWeek(firstDate);
            var weekSaturday = weekSunday.AddDays(6);

            // Filter entries to this week only
            var weekEntries = entries.Where(e => e.Date >= weekSunday && e.Date <= weekSaturday)
                                    .ToDictionary(e => e.Date, e => e);

            // Map to correct rows: Sunday=8, Monday=9, ..., Saturday=14
            var dayToRow = new Dictionary<DayOfWeek, int>
            {
                { DayOfWeek.Sunday, 8 },
                { DayOfWeek.Monday, 9 },
                { DayOfWeek.Tuesday, 10 },
                { DayOfWeek.Wednesday, 11 },
                { DayOfWeek.Thursday, 12 },
                { DayOfWeek.Friday, 13 },
                { DayOfWeek.Saturday, 14 }
            };

            // Fill each day of the week
            for (int i = 0; i < 7; i++)
            {
                var date = weekSunday.AddDays(i);
                var dayOfWeek = date.DayOfWeek;
                var row = dayToRow[dayOfWeek];

                // Fill date
                ws.Cell(row, 2).Value = date.ToDateTime(TimeOnly.MinValue);
                ws.Cell(row, 2).Style.DateFormat.Format = "MMMM d, yyyy";

                // Fill entry data if exists
                if (weekEntries.TryGetValue(date, out var entry))
                {
                    // Times
                    ws.Cell(row, 3).Value = entry.StartTime.ToTimeSpan().TotalDays;
                    ws.Cell(row, 4).Value = entry.EndTime.ToTimeSpan().TotalDays;
                    ws.Cell(row, 3).Style.NumberFormat.Format = "h:mm";
                    ws.Cell(row, 4).Style.NumberFormat.Format = "h:mm";

                    // Odometer
                    if (entry.OdometerStart.HasValue)
                        ws.Cell(row, 7).Value = entry.OdometerStart.Value;
                    if (entry.OdometerEnd.HasValue)
                        ws.Cell(row, 8).Value = entry.OdometerEnd.Value;
                }
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private DateOnly GetSundayOfWeek(DateOnly date)
        {
            var daysSinceSunday = (int)date.DayOfWeek;
            return date.AddDays(-daysSinceSunday);
        }
    }
}