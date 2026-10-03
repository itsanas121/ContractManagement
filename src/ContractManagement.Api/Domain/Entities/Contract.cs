using ContractManagement.Api.Domain.Enums;
using ContractManagement.Api.Domain.Exceptions;

namespace ContractManagement.Api.Domain.Entities;

public class Contract
{
    public int Id { get; private set; }
    public string ContractNumber { get; private set; } = string.Empty;
    public string Title { get; private set ;} = string.Empty;
    public string? Description { get; private set; }
    public decimal ContractValue { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public ContractType ContractType { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public int CompanyId { get; private set; }
    public int OwnerId { get; private set; }

    public int? ApprovedById { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public int? RejectedById { get; private set; }
    public DateTime? RejectedDate { get; private set; }
    public string? RejectionReason { get; private set; }
    public int? TerminatedById { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public string? TerminationReason { get; private set; }

    private Contract() { }

    public Contract(string contractNumber, string title, string? description, decimal contractValue,
        DateOnly startDate, DateOnly endDate, ContractType contractType, int companyId, int ownerId)
    {
        if (string.IsNullOrWhiteSpace(contractNumber))
            throw new DomainException("Contract number is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Contract title is required.");
        if (contractValue <= 0)
            throw new DomainException("Contract value must be greater than zero.");
        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");

        ContractNumber = contractNumber;
        Title = title;
        Description = description;
        ContractValue = contractValue;
        StartDate = startDate;
        EndDate = endDate;
        ContractType = contractType;
        CompanyId = companyId;
        OwnerId = ownerId;
        Status = ContractStatus.Draft;
        CreatedDate = DateTime.UtcNow;
    }

    public void SubmitforReview()
    {
        EnsureStatus(ContractStatus.Draft, "submit");
        Status = ContractStatus.UnderReview;
    }
    
    public void Approve(int approvedById)
    {
        EnsureStatus(ContractStatus.UnderReview, "approve");
        if (approvedById == OwnerId)
            throw new DomainException("The contract owner cannot approve their own contract.");
            
        Status = ContractStatus.Approved;
        ApprovedById = approvedById;
        ApprovedDate = DateTime.UtcNow;
    }

    public void Reject(int rejectedbyId, string reason)
    {
        EnsureStatus(ContractStatus.UnderReview, "reject");
        if(string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required.");
        
        Status = ContractStatus.Rejected;
        RejectedById = rejectedbyId;
        RejectedDate = DateTime.UtcNow;
        RejectionReason = reason;
    }

    public void Activate(DateOnly today)
    {
        EnsureStatus(ContractStatus.Approved, "activate");
        if(StartDate > today)
            throw new ContractActivationException($"Contract cannot be activated before its start date ({StartDate}).");

        Status = ContractStatus.Active;
    }
    public void Terminate(int terminatedById, string reason)
    {
        EnsureStatus(ContractStatus.Active, "terminate");
        if(string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Termination reason is required.");

        Status = ContractStatus.Terminated;
        TerminatedById = terminatedById;
        TerminationDate = DateTime.UtcNow;
        TerminationReason = reason;
    }

    public void Expire(DateOnly today)
    {
        EnsureStatus(ContractStatus.Active, "expire");
        if (EndDate >= today)
            throw new DomainException("Contract has not reached its end date yet.");

        Status = ContractStatus.Expired;
    }

    private void EnsureStatus(ContractStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidContractStatusException(ContractNumber, Status, action);
    }
}