namespace ContractManagement.Core.Application.Services;

public interface IContractLifecycleService
{
    Task SubmitAsync(int contractId, CancellationToken cancellationToken = default);
    Task ApproveAsync(int contractId, CancellationToken cancellationToken = default);
    Task RejectAsync(int contractId, string reason, CancellationToken cancellationToken = default);
}