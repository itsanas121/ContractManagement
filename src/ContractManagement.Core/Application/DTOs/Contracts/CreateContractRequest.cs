using System.ComponentModel.DataAnnotations;
using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Application.DTOs.Contracts;

public class CreateContractRequest
{
    [Required, MaxLength(50)] public string ContractNumber { get; init; } = string.Empty;
    [Required, MaxLength(200)] public string Title { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; init; }
    public int CompanyId { get; init; }
    public int OwnerId { get; init; }
    public decimal ContractValue { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    [Required] public ContractType? ContractType { get; init; }
}