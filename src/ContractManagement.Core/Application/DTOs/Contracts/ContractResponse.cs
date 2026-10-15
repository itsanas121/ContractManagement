using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Application.DTOs.Contracts;

public sealed record ContractResponse(
    int Id,
    string ContractNumber,
    string Title,
    string? Description,
    decimal ContractValue,
    DateOnly StartDate,
    DateOnly EndDate,
    ContractType ContractType,
    ContractStatus Status,
    int CompanyId,
    int OwnerId);