namespace ContractManagement.Core.Application.Services;

public interface IContractLifecycleService
{
    Task SubmitAsync(int contractId, CancellationToken cancellationToken = default);
}