using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Application.DTOs.Contracts;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Core.Application.Services;

public class ContractService : IContractService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public ContractService(
        IAppDbContext dbContext,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<ContractResponse> CreateAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        var contractNumber = request.ContractNumber.Trim();
        var title = request.Title.Trim();

        var exists = await _dbContext.Contracts.AnyAsync(
            c => c.ContractNumber == contractNumber, cancellationToken);

        if (exists)
            throw new DomainException("A contract with the same contract number already exists.");

        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Company", request.CompanyId);

        var owner = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.OwnerId, cancellationToken)
            ?? throw new NotFoundException("User", request.OwnerId);

        var contract = new Contract(
            contractNumber, title, request.Description, request.ContractValue,
            request.StartDate, request.EndDate, request.ContractType!.Value, company, owner);

        // The Id is generated on save, and the audit entry needs it: save, log, save again.
        await using var transaction =
            await _dbContext.BeginTransactionAsync(cancellationToken);

        _dbContext.Contracts.Add(contract);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _auditService.Log(
            AuditEntityType.Contract,
            contract.Id,
            AuditAction.Created,
            _currentUser.UserId,
            newValue: new { contract.ContractNumber, contract.Title, contract.Status });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ContractResponse(
            contract.Id, contract.ContractNumber, contract.Title, contract.Description,
            contract.ContractValue, contract.StartDate, contract.EndDate,
            contract.ContractType, contract.Status, contract.CompanyId, contract.OwnerId);
    }
}