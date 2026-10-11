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
}