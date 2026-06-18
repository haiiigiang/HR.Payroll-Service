namespace HRCore.Domain.Events;

// Namespace TRÙNG với N1 để MassTransit định tuyến đúng.
public class EmployeeUpdatedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ContractTypeId { get; set; }
    public int WorkingStatus { get; set; }
    public DateTime UpdatedAt { get; set; }
}
