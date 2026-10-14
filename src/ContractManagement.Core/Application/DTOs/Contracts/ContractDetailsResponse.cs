using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Application.DTOs.Contracts;

public sealed record ContractDetailsResponse(
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
    string CompanyName,
    int OwnerId,
    string OwnerName,
    IReadOnlyList<ContractPartyResponse> Parties,
    IReadOnlyList<ContractDocumentResponse> Documents);

public sealed record ContractPartyResponse(
    int PartyId,
    string Name,
    PartyRole PartyRole,
    DateTime AddedDate);

public sealed record ContractDocumentResponse(
    int Id,
    string FileName,
    DocumentType DocumentType,
    string? FilePath,
    int UploadedById,
    DateTime UploadedDate);