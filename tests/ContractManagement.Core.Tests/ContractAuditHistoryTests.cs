using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using ContractManagement.Core.Tests.Fakes;

namespace ContractManagement.Core.Tests;

public class ContractAuditHistoryTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeAuditService _audit = new();
    private readonly FakeCurrentUser _currentUser = new() { UserId = 7 };

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Audit_history_of_unknown_contract_is_not_found()
    {
        using var context = _database.CreateContext();
        var service = new ContractService(context, _audit, _currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetAuditHistoryAsync(999));
    }

    [Fact]
    public async Task Audit_history_returns_only_this_contracts_entries_in_order_with_user_name()
    {
        int contractId, ownerId;
        using (var context = _database.CreateContext())
        {
            var company = new Company("Test Company", "1010000001", null, null, null);
            var owner = new User("Sara", "Ali", "sara@test.com", "E001", "hash",
                UserRole.ContractManager, company);
            var contract = new Contract("CNT-001", "Service Agreement", null, 1000m,
                new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31),
                ContractType.Service, company, owner);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();
            contractId = contract.Id;
            ownerId = owner.Id;

            context.AuditLogs.AddRange(
                new AuditLog(AuditEntityType.Contract, contractId, AuditAction.Created, null, null, ownerId),
                new AuditLog(AuditEntityType.Contract, contractId, AuditAction.PartyAdded, null, null, ownerId),
                new AuditLog(AuditEntityType.Contract, contractId + 100, AuditAction.Created, null, null, ownerId), // other contract
                new AuditLog(AuditEntityType.Contract, contractId, AuditAction.Submitted, null, null, 999));        // deleted user
            await context.SaveChangesAsync();
        }

        using var readContext = _database.CreateContext();
        var result = await new ContractService(readContext, _audit, _currentUser)
            .GetAuditHistoryAsync(contractId);

        Assert.Equal(3, result.Count);                       // other contract's entry not included
        Assert.Equal("Created", result[0].Action);
        Assert.Equal("Sara Ali", result[0].PerformedBy);
        Assert.Null(result[2].PerformedBy);                  // unknown user → null, entry still shown
    }
}