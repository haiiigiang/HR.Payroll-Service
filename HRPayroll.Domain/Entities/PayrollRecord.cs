using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRPayroll.Domain.Enums;

namespace HRPayroll.Domain.Entities;

public class PayrollRecord
{
    [Key]
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public EmployeeReference? Employee { get; set; }

    public int Month { get; set; }
    
    public int Year { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAllowances { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeductions { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary { get; set; }

    // === Số liệu chấm công dùng để tính ra bảng lương này (để audit/báo cáo) ===
    [Column(TypeName = "decimal(9,2)")]
    public decimal StandardWorkdays { get; set; }

    [Column(TypeName = "decimal(9,2)")]
    public decimal ActualWorkdays { get; set; }

    [Column(TypeName = "decimal(9,2)")]
    public decimal OvertimeHours { get; set; }

    [Column(TypeName = "decimal(9,2)")]
    public decimal PaidLeaveDays { get; set; }

    [Column(TypeName = "decimal(9,2)")]
    public decimal UnpaidLeaveDays { get; set; }

    public PayrollStatus Status { get; set; }

    public DateTime CalculatedAt { get; set; }
}
