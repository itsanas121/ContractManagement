namespace ContractManagement.Core.Application.DTOs.Contracts;

public sealed record ContractAuditEntryResponse(
    string Action,
    string? OldValue,
    string? NewValue,
    DateTime PerformedAt,
    string? PerformedBy);