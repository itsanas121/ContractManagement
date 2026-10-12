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

    // A contract that is already UnderReview (has a party and a document)
    public static Contract NewUnderReviewContract()
    {
        var contract = NewDraftContract();
        contract.AddParty(NewParty(), PartyRole.Customer);
        contract.AddDocument("agreement.pdf", DocumentType.MainContract, null, 1);
        contract.SubmitForReview();
        return contract;
    }

    // A reviewer in the same company as the contract, with Id 2 (not the owner)
    public static User NewReviewer(Contract contract)
    {
        var reviewer = new User("Omar", "Saleh", "omar@test.com", "E003", "hash",
            UserRole.Reviewer, contract.Company);
        SetId(reviewer, 2);
        return reviewer;
    }
    // An Approved contract (submitted, then approved by a reviewer)
    public static Contract NewApprovedContract()
    {
        var contract = NewUnderReviewContract();
        contract.Approve(NewReviewer(contract));
        return contract;
    }

    // An Active contract (activated on its start date: 1 Nov 2026)
    public static Contract NewActiveContract()
    {
        var contract = NewApprovedContract();
        contract.Activate(new DateOnly(2026, 11, 1));
        return contract;
    }

    // Test-only: set a private Id, because there is no database in unit tests
    private static void SetId(object entity, int id)
    {
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);
    }
}