using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence;

public class HRPayrollDbContext : DbContext, IHRPayrollDbContext
{
    public HRPayrollDbContext(DbContextOptions<HRPayrollDbContext> options) : base(options)
    {
    }

    public DbSet<EmployeeReference> Employees { get; set; } = null!;
    public DbSet<SalaryConfiguration> SalaryConfigurations { get; set; } = null!;
    public DbSet<PayrollRecord> PayrollRecords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EmployeeReference
        modelBuilder.Entity<EmployeeReference>()
            .HasKey(e => e.EmployeeId);

        modelBuilder.Entity<EmployeeReference>()
            .HasOne(e => e.SalaryConfiguration)
            .WithOne(s => s.Employee)
            .HasForeignKey<SalaryConfiguration>(s => s.EmployeeId);

        modelBuilder.Entity<EmployeeReference>()
            .HasMany(e => e.PayrollRecords)
            .WithOne(p => p.Employee)
            .HasForeignKey(p => p.EmployeeId);

        // SalaryConfiguration
        modelBuilder.Entity<SalaryConfiguration>()
            .Property(s => s.BaseSalary)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<SalaryConfiguration>()
            .Property(s => s.MealAllowance)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<SalaryConfiguration>()
            .Property(s => s.TransportAllowance)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<SalaryConfiguration>()
            .Property(s => s.InsuranceDeduction)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<SalaryConfiguration>()
            .Property(s => s.OtherDeductions)
            .HasColumnType("numeric(18,2)");

        // PayrollRecord
        modelBuilder.Entity<PayrollRecord>()
            .Property(p => p.BaseSalary)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<PayrollRecord>()
            .Property(p => p.TotalAllowances)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<PayrollRecord>()
            .Property(p => p.TotalDeductions)
            .HasColumnType("numeric(18,2)");
        modelBuilder.Entity<PayrollRecord>()
            .Property(p => p.NetSalary)
            .HasColumnType("numeric(18,2)");
    }
}
