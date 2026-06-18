using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Features.SalaryConfigs;

public class GetSalaryConfigQuery : IRequest<SalaryConfigDto?>
{
    public Guid EmployeeId { get; set; }
}

public class GetSalaryConfigQueryHandler : IRequestHandler<GetSalaryConfigQuery, SalaryConfigDto?>
{
    private readonly IHRPayrollDbContext _context;

    public GetSalaryConfigQueryHandler(IHRPayrollDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryConfigDto?> Handle(GetSalaryConfigQuery request, CancellationToken cancellationToken)
    {
        var config = await _context.SalaryConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId, cancellationToken);

        if (config == null) return null;

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
