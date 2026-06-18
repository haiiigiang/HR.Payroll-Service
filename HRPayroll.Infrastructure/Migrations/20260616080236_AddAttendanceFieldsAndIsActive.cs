using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRPayroll.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceFieldsAndIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualWorkdays",
                table: "PayrollRecords",
                type: "numeric(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeHours",
                table: "PayrollRecords",
                type: "numeric(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidLeaveDays",
                table: "PayrollRecords",
                type: "numeric(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardWorkdays",
                table: "PayrollRecords",
                type: "numeric(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnpaidLeaveDays",
                table: "PayrollRecords",
                type: "numeric(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualWorkdays",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "OvertimeHours",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "PaidLeaveDays",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "StandardWorkdays",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "UnpaidLeaveDays",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employees");
        }
    }
}
