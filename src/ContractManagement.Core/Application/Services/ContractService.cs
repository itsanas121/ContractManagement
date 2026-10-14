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

    public async Task<ContractSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        // One query: SQL does GROUP BY Status + COUNT(*), we only receive one row per status.
        var counts = await _dbContext.Contracts
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // One query: SQL does SUM(ContractValue) for active contracts only.
        var totalActiveValue = await _dbContext.Contracts
            .Where(c => c.Status == ContractStatus.Active)
            .SumAsync(c => c.ContractValue, cancellationToken);

        int CountOf(ContractStatus status) =>
            counts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

        return new ContractSummaryResponse(
            TotalContracts: counts.Sum(x => x.Count),
            Draft: CountOf(ContractStatus.Draft),
            UnderReview: CountOf(ContractStatus.UnderReview),
            Approved: CountOf(ContractStatus.Approved),
            Rejected: CountOf(ContractStatus.Rejected),
            Active: CountOf(ContractStatus.Active),
            Expired: CountOf(ContractStatus.Expired),
            Terminated: CountOf(ContractStatus.Terminated),
            TotalActiveContractValue: totalActiveValue);
    }

    public async Task<IReadOnlyList<ContractAuditEntryResponse>> GetAuditHistoryAsync(
        int contractId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenException("No logged-in user.");

        var userCompanyId = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => (int?)u.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var contractCompanyId = await _dbContext.Contracts
            .Where(c => c.Id == contractId)
            .Select(c => (int?)c.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Contract", contractId);

        if (contractCompanyId != userCompanyId)
            throw new ForbiddenException("You can only view the history of your own company's contracts.");

        // LEFT JOIN to Users: the log stays even if the user is removed (no FK on purpose)
        return await (
            from a in _dbContext.AuditLogs
            where a.EntityType == AuditEntityType.Contract && a.EntityId == contractId
            join u in _dbContext.Users on a.PerformedById equals u.Id into users
            from u in users.DefaultIfEmpty()
            orderby a.PerformedAt, a.Id
            select new ContractAuditEntryResponse(
                a.Action.ToString(),
                a.OldValue,
                a.NewValue,
                a.PerformedAt,
                u == null ? null : u.FirstName + " " + u.LastName)
        ).ToListAsync(cancellationToken);
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

        contract.AddDocument(
            fileName,
            request.DocumentType!.Value,
            request.FilePath,
            contract.OwnerId);

        _auditService.Log(
            AuditEntityType.Contract,
            contract.Id,
            AuditAction.DocumentAdded,
            _currentUser.UserId,
            newValue: new
            {
                FileName = fileName,
                request.DocumentType
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ContractDetailsResponse> GetByIdAsync(
        int contractId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenException("No logged-in user.");

        var userCompanyId = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => (int?)u.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var contract = await _dbContext.Contracts
            .AsNoTracking()
            .Where(c => c.Id == contractId)
            .Select(c => new ContractDetailsResponse(
                c.Id,
                c.ContractNumber,
                c.Title,
                c.Description,
                c.ContractValue,
                c.StartDate,
                c.EndDate,
                c.ContractType,
                c.Status,
                c.CompanyId,
                c.Company.Name,
                c.OwnerId,
                c.Owner.FirstName + " " + c.Owner.LastName,
                c.Parties
                    .OrderBy(p => p.AddedDate)
                    .Select(p => new ContractPartyResponse(
                        p.PartyId,
                        p.Party.Name,
                        p.PartyRole,
                        p.AddedDate))
                    .ToList(),
                c.Documents
                    .OrderBy(d => d.UploadedDate)
                    .Select(d => new ContractDocumentResponse(
                        d.Id,
                        d.FileName,
                        d.DocumentType,
                        d.FilePath,
                        d.UploadedById,
                        d.UploadedDate))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Contract", contractId);

        if (contract.CompanyId != userCompanyId)
            throw new ForbiddenException(
                "You can only view contracts of your own company.");

        return contract;
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