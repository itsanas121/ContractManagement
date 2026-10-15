using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Tests;

public class ContractActivationTests
{
    // The test contract runs from 1 Nov 2026 to 31 Oct 2027

    [Fact]
    public void Activate_OnStartDate_MovesToActive()
    {
        var contract = TestData.NewApprovedContract();

        contract.Activate(new DateOnly(2026, 11, 1));

        Assert.Equal(ContractStatus.Active, contract.Status);
    }

    [Fact]
    public void Activate_BeforeStartDate_Throws_AndStaysApproved()
    {
        var contract = TestData.NewApprovedContract();

        Assert.Throws<ContractActivationException>(() => contract.Activate(new DateOnly(2026, 10, 15)));
        Assert.Equal(ContractStatus.Approved, contract.Status);
    }

    [Fact]
    public void Activate_AfterEndDate_Throws()
    {
        var contract = TestData.NewApprovedContract();

        Assert.Throws<ContractActivationException>(() => contract.Activate(new DateOnly(2027, 11, 1)));
    }

    [Fact]
    public void Activate_DraftContract_ThrowsInvalidStatus()
    {
        var contract = TestData.NewDraftContract();

        Assert.Throws<InvalidContractStatusException>(() => contract.Activate(new DateOnly(2026, 11, 1)));
    }

    [Fact]
    public void Terminate_WithReason_MovesToTerminated()
    {
        var contract = TestData.NewActiveContract();

        contract.Terminate(4, "Client ended the agreement");

        Assert.Equal(ContractStatus.Terminated, contract.Status);
        Assert.Equal(4, contract.TerminatedById);
        Assert.Equal("Client ended the agreement", contract.TerminationReason);
    }

    [Fact]
    public void Terminate_WithoutReason_Throws_AndStaysActive()
    {
        var contract = TestData.NewActiveContract();

        Assert.Throws<DomainException>(() => contract.Terminate(4, ""));
        Assert.Equal(ContractStatus.Active, contract.Status);
    }
    [Fact]
    public void Activate_Twice_ThrowsInvalidStatus()
    {
        var contract = TestData.NewActiveContract();

        Assert.Throws<InvalidContractStatusException>(() => contract.Activate(new DateOnly(2026, 11, 2)));
    }

    [Fact]
    public void Terminate_NotActiveContract_ThrowsInvalidStatus()
    {
        var contract = TestData.NewApprovedContract();

        Assert.Throws<InvalidContractStatusException>(() => contract.Terminate(4, "Client ended the agreement"));
    }

    [Fact]
    public void Terminate_WithWhitespaceReason_Throws_AndStaysActive()
    {
        var contract = TestData.NewActiveContract();

        Assert.Throws<DomainException>(() => contract.Terminate(4, "   "));
        Assert.Equal(ContractStatus.Active, contract.Status);
    }
}