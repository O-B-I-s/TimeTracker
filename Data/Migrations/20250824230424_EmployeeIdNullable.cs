using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimesheetEntries_Employees_EmployeeId",
                table: "TimesheetEntries");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "TimesheetEntries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_Employees_EmployeeId",
                table: "TimesheetEntries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimesheetEntries_Employees_EmployeeId",
                table: "TimesheetEntries");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "TimesheetEntries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TimesheetEntries_Employees_EmployeeId",
                table: "TimesheetEntries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
