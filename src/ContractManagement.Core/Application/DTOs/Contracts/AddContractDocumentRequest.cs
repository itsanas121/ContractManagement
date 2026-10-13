using System.ComponentModel.DataAnnotations;
using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Application.DTOs.Contracts;

public class AddContractDocumentRequest
{
    [Required, MaxLength(260)] public string FileName { get; init; } = string.Empty;
    [Required] public DocumentType? DocumentType { get; init; }
    [MaxLength(500)] public string? FilePath { get; init; }
}