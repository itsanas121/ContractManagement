using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Application.Common;

public interface IAuditService
{
    // Adds an audit entry to the current DbContext. It is NOT saved here:
    // the caller's SaveChangesAsync saves it together with the business change.
    void Log(AuditEntityType entityType, int entityId, AuditAction action,
        int? performedById, object? oldValue = null, object? newValue = null);
}