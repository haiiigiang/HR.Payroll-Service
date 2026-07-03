using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Features.Payrolls;

public class GetPayrollsQuery : IRequest<List<PayrollDto>>
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class GetPayrollsQueryHandler : IRequestHandler<GetPayrollsQuery, List<PayrollDto>>
{
    private readonly IHRPayrollDbContext _context;

    public GetPayrollsQueryHandler(IHRPayrollDbContext context)
    {
        _context = context;
    }

    public async Task<List<PayrollDto>> Handle(GetPayrollsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PayrollRecords.AsNoTracking().AsQueryable();

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
        }
        
        if (request.Month.HasValue)
        {
            query = query.Where(x => x.Month == request.Month.Value);
        }

        if (request.Year.HasValue)
        {
            query = query.Where(x => x.Year == request.Year.Value);
        }

        var records = await query.ToListAsync(cancellationToken);

        // Map tên/mã NV từ read-model bằng MỘT query — frontend không phải gọi N1 để tự join.
        var employeeIds = records.Select(r => r.EmployeeId).Distinct().ToList();
        var refs = await _context.Employees.AsNoTracking()
            .Where(x => employeeIds.Contains(x.EmployeeId))
            .ToDictionaryAsync(x => x.EmployeeId, cancellationToken);

        return records.Select(r => new PayrollDto
        {
            EmployeeCode = refs.TryGetValue(r.EmployeeId, out var e) ? e.EmployeeCode : null,
            EmployeeName = refs.TryGetValue(r.EmployeeId, out e) ? e.FullName : null,
            DepartmentName = refs.TryGetValue(r.EmployeeId, out e) ? e.DepartmentName : null,
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            Month = r.Month,
            Year = r.Year,
            BaseSalary = r.BaseSalary,
            TotalAllowances = r.TotalAllowances,
            TotalDeductions = r.TotalDeductions,
            NetSalary = r.NetSalary,
            StandardWorkdays = r.StandardWorkdays,
            ActualWorkdays = r.ActualWorkdays,
            OvertimeHours = r.OvertimeHours,
            PaidLeaveDays = r.PaidLeaveDays,
            UnpaidLeaveDays = r.UnpaidLeaveDays,
            Status = r.Status,
            CalculatedAt = r.CalculatedAt
        }).ToList();
    }
}
