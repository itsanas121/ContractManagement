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