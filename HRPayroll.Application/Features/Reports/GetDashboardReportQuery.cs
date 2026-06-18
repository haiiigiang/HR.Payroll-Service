using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Features.Reports;

public class GetDashboardReportQuery : IRequest<DashboardReportDto>
{
    public int Month { get; set; }
    public int Year { get; set; }
}

public class GetDashboardReportQueryHandler : IRequestHandler<GetDashboardReportQuery, DashboardReportDto>
{
    private readonly IHRPayrollDbContext _context;

    public GetDashboardReportQueryHandler(IHRPayrollDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardReportDto> Handle(GetDashboardReportQuery request, CancellationToken cancellationToken)
    {
        // Include Employee để lấy phòng ban (dữ liệu đồng bộ từ HR Core)
        var records = await _context.PayrollRecords
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => x.Month == request.Month && x.Year == request.Year)
            .ToListAsync(cancellationToken);

        var byDepartment = records
            .GroupBy(x => new { x.Employee?.DepartmentId, x.Employee?.DepartmentName })
            .Select(g => new DepartmentSalaryDto
            {
                DepartmentId = g.Key.DepartmentId,
                DepartmentName = g.Key.DepartmentName ?? "Chưa phân phòng",
                EmployeeCount = g.Count(),
                TotalSalaryFund = g.Sum(x => x.NetSalary),
                TotalAllowances = g.Sum(x => x.TotalAllowances),
                TotalDeductions = g.Sum(x => x.TotalDeductions)
            })
            .OrderByDescending(d => d.TotalSalaryFund)
            .ToList();

        return new DashboardReportDto
        {
            Month = request.Month,
            Year = request.Year,
            TotalEmployeesProcessed = records.Count,
            TotalSalaryFund = records.Sum(x => x.NetSalary),
            TotalAllowances = records.Sum(x => x.TotalAllowances),
            TotalDeductions = records.Sum(x => x.TotalDeductions),
            ByDepartment = byDepartment
        };
    }
}
