namespace HRCore.Domain.Events;

// Namespace TRÙNG với N1 để MassTransit định tuyến đúng.
public class EmployeeResignedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public DateTime? ResignedDate { get; set; }
    public string? ResignedReason { get; set; }
    public int WorkingStatus { get; set; }
    public DateTime UpdatedAt { get; set; }
}
