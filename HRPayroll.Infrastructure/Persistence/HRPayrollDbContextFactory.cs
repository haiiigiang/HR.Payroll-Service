using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HRPayroll.Infrastructure.Persistence;

// Chỉ dùng cho EF Core tooling (dotnet ef migrations/database) tại design-time.
// Cho phép tạo migration mà không cần nạp startup project là Web API.
public class HRPayrollDbContextFactory : IDesignTimeDbContextFactory<HRPayrollDbContext>
{
    public HRPayrollDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HRPayrollDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=PayrollDB;Username=postgres;Password=postgres");
        return new HRPayrollDbContext(optionsBuilder.Options);
    }
}
