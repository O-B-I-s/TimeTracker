namespace Core.Entities
{
    public class TimesheetEntry
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }


        public int? OdometerStart { get; set; }
        public int? OdometerEnd { get; set; }
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; } // Navigation property

        public double HoursWorked => Math.Max(0,
        EndTime.ToTimeSpan().TotalHours - StartTime.ToTimeSpan().TotalHours);


        public double? Kilometres =>
        OdometerStart.HasValue && OdometerEnd.HasValue
        ? OdometerEnd.Value - OdometerStart.Value
        : null;
    }
}
