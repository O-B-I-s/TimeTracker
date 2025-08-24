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

        public byte[] Generate(IEnumerable<TimesheetEntry> entries, DateOnly weekStart, string employeeName, string employeeId, string location, string department)
        {
            // Load the template
            var templatePath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "templates", "TimesheetTemplate.xlsx");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Template not found at {Path}", templatePath);
                throw new FileNotFoundException("Excel template not found", templatePath);
            }

            using var wb = new XLWorkbook(templatePath);
            var ws = wb.Worksheet(1); // First worksheet

            // Fill employee info
            ws.Cell("B3").Value = employeeName;     // NAME (B3)
            ws.Cell("F3").Value = location;         // LOCATION (F3)
            ws.Cell("B4").Value = employeeId;       // EMPLOYEE ID# (B4)
            ws.Cell("F4").Value = department;       // DEPARTMENT (F4)

            // Map entries by date for quick lookup
            var byDate = entries.ToDictionary(e => e.Date, e => e);

            // Fill time entries - starting at row 8 based on template
            int row = 8; // Sunday row
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);

                // Fill date column (B)
                ws.Cell(row, 2).Value = date.ToDateTime(TimeOnly.MinValue);
                ws.Cell(row, 2).Style.DateFormat.Format = "MMMM d, yyyy";

                if (byDate.TryGetValue(date, out var entry))
                {
                    // Fill time data
                    ws.Cell(row, 3).Value = entry.StartTime.ToTimeSpan().TotalDays;  // START (C)
                    ws.Cell(row, 4).Value = entry.EndTime.ToTimeSpan().TotalDays;    // FINISH (D)
                    ws.Cell(row, 3).Style.NumberFormat.Format = "h:mm";
                    ws.Cell(row, 4).Style.NumberFormat.Format = "h:mm";

                    // Fill odometer data
                    if (entry.OdometerStart.HasValue)
                        ws.Cell(row, 6).Value = entry.OdometerStart.Value;  // ODOMETER START (F)
                    if (entry.OdometerEnd.HasValue)
                        ws.Cell(row, 7).Value = entry.OdometerEnd.Value;    // ODOMETER FINISH (G)
                }

                row++;
            }

            // Save to memory stream
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}