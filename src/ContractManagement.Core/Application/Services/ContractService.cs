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
        var companyId = request.CompanyId!.Value;
        var ownerId = request.OwnerId!.Value;

        var exists = await _dbContext.Contracts.AnyAsync(
            c => c.ContractNumber == contractNumber, cancellationToken);

        if (exists)
            throw new DomainException("A contract with the same contract number already exists.");

        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken)
            ?? throw new NotFoundException("Company", companyId);

        var owner = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken)
            ?? throw new NotFoundException("User", ownerId);

        var contract = new Contract(
            contractNumber, title, request.Description, request.ContractValue!.Value,
            request.StartDate!.Value, request.EndDate!.Value, request.ContractType!.Value, company, owner);

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
    public async Task AddPartyAsync(
        int contractId,
        AddContractPartyRequest request,
        CancellationToken cancellationToken = default)
    {
        var contract = await LoadContractAsync(contractId, cancellationToken);
        EnsureOwner(contract);

        var party = await _dbContext.Parties
            .FirstOrDefaultAsync(p => p.Id == request.PartyId, cancellationToken)
            ?? throw new NotFoundException("Party", request.PartyId!.Value);

        contract.AddParty(party, request.PartyRole!.Value);

        _auditService.Log(
            AuditEntityType.Contract,
            contract.Id,
            AuditAction.PartyAdded,
            _currentUser.UserId,
            newValue: new { PartyId = party.Id, request.PartyRole });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddDocumentAsync(
        int contractId,
        AddContractDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var contract = await LoadContractAsync(contractId, cancellationToken);
        EnsureOwner(contract);

        var fileName = request.FileName.Trim();

        contract.AddDocument(fileName, request.DocumentType!.Value, request.FilePath, contract.OwnerId);

        _auditService.Log(
            AuditEntityType.Contract,
            contract.Id,
            AuditAction.DocumentAdded,
            _currentUser.UserId,
            newValue: new { FileName = fileName, request.DocumentType });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Contract> LoadContractAsync(int contractId, CancellationToken cancellationToken)
    {
        var contract = await _dbContext.Contracts
            .Include(c => c.Parties)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        return contract ?? throw new NotFoundException("Contract", contractId);
    }

    private void EnsureOwner(Contract contract)
    {
        if (contract.OwnerId != _currentUser.UserId)
            throw new ForbiddenException("Only the contract owner can change its parties and documents.");
    }
}