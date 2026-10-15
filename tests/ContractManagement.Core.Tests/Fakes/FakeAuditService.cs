using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Tests.Fakes;

public sealed record AuditCall(
    AuditEntityType EntityType, int EntityId, AuditAction Action,
    int? PerformedById, object? OldValue, object? NewValue);

public sealed class FakeAuditService : IAuditService
{
    private readonly List<AuditCall> _calls = new();

    public IReadOnlyList<AuditCall> Calls => _calls;
    public bool ThrowOnLog { get; set; }

    public void Log(AuditEntityType entityType, int entityId, AuditAction action,
        int? performedById, object? oldValue = null, object? newValue = null)
    {
        if (ThrowOnLog)
            throw new InvalidOperationException("Audit failure (test).");

        // same rule as the real AuditLog constructor
        if (entityId <= 0)
            throw new DomainException("An audit entry needs the ID of a saved entity.");

        _calls.Add(new AuditCall(entityType, entityId, action, performedById, oldValue, newValue));
    }
}