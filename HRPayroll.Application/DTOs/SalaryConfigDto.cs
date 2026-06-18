namespace HRPayroll.Application.DTOs;

public class SalaryConfigDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal MealAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal InsuranceDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
}
