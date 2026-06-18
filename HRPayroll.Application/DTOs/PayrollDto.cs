using HRPayroll.Domain.Enums;

namespace HRPayroll.Application.DTOs;

public class PayrollDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    // Số liệu chấm công đã dùng để tính (audit)
    public decimal StandardWorkdays { get; set; }
    public decimal ActualWorkdays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }

    public PayrollStatus Status { get; set; }
    public DateTime CalculatedAt { get; set; }
}
