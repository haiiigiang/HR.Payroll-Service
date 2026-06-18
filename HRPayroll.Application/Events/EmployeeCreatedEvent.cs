namespace HRCore.Domain.Events;

// Note: Namespace matches N1's event namespace to ensure MassTransit deserialization works correctly.
public class EmployeeCreatedEvent
{
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ContractTypeId { get; set; }
    public DateTime HireDate { get; set; }
    public int WorkingStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
