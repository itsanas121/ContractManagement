using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Tests;

public static class TestData
{
    public static Contract NewDraftContract()
    {
        var company = new Company("Test Company", "1010000001", null, null, null);
        var owner = new User("Sara", "Ali", "sara@test.com", "E001", "hash", UserRole.ContractManager, company);

        return new Contract("CNT-001", "Service Agreement", null, 50000m,
            new DateOnly(2026, 11, 1), new DateOnly(2027, 10, 31),
            ContractType.Service, company, owner);
    }

    public static Party NewParty() =>
        new IndividualParty("Ahmed Ali", "1012345678", "ahmed@test.com", "0500000000");
}