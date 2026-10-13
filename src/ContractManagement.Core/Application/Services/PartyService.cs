using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Application.DTOs.Parties;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Core.Application.Services;

public class PartyService : IPartyService
{
    private const string IndividualType = "Individual";
    private const string OrganizationType = "Organization";

    private readonly IAppDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public PartyService(
        IAppDbContext dbContext,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<PartyResponse> RegisterIndividualAsync(
        CreateIndividualPartyRequest request,
        CancellationToken cancellationToken = default)
    {
        var party = new IndividualParty(
            request.Name.Trim(),
            request.NationalId.Trim(),
            request.Email.Trim(),
            request.Phone.Trim());

        await EnsureNotDuplicateAsync(party, cancellationToken);
        await SaveWithAuditAsync(party, cancellationToken);

        return ToResponse(party);
    }

    public async Task<PartyResponse> RegisterOrganizationAsync(
        CreateOrganizationPartyRequest request,
        CancellationToken cancellationToken = default)
    {
        var party = new OrganizationParty(
            request.OrganizationName.Trim(),
            request.RegistrationNumber.Trim(),
            request.ContactPerson.Trim(),
            request.Email.Trim(),
            request.Phone.Trim());

        await EnsureNotDuplicateAsync(party, cancellationToken);
        await SaveWithAuditAsync(party, cancellationToken);

        return ToResponse(party);
    }

    public async Task<IReadOnlyList<PartyResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Parties
            .AsNoTracking()
            .OrderBy(p => p.Name).ThenBy(p => p.Id)
            .Select(p => new PartyResponse(
                p.Id,
                p is IndividualParty ? IndividualType : OrganizationType,
                p.Name,
                p.Email,
                p.Phone,
                (p as IndividualParty)!.NationalId,
                (p as OrganizationParty)!.RegistrationNumber,
                (p as OrganizationParty)!.ContactPerson))
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureNotDuplicateAsync(Party party, CancellationToken cancellationToken)
    {
        switch (party)
        {
            case IndividualParty individual:
            {
                var nationalId = individual.NationalId;
                var exists = await _dbContext.Parties.OfType<IndividualParty>()
                    .AsNoTracking()
                    .AnyAsync(p => p.NationalId == nationalId, cancellationToken);

                if (exists)
                    throw new DomainException("An individual party with the same National ID already exists.");
                break;
            }
            case OrganizationParty organization:
            {
                var registrationNumber = organization.RegistrationNumber;
                var exists = await _dbContext.Parties.OfType<OrganizationParty>()
                    .AsNoTracking()
                    .AnyAsync(p => p.RegistrationNumber == registrationNumber, cancellationToken);

                if (exists)
                    throw new DomainException("An organization party with the same registration number already exists.");
                break;
            }
        }
    }

    // The only place that depends on the audit pattern (same as CompanyService):
    // save to get the generated Id, log the audit entry, save again, then commit.
    private async Task SaveWithAuditAsync(Party party, CancellationToken cancellationToken)
    {
        await using var transaction =
            await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            _dbContext.Parties.Add(party);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Race condition: the same value was inserted after our check.
            await EnsureNotDuplicateAsync(party, cancellationToken);
            throw;
        }

        _auditService.Log(
            AuditEntityType.Party,
            party.Id,
            AuditAction.Created,
            _currentUser.UserId,
            newValue: new
            {
                PartyType = party is IndividualParty ? IndividualType : OrganizationType,
                party.Name
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static PartyResponse ToResponse(Party party) => party switch
    {
        IndividualParty i => new PartyResponse(
            i.Id, IndividualType, i.Name, i.Email, i.Phone, i.NationalId, null, null),
        OrganizationParty o => new PartyResponse(
            o.Id, OrganizationType, o.Name, o.Email, o.Phone, null, o.RegistrationNumber, o.ContactPerson),
        _ => throw new InvalidOperationException($"Unknown party type {party.GetType().Name}.")
    };
}