using System.ComponentModel.DataAnnotations;

namespace ContractManagement.Core.Application.DTOs.Parties;

public class CreateOrganizationPartyRequest
{
    [Required, MaxLength(200)] public string OrganizationName { get; init; } = string.Empty;
    [Required, MaxLength(10)] public string RegistrationNumber { get; init; } = string.Empty;
    [Required, MaxLength(200)] public string ContactPerson { get; init; } = string.Empty;
    [Required, MaxLength(256)] public string Email { get; init; } = string.Empty;
    [Required, MaxLength(30)] public string Phone { get; init; } = string.Empty;
}