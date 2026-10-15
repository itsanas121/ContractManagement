using System.Text.Json;
using System.Text.Json.Serialization;
using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Infrastructure.Auditing;

public sealed class AuditService(IAppDbContext db) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public void Log(AuditEntityType entityType, int entityId, AuditAction action,
        int? performedById, object? oldValue = null, object? newValue = null)
    {
        var entry = new AuditLog(entityType, entityId, action,
            Serialize(oldValue), Serialize(newValue), performedById);

        db.AuditLogs.Add(entry);
    }

    private static string? Serialize(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
}