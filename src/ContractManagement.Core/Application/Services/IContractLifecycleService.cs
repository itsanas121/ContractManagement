namespace ContractManagement.Core.Application.Services;

public interface IContractLifecycleService
{
    Task SubmitAsync(int contractId, CancellationToken cancellationToken = default);
    Task ApproveAsync(int contractId, CancellationToken cancellationToken = default);
    Task RejectAsync(int contractId, string reason, CancellationToken cancellationToken = default);
    Task ActivateAsync(int contractId, CancellationToken cancellationToken = default);
    Task TerminateAsync(int contractId, string reason, CancellationToken cancellationToken = default);
}