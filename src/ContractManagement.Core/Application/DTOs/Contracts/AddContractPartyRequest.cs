using System.ComponentModel.DataAnnotations;
using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Application.DTOs.Contracts;

public class AddContractPartyRequest
{
    [Required] public int? PartyId { get; init; }
    [Required] public PartyRole? PartyRole { get; init; }
}