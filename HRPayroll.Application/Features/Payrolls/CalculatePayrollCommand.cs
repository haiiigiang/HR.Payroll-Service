using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Features.Payrolls;

public class CalculatePayrollCommand : IRequest<List<PayrollDto>>
{
    public int Month { get; set; }
    public int Year { get; set; }
    public Guid? EmployeeId { get; set; } // null = tính cho tất cả

    // Số liệu chấm công (từ Attendance N2). Khi tính tay (không có chấm công) thì để 0 → coi như đủ công.
    public decimal StandardWorkdays { get; set; }
    public decimal ActualWorkdays { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
}

public class CalculatePayrollCommandHandler : IRequestHandler<CalculatePayrollCommand, List<PayrollDto>>
{
    private const decimal OvertimeFactor = 1.5m;        // hệ số tăng ca ngày thường
    private const decimal DefaultStandardWorkdays = 22m; // công chuẩn mặc định khi không có chấm công
    private const decimal HoursPerDay = 8m;

    private readonly IHRPayrollDbContext _context;

    public CalculatePayrollCommandHandler(IHRPayrollDbContext context)
    {
        _context = context;
    }

    public async Task<List<PayrollDto>> Handle(CalculatePayrollCommand request, CancellationToken cancellationToken)
    {
        var configsQuery = _context.SalaryConfigurations.AsQueryable();
        if (request.EmployeeId.HasValue)
            configsQuery = configsQuery.Where(x => x.EmployeeId == request.EmployeeId.Value);

        var configs = await configsQuery.ToListAsync(cancellationToken);
        var results = new List<PayrollDto>();

        // Có số liệu chấm công thật khi N2 gửi kèm StandardWorkdays > 0.
        bool hasAttendance = request.StandardWorkdays > 0;
        decimal standardWorkdays = hasAttendance ? request.StandardWorkdays : DefaultStandardWorkdays;
        decimal actualWorkdays = hasAttendance ? request.ActualWorkdays : standardWorkdays; // tính tay → coi đủ công
        decimal paidLeaveDays = hasAttendance ? request.PaidLeaveDays : 0m;
        decimal unpaidLeaveDays = hasAttendance ? request.UnpaidLeaveDays : 0m;
        decimal overtimeHours = hasAttendance ? request.OvertimeHours : 0m;

        foreach (var config in configs)
        {
            decimal totalAllowances = config.MealAllowance + config.TransportAllowance;
            decimal totalDeductions = config.InsuranceDeduction + config.OtherDeductions;

            // Lương theo công thực tế: đơn giá ngày × (ngày công + ngày phép CÓ lương).
            decimal dailyRate = standardWorkdays > 0 ? config.BaseSalary / standardWorkdays : 0m;
            decimal hourlyRate = dailyRate / HoursPerDay;
            decimal workSalary = dailyRate * (actualWorkdays + paidLeaveDays);
            decimal overtimePay = hourlyRate * overtimeHours * OvertimeFactor;

            // Sàn ở 0: thực lãnh không bao giờ âm.
            decimal netSalary = Math.Max(0, workSalary + overtimePay + totalAllowances - totalDeductions);

            var record = await _context.PayrollRecords
                .FirstOrDefaultAsync(x => x.EmployeeId == config.EmployeeId && x.Month == request.Month && x.Year == request.Year, cancellationToken);

            if (record == null)
            {
                record = new PayrollRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = config.EmployeeId,
                    Month = request.Month,
                    Year = request.Year,
                    Status = PayrollStatus.Pending,
                };
                _context.PayrollRecords.Add(record);
            }

            record.BaseSalary = config.BaseSalary;
            record.TotalAllowances = totalAllowances;
            record.TotalDeductions = totalDeductions;
            record.NetSalary = netSalary;
            record.StandardWorkdays = standardWorkdays;
            record.ActualWorkdays = actualWorkdays;
            record.OvertimeHours = overtimeHours;
            record.PaidLeaveDays = paidLeaveDays;
            record.UnpaidLeaveDays = unpaidLeaveDays;
            record.CalculatedAt = DateTime.UtcNow;

            results.Add(MapToDto(record));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return results;
    }

    private static PayrollDto MapToDto(PayrollRecord r) => new()
    {
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
    };
}
