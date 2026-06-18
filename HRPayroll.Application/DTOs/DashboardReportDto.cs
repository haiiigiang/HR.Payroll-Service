namespace HRPayroll.Application.DTOs;

public class DashboardReportDto
{
    public int TotalEmployeesProcessed { get; set; }
    public decimal TotalSalaryFund { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    // Báo cáo tổng hợp theo từng phòng ban (cho biểu đồ Dashboard HR)
    public List<DepartmentSalaryDto> ByDepartment { get; set; } = new();
}

public class DepartmentSalaryDto
{
    public Guid? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "Chưa phân phòng";
    public int EmployeeCount { get; set; }
    public decimal TotalSalaryFund { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
}
