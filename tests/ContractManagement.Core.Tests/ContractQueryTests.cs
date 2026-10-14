using ContractManagement.Core.Application.DTOs.Contracts;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using ContractManagement.Core.Tests.Fakes;

namespace ContractManagement.Core.Tests;

public class ContractQueryTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeAuditService _audit = new();
    private readonly FakeCurrentUser _currentUser = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task GetById_existing_contract_returns_details_for_same_company_user()
    {
        var seed = Seed();
        _currentUser.UserId = seed.ReaderId;

        var response = await GetById(seed.ContractId);

        Assert.Equal(seed.ContractId, response.Id);
        Assert.Equal("CNT-QUERY-001", response.ContractNumber);
        Assert.Equal("Query test contract", response.Title);
        Assert.Equal("Details test", response.Description);
        Assert.Equal(ContractStatus.Draft, response.Status);
        Assert.Equal(ContractType.Service, response.ContractType);
        Assert.Equal(1000m, response.ContractValue);
        Assert.Equal(new DateOnly(2026, 10, 1), response.StartDate);
        Assert.Equal(new DateOnly(2027, 10, 1), response.EndDate);

        Assert.Equal(seed.CompanyId, response.CompanyId);
        Assert.Equal("Company One", response.CompanyName);
        Assert.Equal(seed.OwnerId, response.OwnerId);
        Assert.Equal("Sara Ali", response.OwnerName);

        var party = Assert.Single(response.Parties);
        Assert.Equal(seed.PartyId, party.PartyId);
        Assert.Equal("Ahmed Ali", party.Name);
        Assert.Equal(PartyRole.Customer, party.PartyRole);
        Assert.NotEqual(default(DateTime), party.AddedDate);

        var document = Assert.Single(response.Documents);
        Assert.True(document.Id > 0);
        Assert.Equal("Main Contract.pdf", document.FileName);
        Assert.Equal(DocumentType.MainContract, document.DocumentType);
        Assert.Null(document.FilePath);
        Assert.Equal(seed.OwnerId, document.UploadedById);
        Assert.NotEqual(default(DateTime), document.UploadedDate);

        Assert.Empty(_audit.Calls);
    }

    [Fact]
    public async Task GetById_unknown_contract_throws_not_found()
    {
        var seed = Seed();
        _currentUser.UserId = seed.ReaderId;

        await Assert.ThrowsAsync<NotFoundException>(() =>
            GetById(int.MaxValue));

        Assert.Empty(_audit.Calls);
    }

    [Fact]
    public async Task GetById_user_from_another_company_throws_forbidden()
    {
        var seed = Seed();
        _currentUser.UserId = seed.OtherCompanyUserId;

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            GetById(seed.ContractId));

        Assert.Empty(_audit.Calls);
    }

    private async Task<ContractDetailsResponse> GetById(int contractId)
    {
        using var context = _database.CreateContext();

        var service = new ContractService(
            context, _audit, _currentUser);

        return await service.GetByIdAsync(contractId);
    }

    private SeedResult Seed()
    {
        using var context = _database.CreateContext();

        var company = new Company(
            "Company One", "QUERY-COMPANY-001", null, null, null);

        var otherCompany = new Company(
            "Company Two", "QUERY-COMPANY-002", null, null, null);

        context.Companies.AddRange(company, otherCompany);
        context.SaveChanges();

        var owner = new User(
            "Sara", "Ali",
            "owner-query@test.com", "QUERY-OWNER", "hash",
            UserRole.ContractManager, company);

        var reader = new User(
            "Omar", "Saleh",
            "reader-query@test.com", "QUERY-READER", "hash",
            UserRole.Reviewer, company);

        var otherCompanyUser = new User(
            "Fahad", "Ali",
            "other-query@test.com", "QUERY-OTHER", "hash",
            UserRole.Reviewer, otherCompany);

        context.Users.AddRange(owner, reader, otherCompanyUser);

        var party = new IndividualParty(
            "Ahmed Ali", "1099999999",
            "party-query@test.com", "0500000001");

        context.Parties.Add(party);
        context.SaveChanges();

        var contract = new Contract(
            "CNT-QUERY-001",
            "Query test contract",
            "Details test",
            1000m,
            new DateOnly(2026, 10, 1),
            new DateOnly(2027, 10, 1),
            ContractType.Service,
            company,
            owner);

        contract.AddParty(party, PartyRole.Customer);
        contract.AddDocument(
            "Main Contract.pdf",
            DocumentType.MainContract,
            null,
            owner.Id);

        context.Contracts.Add(contract);
        context.SaveChanges();

        return new SeedResult(
            contract.Id,
            company.Id,
            owner.Id,
            reader.Id,
            otherCompanyUser.Id,
            party.Id);
    }

    private sealed record SeedResult(
        int ContractId,
        int CompanyId,
        int OwnerId,
        int ReaderId,
        int OtherCompanyUserId,
        int PartyId);
}