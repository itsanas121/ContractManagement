using ContractManagement.Core.Application.DTOs.Contracts;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using ContractManagement.Core.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Core.Tests;

public class ContractServiceTests : IDisposable
{
    private const int CurrentUserId = 7;

    private readonly TestDatabase _database = new();
    private readonly FakeAuditService _audit = new();
    private readonly FakeCurrentUser _currentUser = new() { UserId = CurrentUserId };

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Create_with_unknown_company_is_rejected()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Create(Request(companyId: 999, ownerId: 1)));

        Assert.Empty(_audit.Calls);
    }

    [Fact]
    public async Task Create_with_unknown_owner_is_rejected()
    {
        var (companyId, _) = SeedCompanyAndOwner();

        await Assert.ThrowsAsync<NotFoundException>(() => Create(Request(companyId, ownerId: 999)));

        Assert.Empty(_audit.Calls);
    }



    private async Task<ContractResponse> Create(CreateContractRequest request)
    {
        using var context = _database.CreateContext();
        return await new ContractService(context, _audit, _currentUser).CreateAsync(request);
    }

    private (int CompanyId, int OwnerId) SeedCompanyAndOwner()
    {
        using var context = _database.CreateContext();
        var company = new Company("Test Company", "1010000001", null, null, null);
        context.Companies.Add(company);
        context.SaveChanges();

        var owner = new User("Sara", "Ali", "sara@test.com", "E001", "hash", UserRole.ContractManager, company);
        context.Users.Add(owner);
        context.SaveChanges();

        return (company.Id, owner.Id);
    }

    [Fact]
    public async Task Create_with_valid_data_saves_a_draft_contract_and_writes_one_audit_entry()
    {
        var (companyId, ownerId) = SeedCompanyAndOwner();

        var response = await Create(Request(companyId, ownerId));

        Assert.True(response.Id > 0);
        Assert.Equal(ContractStatus.Draft, response.Status);

        using var context = _database.CreateContext();
        var saved = await context.Contracts.AsNoTracking().SingleAsync();
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal(ContractStatus.Draft, saved.Status);

        var call = Assert.Single(_audit.Calls);
        Assert.Equal(AuditEntityType.Contract, call.EntityType);
        Assert.Equal(response.Id, call.EntityId);
        Assert.Equal(AuditAction.Created, call.Action);
        Assert.Equal(CurrentUserId, call.PerformedById);
    }

    [Fact]
    public async Task Create_with_duplicate_contract_number_is_rejected_without_a_second_audit_entry()
    {
        var (companyId, ownerId) = SeedCompanyAndOwner();
        await Create(Request(companyId, ownerId));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            Create(Request(companyId, ownerId)));

        Assert.Equal("A contract with the same contract number already exists.", exception.Message);
        Assert.Single(_audit.Calls);

        using var context = _database.CreateContext();
        Assert.Equal(1, await context.Contracts.CountAsync());
    }
    private static CreateContractRequest Request(int companyId, int ownerId) => new()
    {
        ContractNumber = "CNT-2026-001",
        Title = "Software Development Agreement",
        CompanyId = companyId,
        OwnerId = ownerId,
        ContractValue = 500000m,
        StartDate = new DateOnly(2026, 10, 1),
        EndDate = new DateOnly(2027, 9, 30),
        ContractType = ContractType.Service
    };
}