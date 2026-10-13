using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Tests;

public class ContractReviewTests
{
    [Fact]
    public void Approve_ByReviewer_MovesToApproved()
    {
        // Arrange
        var contract = TestData.NewUnderReviewContract();
        var reviewer = TestData.NewReviewer(contract);

        // Act
        contract.Approve(reviewer);

        // Assert
        Assert.Equal(ContractStatus.Approved, contract.Status);
        Assert.Equal(2, contract.ApprovedById);
    }

    [Fact]
    public void Reject_WithReason_MovesToRejected()
    {
        var contract = TestData.NewUnderReviewContract();
        var reviewer = TestData.NewReviewer(contract);

        contract.Reject(reviewer, "Missing signature page");

        Assert.Equal(ContractStatus.Rejected, contract.Status);
        Assert.Equal("Missing signature page", contract.RejectionReason);
    }

    [Fact]
    public void Reject_WithoutReason_Throws_AndStaysUnderReview()
    {
        var contract = TestData.NewUnderReviewContract();
        var reviewer = TestData.NewReviewer(contract);

        Assert.Throws<DomainException>(() => contract.Reject(reviewer, "   "));
        Assert.Equal(ContractStatus.UnderReview, contract.Status);
    }

    [Fact]
    public void Approve_ByNonReviewer_Throws()
    {
        var contract = TestData.NewUnderReviewContract();
        var manager = new User("Ali", "Hassan", "ali@test.com", "E004", "hash",
            UserRole.ContractManager, contract.Company);

        Assert.Throws<DomainException>(() => contract.Approve(manager));
    }

    [Fact]
    public void Approve_DraftContract_ThrowsInvalidStatus()
    {
        var contract = TestData.NewDraftContract();
        var reviewer = TestData.NewReviewer(contract);

        Assert.Throws<InvalidContractStatusException>(() => contract.Approve(reviewer));
    }
    
    [Fact]
    public void Approve_ByInactiveReviewer_Throws()
    {
        var contract = TestData.NewUnderReviewContract();
        var reviewer = TestData.NewReviewer(contract);
        reviewer.Deactivate();

        var ex = Assert.Throws<DomainException>(() => contract.Approve(reviewer));
        Assert.Contains("Inactive", ex.Message);
    }

    [Fact]
    public void Approve_ByReviewerFromAnotherCompany_Throws()
    {
        var contract = TestData.NewUnderReviewContract();
        var otherCompany = new Company("Other Company", "1010000002", null, null, null);
        TestData.SetId(otherCompany, 2);
        var reviewer = new User("Khalid", "Omar", "khalid@test.com", "E005", "hash",
            UserRole.Reviewer, otherCompany);
        TestData.SetId(reviewer, 3);
        TestData.SetProperty(reviewer, "CompanyId", 2);

        var ex = Assert.Throws<DomainException>(() => contract.Approve(reviewer));
        Assert.Contains("company", ex.Message);
    }

    [Fact]
    public void Approve_ByContractOwner_Throws()
    {
        var contract = TestData.NewUnderReviewContract();
        var ownerAsReviewer = new User("Sara", "Ali", "sara2@test.com", "E006", "hash",
            UserRole.Reviewer, contract.Company);
        TestData.SetId(ownerAsReviewer, contract.OwnerId);

        var ex = Assert.Throws<DomainException>(() => contract.Approve(ownerAsReviewer));
        Assert.Contains("owner", ex.Message);
    }
}