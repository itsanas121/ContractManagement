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
    private readonly FakeCurrentUser _currentUser = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Audit_history_of_unknown_contract_is_not_found()
    {
        var (_, ownerId) = await SeedContractAsync("1010000001", "sara@test.com");
        _currentUser.UserId = ownerId;

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => Service().GetAuditHistoryAsync(999));
        Assert.Contains("Contract", ex.Message);
    }

    [Fact]
    public async Task Audit_history_of_another_companys_contract_is_forbidden()
    {
        var (contractId, _) = await SeedContractAsync("1010000001", "sara@test.com");
        var (_, otherUserId) = await SeedContractAsync("1010000002", "omar@test.com", "CNT-002");
        _currentUser.UserId = otherUserId;

        await Assert.ThrowsAsync<ForbiddenException>(() => Service().GetAuditHistoryAsync(contractId));
    }

    [Fact]
    public async Task Audit_history_returns_only_this_contracts_entries_in_order_with_user_name()
    {
        var (contractId, ownerId) = await SeedContractAsync("1010000001", "sara@test.com");
        _currentUser.UserId = ownerId;

        using (var context = _database.CreateContext())
        {
            context.AuditLogs.AddRange(
                new AuditLog(AuditEntityType.Contract, contractId, AuditAction.Created, null, null, ownerId),
                new AuditLog(AuditEntityType.Contract, contractId, AuditAction.PartyAdded, null, null, ownerId),
                new AuditLog(AuditEntityType.Contract, contractId + 100, AuditAction.Created, null, null, ownerId), // other contract
                new AuditLog(AuditEntityType.Contract, contractId, AuditAction.Submitted, null, null, 999));        // deleted user
            await context.SaveChangesAsync();
        }

        var result = await Service().GetAuditHistoryAsync(contractId);

        Assert.Equal(3, result.Count);                       // other contract's entry not included
        Assert.Equal("Created", result[0].Action);
        Assert.Equal("Sara Ali", result[0].PerformedBy);
        Assert.Null(result[2].PerformedBy);                  // unknown user → null, entry still shown
    }

    private ContractService Service() =>
        new(_database.CreateContext(), _audit, _currentUser);

    // Creates a company + owner + one contract, returns (contractId, ownerId)
    private async Task<(int ContractId, int OwnerId)> SeedContractAsync(
        string registrationNumber, string email, string contractNumber = "CNT-001")
    {
        using var context = _database.CreateContext();
        var company = new Company("Company " + registrationNumber, registrationNumber, null, null, null);
        var owner = new User("Sara", "Ali", email, "E-" + email, "hash", UserRole.ContractManager, company);
        var contract = new Contract(contractNumber, "Service Agreement", null, 1000m,
            new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31),
            ContractType.Service, company, owner);

        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        return (contract.Id, owner.Id);
    }
}