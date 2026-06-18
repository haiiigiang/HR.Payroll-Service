using HRCore.Domain.Events;
using HRPayroll.Application.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPayroll.Application.Consumers;

// Đánh dấu nhân viên đã nghỉ việc (IsActive = false) khi N1 publish EmployeeResignedEvent.
public class EmployeeResignedEventConsumer : IConsumer<EmployeeResignedEvent>
{
    private readonly IHRPayrollDbContext _context;
    private readonly ILogger<EmployeeResignedEventConsumer> _logger;

    public EmployeeResignedEventConsumer(IHRPayrollDbContext context, ILogger<EmployeeResignedEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmployeeResignedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("N3 received EmployeeResignedEvent for EmployeeId: {EmployeeId}", msg.EmployeeId);

        var existing = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == msg.EmployeeId);
        if (existing != null)
        {
            existing.IsActive = false;
            await _context.SaveChangesAsync(CancellationToken.None);
            _logger.LogInformation("Marked EmployeeReference as resigned (IsActive=false) in PayrollDB.");
        }
    }
}
