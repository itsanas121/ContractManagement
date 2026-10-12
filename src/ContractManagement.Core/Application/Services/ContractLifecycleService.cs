using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Core.Application.Services;

public class ContractLifecycleService : IContractLifecycleService
{
    private readonly IAppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUser _currentUser;

    public ContractLifecycleService(IAppDbContext db, IAuditService audit, ICurrentUser currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task SubmitAsync(int contractId, CancellationToken cancellationToken = default)
    {
        var contract = await LoadContractAsync(contractId, cancellationToken);

        if (contract.OwnerId != _currentUser.UserId)
            throw new ForbiddenException("Only the contract owner can submit it for review.");

        var oldStatus = contract.Status;
        contract.SubmitForReview();

        _audit.Log(AuditEntityType.Contract, contract.Id, AuditAction.Submitted, _currentUser.UserId,
            oldValue: new { Status = oldStatus },
            newValue: new { Status = contract.Status });

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Contract> LoadContractAsync(int contractId, CancellationToken cancellationToken)
    {
        var contract = await _db.Contracts
            .Include(c => c.Company)
            .Include(c => c.Parties)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        return contract ?? throw new NotFoundException("Contract", contractId);
    }
}