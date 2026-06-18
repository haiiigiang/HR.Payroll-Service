using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Interfaces;

public interface IHRPayrollDbContext
{
    DbSet<EmployeeReference> Employees { get; }
    DbSet<SalaryConfiguration> SalaryConfigurations { get; }
    DbSet<PayrollRecord> PayrollRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
