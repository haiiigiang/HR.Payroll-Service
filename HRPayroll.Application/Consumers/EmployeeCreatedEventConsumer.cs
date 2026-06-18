using HRCore.Domain.Events;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPayroll.Application.Consumers;

public class EmployeeCreatedEventConsumer : IConsumer<EmployeeCreatedEvent>
{
    private readonly IHRPayrollDbContext _context;
    private readonly ILogger<EmployeeCreatedEventConsumer> _logger;

    public EmployeeCreatedEventConsumer(IHRPayrollDbContext context, ILogger<EmployeeCreatedEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        _logger.LogInformation("N3 received EmployeeCreatedEvent for EmployeeId: {EmployeeId}", context.Message.EmployeeId);

        var existing = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == context.Message.EmployeeId);

        if (existing == null)
        {
            var newRef = new EmployeeReference
            {
                EmployeeId = context.Message.EmployeeId,
                EmployeeCode = context.Message.EmployeeCode,
                FullName = context.Message.FullName,
                DepartmentId = context.Message.DepartmentId,
                IsActive = true
            };

            _context.Employees.Add(newRef);
            _logger.LogInformation("Successfully synced new EmployeeReference to PayrollDB.");
        }
        else
        {
            _logger.LogInformation("EmployeeReference already exists in PayrollDB.");
        }

        // Tự tạo cấu hình lương mặc định (BaseSalary=0) nếu chưa có → đảm bảo "chốt công là ra payroll record".
        // HR chỉ cần vào sửa lại con số sau.
        var hasConfig = await _context.SalaryConfigurations
            .AnyAsync(c => c.EmployeeId == context.Message.EmployeeId);
        if (!hasConfig)
        {
            _context.SalaryConfigurations.Add(new SalaryConfiguration
            {
                Id = Guid.NewGuid(),
                EmployeeId = context.Message.EmployeeId,
                BaseSalary = 0,
                MealAllowance = 0,
                TransportAllowance = 0,
                InsuranceDeduction = 0,
                OtherDeductions = 0,
                CreatedAt = DateTime.UtcNow
            });
            _logger.LogInformation("Created default SalaryConfiguration (BaseSalary=0) for new employee.");
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
