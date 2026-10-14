using ContractManagement.Core.Application.DTOs.Contracts;

namespace ContractManagement.Core.Application.Services;

public interface IContractService
{
    Task<ContractResponse> CreateAsync(
        CreateContractRequest request, CancellationToken cancellationToken = default);
    Task AddPartyAsync(
         int contractId, AddContractPartyRequest request, CancellationToken cancellationToken = default);

    Task AddDocumentAsync(
        int contractId, AddContractDocumentRequest request, CancellationToken cancellationToken = default);

    Task<ContractSummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContractAuditEntryResponse>> GetAuditHistoryAsync(
        int contractId, CancellationToken cancellationToken = default);
        
    Task<ContractDetailsResponse> GetByIdAsync(
        int contractId, CancellationToken cancellationToken = default);
}