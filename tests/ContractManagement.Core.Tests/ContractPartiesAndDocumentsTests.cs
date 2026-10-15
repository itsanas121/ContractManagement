using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Tests;

public class ContractPartiesAndDocumentsTests
{
    [Fact]
    public void AddParty_to_draft_contract_adds_the_party_with_its_role()
    {
        var contract = TestData.NewDraftContract();
        var party = TestData.NewParty();

        contract.AddParty(party, PartyRole.Customer);

        var added = Assert.Single(contract.Parties);
        Assert.Equal(party.Id, added.PartyId);
        Assert.Equal(PartyRole.Customer, added.PartyRole);
    }


    [Fact]
    public void AddParty_allows_different_parties_on_the_same_contract()
    {
        var contract = TestData.NewDraftContract();
        var individual = TestData.NewParty();
        var organization = new OrganizationParty(
            "ABC Company", "7001234567", "Sara Ali", "info@abc.com", "0112345678");
        SetPartyId(individual, 1);
        SetPartyId(organization, 2);

        contract.AddParty(individual, PartyRole.Customer);
        contract.AddParty(organization, PartyRole.Supplier);

        Assert.Equal(2, contract.Parties.Count);
    }

    [Fact]
    public void AddParty_same_party_twice_is_rejected()
    {
        var contract = TestData.NewDraftContract();
        var party = TestData.NewParty();
        contract.AddParty(party, PartyRole.Customer);

        var exception = Assert.Throws<DomainException>(() => contract.AddParty(party, PartyRole.Supplier));

        Assert.Equal("Ahmed Ali is already a party to this contract.", exception.Message);
        Assert.Single(contract.Parties);
    }

    [Fact]
    public void AddParty_to_non_draft_contract_is_rejected()
    {
        var contract = TestData.NewUnderReviewContract();

        Assert.Throws<InvalidContractStatusException>(() =>
            contract.AddParty(new IndividualParty("Omar Saleh", "1099999999", "omar@test.com", "0500000001"),
                PartyRole.Consultant));

        Assert.Single(contract.Parties);
    }

    [Fact]
    public void AddDocument_to_draft_contract_adds_the_document()
    {
        var contract = TestData.NewDraftContract();

        contract.AddDocument("Main Contract.pdf", DocumentType.MainContract, "/files/main.pdf", 1);

        var document = Assert.Single(contract.Documents);
        Assert.Equal("Main Contract.pdf", document.FileName);
        Assert.Equal(DocumentType.MainContract, document.DocumentType);
        Assert.Equal("/files/main.pdf", document.FilePath);
        Assert.Equal(1, document.UploadedById);
    }

    [Fact]
    public void AddDocument_allows_multiple_documents_on_the_same_contract()
    {
        var contract = TestData.NewDraftContract();

        contract.AddDocument("Main Contract.pdf", DocumentType.MainContract, null, 1);
        contract.AddDocument("Technical Proposal.pdf", DocumentType.MainContract, null, 1);

        Assert.Equal(2, contract.Documents.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddDocument_with_blank_file_name_is_rejected(string fileName)
    {
        var contract = TestData.NewDraftContract();

        var exception = Assert.Throws<DomainException>(() =>
            contract.AddDocument(fileName, DocumentType.MainContract, null, 1));

        Assert.Equal("File name is required.", exception.Message);
        Assert.Empty(contract.Documents);
    }

    [Fact]
    public void AddDocument_to_non_draft_contract_is_rejected()
    {
        var contract = TestData.NewUnderReviewContract();

        Assert.Throws<InvalidContractStatusException>(() =>
            contract.AddDocument("Late.pdf", DocumentType.MainContract, null, 1));

        Assert.Single(contract.Documents);
    }

    private static void SetPartyId(Party party, int id) =>
    typeof(Party).GetProperty(nameof(Party.Id))!.SetValue(party, id);
}