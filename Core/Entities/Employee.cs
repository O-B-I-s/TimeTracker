namespace Core.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmployeeId { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }

        public ICollection<TimesheetEntry> TimesheetEntries { get; set; }
    }
}
