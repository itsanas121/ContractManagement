using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Tests.Fakes;

namespace ContractManagement.Core.Tests;

public class ContractSummaryTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeAuditService _audit = new();
    private readonly FakeCurrentUser _currentUser = new() { UserId = 7 };

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Summary_with_no_contracts_returns_zeros()
    {
        using var context = _database.CreateContext();
        var result = await new ContractService(context, _audit, _currentUser)
            .GetSummaryAsync(CancellationToken.None);

        Assert.Equal(0, result.TotalContracts);
        Assert.Equal(0m, result.TotalActiveContractValue);
    }

    [Fact]
    public async Task Summary_counts_by_status_and_sums_only_active_value()
    {
        using (var context = _database.CreateContext())
        {
            var company = new Company("Test Company", "1010000001", null, null, null);
            var owner = new User("Sara", "Ali", "sara@test.com", "E001", "hash",
                UserRole.ContractManager, company);

            var active1 = NewContract("CNT-001", 1000m, company, owner);
            var active2 = NewContract("CNT-002", 2000m, company, owner);
            var draft = NewContract("CNT-003", 5000m, company, owner);

            TestData.SetProperty(active1, "Status", ContractStatus.Active);
            TestData.SetProperty(active2, "Status", ContractStatus.Active);

            context.Contracts.AddRange(active1, active2, draft);
            await context.SaveChangesAsync();
        }

        using var readContext = _database.CreateContext();
        var result = await new ContractService(readContext, _audit, _currentUser)
            .GetSummaryAsync(CancellationToken.None);

        Assert.Equal(3, result.TotalContracts);
        Assert.Equal(2, result.Active);
        Assert.Equal(1, result.Draft);
        Assert.Equal(0, result.Approved);
        Assert.Equal(3000m, result.TotalActiveContractValue); // Draft's 5000 not counted
    }

    private static Contract NewContract(string number, decimal value, Company company, User owner) =>
        new(number, "Service Agreement", null, value,
            new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31),
            ContractType.Service, company, owner);
}