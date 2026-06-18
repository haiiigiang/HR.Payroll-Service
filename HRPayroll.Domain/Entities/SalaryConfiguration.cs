using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRPayroll.Domain.Entities;

public class SalaryConfiguration
{
    [Key]
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    
    [ForeignKey(nameof(EmployeeId))]
    public EmployeeReference? Employee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MealAllowance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TransportAllowance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InsuranceDeduction { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherDeductions { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
