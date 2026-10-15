using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Tests;

public class ContractSubmitTests
{
    [Fact]
    public void New_contract_starts_as_draft()
    {
        var contract = TestData.NewDraftContract();

        Assert.Equal(ContractStatus.Draft, contract.Status);
    }

    [Fact]
    public void Submit_without_party_and_document_returns_both_errors()
    {
        var contract = TestData.NewDraftContract();

        var exception = Assert.Throws<DomainValidationException>(() => contract.SubmitForReview());

        Assert.Equal(2, exception.Errors.Count);
        Assert.Contains("Contract must contain at least one party.", exception.Errors);
        Assert.Contains("Contract document is required.", exception.Errors);
        Assert.Equal(ContractStatus.Draft, contract.Status);
    }

    [Fact]
    public void Submit_with_party_and_document_moves_to_under_review()
    {
        var contract = TestData.NewDraftContract();
        contract.AddParty(TestData.NewParty(), PartyRole.Customer);
        contract.AddDocument("Main Contract.pdf", DocumentType.MainContract, null, 1);

        contract.SubmitForReview();

        Assert.Equal(ContractStatus.UnderReview, contract.Status);
    }

    [Fact]
    public void Submit_twice_is_rejected()
    {
        var contract = TestData.NewDraftContract();
        contract.AddParty(TestData.NewParty(), PartyRole.Customer);
        contract.AddDocument("Main Contract.pdf", DocumentType.MainContract, null, 1);
        contract.SubmitForReview();

        Assert.Throws<InvalidContractStatusException>(() => contract.SubmitForReview());
    }
}