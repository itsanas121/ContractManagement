namespace ContractManagement.Api.Domain.Entities;

public class AuditLog
{
    public int Id { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public int EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public int? PerformedById { get; private set; }
    public DateTime PerformedAt { get; private set; }

    private AuditLog() { }

    public AuditLog(string entityType, int entityId, string action,
        string? oldValue, string? newValue, int? performedById)
    {
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        PerformedById = performedById;
        PerformedAt = DateTime.UtcNow;
    }
}