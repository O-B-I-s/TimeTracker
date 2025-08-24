using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();

        public DbSet<Employee> Employees => Set<Employee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Convert DateOnly -> DateTime for SQL Server
            modelBuilder.Entity<TimesheetEntry>()
                .Property(e => e.Date)
                .HasConversion(
                    d => d.ToDateTime(TimeOnly.MinValue),   // store as DateTime
                    d => DateOnly.FromDateTime(d));         // read back as DateOnly

            // Convert TimeOnly -> TimeSpan for SQL Server
            modelBuilder.Entity<TimesheetEntry>()
                .Property(e => e.StartTime)
                .HasConversion(
                    t => t.ToTimeSpan(),
                    t => TimeOnly.FromTimeSpan(t));

            modelBuilder.Entity<TimesheetEntry>()
                .Property(e => e.EndTime)
                .HasConversion(
                    t => t.ToTimeSpan(),
                    t => TimeOnly.FromTimeSpan(t));


            modelBuilder.Entity<TimesheetEntry>()
                .HasIndex(e => e.Date);
            modelBuilder.Entity<TimesheetEntry>()
                .HasOne(e => e.Employee)
                .WithMany(e => e.TimesheetEntries)
                .HasForeignKey(e => e.EmployeeId);

            modelBuilder.Entity<TimesheetEntry>()
                .HasIndex(e => e.Date);
        }
    }


}
