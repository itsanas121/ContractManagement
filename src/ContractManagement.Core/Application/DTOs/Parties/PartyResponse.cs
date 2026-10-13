namespace ContractManagement.Core.Application.DTOs.Parties;

public sealed record PartyResponse(
    int Id,
    string PartyType,
    string Name,
    string Email,
    string Phone,
    string? NationalId,
    string? RegistrationNumber,
    string? ContactPerson);