using HRCore.Domain.Events;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPayroll.Application.Consumers;

// Cập nhật EmployeeReference khi N1 sửa thông tin nhân viên (tên, phòng ban...).
public class EmployeeUpdatedEventConsumer : IConsumer<EmployeeUpdatedEvent>
{
    private readonly IHRPayrollDbContext _context;
    private readonly ILogger<EmployeeUpdatedEventConsumer> _logger;

    public EmployeeUpdatedEventConsumer(IHRPayrollDbContext context, ILogger<EmployeeUpdatedEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmployeeUpdatedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("N3 received EmployeeUpdatedEvent for EmployeeId: {EmployeeId}", msg.EmployeeId);

        var existing = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == msg.EmployeeId);
        if (existing == null)
        {
            // Chưa có thì tạo mới (idempotent — phòng khi bỏ lỡ event created)
            existing = new EmployeeReference { EmployeeId = msg.EmployeeId };
            _context.Employees.Add(existing);
        }

        existing.EmployeeCode = msg.EmployeeCode;
        existing.FullName = msg.FullName;
        existing.DepartmentId = msg.DepartmentId;
        existing.IsActive = true;

        await _context.SaveChangesAsync(CancellationToken.None);
        _logger.LogInformation("Updated EmployeeReference in PayrollDB.");
    }
}
