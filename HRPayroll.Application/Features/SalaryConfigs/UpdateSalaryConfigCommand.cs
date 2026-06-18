using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Features.SalaryConfigs;

public class UpdateSalaryConfigCommand : IRequest<SalaryConfigDto>
{
    public Guid EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal MealAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal InsuranceDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
}

public class UpdateSalaryConfigCommandHandler : IRequestHandler<UpdateSalaryConfigCommand, SalaryConfigDto>
{
    private readonly IHRPayrollDbContext _context;

    public UpdateSalaryConfigCommandHandler(IHRPayrollDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryConfigDto> Handle(UpdateSalaryConfigCommand request, CancellationToken cancellationToken)
    {
        // Validate nghiệp vụ: không âm và khấu trừ không được vượt tổng thu nhập (tránh thực lãnh âm).
        if (request.BaseSalary < 0 || request.MealAllowance < 0 || request.TransportAllowance < 0
            || request.InsuranceDeduction < 0 || request.OtherDeductions < 0)
            throw new InvalidOperationException("Lương, phụ cấp và khấu trừ không được là số âm.");

        var gross = request.BaseSalary + request.MealAllowance + request.TransportAllowance;
        var deductions = request.InsuranceDeduction + request.OtherDeductions;
        if (deductions > gross)
            throw new InvalidOperationException(
                $"Tổng khấu trừ ({deductions:#,##0}đ) không được vượt quá tổng thu nhập ({gross:#,##0}đ).");

        var config = await _context.SalaryConfigurations
            .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId, cancellationToken);

        if (config == null)
        {
            config = new SalaryConfiguration
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                CreatedAt = DateTime.UtcNow
            };
            _context.SalaryConfigurations.Add(config);
        }

        config.BaseSalary = request.BaseSalary;
        config.MealAllowance = request.MealAllowance;
        config.TransportAllowance = request.TransportAllowance;
        config.InsuranceDeduction = request.InsuranceDeduction;
        config.OtherDeductions = request.OtherDeductions;
        config.UpdatedAt = DateTime.UtcNow;

        // Note: For real world we might want to check if EmployeeReference exists.
        // If not, we might need to sync or throw an error depending on architecture.
        // For N3 we'll assume the EmployeeReference is synced via message broker.

        await _context.SaveChangesAsync(cancellationToken);

        return new SalaryConfigDto
        {
            Id = config.Id,
            EmployeeId = config.EmployeeId,
            BaseSalary = config.BaseSalary,
            MealAllowance = config.MealAllowance,
            TransportAllowance = config.TransportAllowance,
            InsuranceDeduction = config.InsuranceDeduction,
            OtherDeductions = config.OtherDeductions
        };
    }
}
