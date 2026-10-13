using System.ComponentModel.DataAnnotations;
using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Application.DTOs.Contracts;

public class CreateContractRequest
{
    [Required, MaxLength(50)] public string ContractNumber { get; init; } = string.Empty;
    [Required, MaxLength(200)] public string Title { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; init; }
    [Required] public int? CompanyId { get; init; }
    [Required] public int? OwnerId { get; init; }
    [Required] public decimal? ContractValue { get; init; }
    [Required] public DateOnly? StartDate { get; init; }
    [Required] public DateOnly? EndDate { get; init; }
    [Required] public ContractType? ContractType { get; init; }
}