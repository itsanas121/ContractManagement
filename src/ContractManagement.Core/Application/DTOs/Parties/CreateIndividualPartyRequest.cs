using System.ComponentModel.DataAnnotations;

namespace ContractManagement.Core.Application.DTOs.Parties;

public class CreateIndividualPartyRequest
{
    [Required, MaxLength(200)] public string Name { get; init; } = string.Empty;
    [Required, MaxLength(10)] public string NationalId { get; init; } = string.Empty;
    [Required, MaxLength(256)] public string Email { get; init; } = string.Empty;
    [Required, MaxLength(30)] public string Phone { get; init; } = string.Empty;
}