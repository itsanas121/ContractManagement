using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Domain.Entities;

public class AuditLog
{
    public int Id { get; private set; }
    public AuditEntityType EntityType { get; private set; }
    public int EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public int? PerformedById { get; private set; }
    public DateTime PerformedAt { get; private set; }

    private AuditLog() { } // Required by EF Core

    public AuditLog(AuditEntityType entityType, int entityId, AuditAction action,
        string? oldValue, string? newValue, int? performedById)
    {
        if (entityId <= 0)
            throw new DomainException("An audit entry needs the ID of a saved entity.");

        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        PerformedById = performedById;
        PerformedAt = DateTime.UtcNow;
    }
}