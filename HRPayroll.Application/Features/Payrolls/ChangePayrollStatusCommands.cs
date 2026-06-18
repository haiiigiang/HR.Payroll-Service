using HRPayroll.Application.DTOs;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Features.Payrolls;

// Duyệt lương: Pending -> Approved
public class ApprovePayrollCommand : IRequest<PayrollDto>
{
    public Guid Id { get; set; }
}

// Chi trả lương: Approved -> Paid
public class MarkPayrollPaidCommand : IRequest<PayrollDto>
{
    public Guid Id { get; set; }
}

public class ApprovePayrollCommandHandler : IRequestHandler<ApprovePayrollCommand, PayrollDto>
{
    private readonly IHRPayrollDbContext _context;

    public ApprovePayrollCommandHandler(IHRPayrollDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollDto> Handle(ApprovePayrollCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.PayrollRecords
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy bảng lương {request.Id}.");

        if (record.Status != PayrollStatus.Pending)
            throw new InvalidOperationException($"Chỉ duyệt được bảng lương ở trạng thái Pending (hiện tại: {record.Status}).");

        record.Status = PayrollStatus.Approved;
        await _context.SaveChangesAsync(cancellationToken);

        return PayrollMapper.ToDto(record);
    }
}

public class MarkPayrollPaidCommandHandler : IRequestHandler<MarkPayrollPaidCommand, PayrollDto>
{
    private readonly IHRPayrollDbContext _context;

    public MarkPayrollPaidCommandHandler(IHRPayrollDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollDto> Handle(MarkPayrollPaidCommand request, CancellationToken cancellationToken)
    {
        var record = await _context.PayrollRecords
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy bảng lương {request.Id}.");

        if (record.Status != PayrollStatus.Approved)
            throw new InvalidOperationException($"Chỉ chi trả được bảng lương đã Approved (hiện tại: {record.Status}).");

        record.Status = PayrollStatus.Paid;
        await _context.SaveChangesAsync(cancellationToken);

        return PayrollMapper.ToDto(record);
    }
}

internal static class PayrollMapper
{
    public static PayrollDto ToDto(PayrollRecord r) => new()
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
