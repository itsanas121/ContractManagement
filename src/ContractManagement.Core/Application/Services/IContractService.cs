using ContractManagement.Core.Application.DTOs.Contracts;

namespace ContractManagement.Core.Application.Services;

public interface IContractService
{
    Task<ContractResponse> CreateAsync(
        CreateContractRequest request, CancellationToken cancellationToken = default);
}