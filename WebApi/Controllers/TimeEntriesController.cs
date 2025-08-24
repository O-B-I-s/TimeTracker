using Core.Entities;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeEntriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ExcelExporter _excelService;

        public TimeEntriesController(AppDbContext context, ExcelExporter excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        // GET: api/timeentries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimesheetEntry>>> GetEntries()
        {
            return await _context.TimesheetEntries
                                 .OrderBy(e => e.Date)
                                 .ToListAsync();
        }

        // GET: api/timeentries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TimesheetEntry>> GetEntry(int id)
        {
            var entry = await _context.TimesheetEntries.FindAsync(id);
            if (entry == null) return NotFound();
            return entry;
        }

        // GET: api/timeentries/week/2025-08-18
        [HttpGet("week/{weekStart}")]
        public async Task<ActionResult<IEnumerable<TimesheetEntry>>> GetEntriesByWeek(DateOnly weekStart)
        {
            var weekEnd = weekStart.AddDays(7);
            var entries = await _context.TimesheetEntries
                                        .Where(e => e.Date >= weekStart && e.Date < weekEnd)
                                        .OrderBy(e => e.Date)
                                        .ToListAsync();
            return entries;
        }

        // POST: api/timeentries
        [HttpPost]
        public async Task<ActionResult<TimesheetEntry>> AddEntry(TimesheetEntry entry)
        {
            // Ensure one entry per date (upsert)
            var existing = await _context.TimesheetEntries
                                         .FirstOrDefaultAsync(e => e.Date == entry.Date);
            if (existing != null)
            {
                existing.StartTime = entry.StartTime;
                existing.EndTime = entry.EndTime;
                existing.OdometerStart = entry.OdometerStart;
                existing.OdometerEnd = entry.OdometerEnd;
                await _context.SaveChangesAsync();
                return Ok(existing);
            }

            _context.TimesheetEntries.Add(entry);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
        }

        // PUT: api/timeentries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(int id, TimesheetEntry entry)
        {
            if (id != entry.Id) return BadRequest();

            _context.Entry(entry).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/timeentries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            var entry = await _context.TimesheetEntries.FindAsync(id);
            if (entry == null) return NotFound();

            _context.TimesheetEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: GET api/timeentries/export/current-week?name=Johnson%20Obioma&employeeId=81235493&location=CALGARY&department=MERCH
        [HttpGet("export/current-week")]
        public async Task<IActionResult> ExportCurrentWeek([FromQuery] string name, [FromQuery] string employeeId, [FromQuery] string location, [FromQuery] string department)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var sunday = GetSundayOfWeek(today);
            var saturday = sunday.AddDays(6);

            var entries = await _context.TimesheetEntries
                                        .Where(e => e.Date >= sunday && e.Date <= saturday)
                                        .OrderBy(e => e.Date)
                                        .ToListAsync();

            if (!entries.Any())
            {
                return BadRequest($"No entries found for week {sunday:yyyy-MM-dd}");
            }

            var bytes = _excelService.Generate(entries, name, employeeId, location, department);
            return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"TimeEntries_Week_{sunday:yyyyMMdd}.xlsx");
        }
        private DateOnly GetSundayOfWeek(DateOnly date)
        {
            var daysSinceSunday = (int)date.DayOfWeek;
            return date.AddDays(-daysSinceSunday);
        }
    }
}