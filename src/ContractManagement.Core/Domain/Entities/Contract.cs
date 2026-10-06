using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Domain.Entities;

public class Contract
{
    private readonly List<ContractParty> _parties = new();
    private readonly List<ContractDocument> _documents = new();
    
    public int Id { get; private set; }
    public string ContractNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal ContractValue { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public ContractType ContractType { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public int CompanyId { get; private set; }
    public Company Company { get; private set; } = null!;
    public int OwnerId { get; private set; }
    public User Owner { get; private set; } = null!;

    public int? ApprovedById { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public int? RejectedById { get; private set; }
    public DateTime? RejectedDate { get; private set; }
    public string? RejectionReason { get; private set; }
    public int? TerminatedById { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public string? TerminationReason { get; private set; }

    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<ContractParty> Parties => _parties;
    public IReadOnlyCollection<ContractDocument> Documents => _documents;
    
    private Contract() { }

    public Contract(string contractNumber, string title, string? description, decimal contractValue,
        DateOnly startDate, DateOnly endDate, ContractType contractType, Company company, User owner)
    {
        if (string.IsNullOrWhiteSpace(contractNumber))
            throw new DomainException("Contract number is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Contract title is required.");
        if (contractValue <= 0)
            throw new DomainException("Contract value must be greater than zero.");
        if (endDate <= startDate)
            throw new DomainException("End date must be after start date.");
        if (!company.IsActive)
            throw new DomainException("Contracts cannot be created for an inactive company.");
        if (!owner.IsActive)
            throw new DomainException("The contract owner must be an active user.");
        if (owner.CompanyId != company.Id)
            throw new DomainException("The contract owner must belong to the same company.");

        ContractNumber = contractNumber;
        Title = title;
        Description = description;
        ContractValue = contractValue;
        StartDate = startDate;
        EndDate = endDate;
        ContractType = contractType;
        Company = company;
        CompanyId = company.Id;
        Owner = owner;
        OwnerId = owner.Id;
        Status = ContractStatus.Draft;
        CreatedDate = DateTime.UtcNow;
    }

    public void AddParty(Party party, PartyRole partyRole)
    {
        EnsureStatus(ContractStatus.Draft, "add a party to");
        if (_parties.Any(cp => cp.PartyId == party.Id))
            throw new DomainException($"{party.DisplayName} is already a party to this contract.");

        _parties.Add(new ContractParty(party, partyRole));
    }

    public void AddDocument(string fileName, DocumentType documentType, string? filePath, int uploadedById)
    {
        EnsureStatus(ContractStatus.Draft, "add a document to");
        _documents.Add(new ContractDocument(fileName, documentType, filePath, uploadedById));
    }

    public void SubmitForReview()
    {
        EnsureStatus(ContractStatus.Draft, "submit");

        var errors = new List<string>();
        if (_parties.Count == 0)
            errors.Add("Contract must contain at least one party.");
        if (_documents.Count == 0)
            errors.Add("Contract document is required.");
        if (!Company.IsActive)
            errors.Add("Company must be active.");

        if (errors.Count > 0)
            throw new DomainValidationException(errors);

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

    public void Reject(int rejectedById, string reason)
    {
        EnsureStatus(ContractStatus.UnderReview, "reject");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required.");

        Status = ContractStatus.Rejected;
        RejectedById = rejectedById;
        RejectedDate = DateTime.UtcNow;
        RejectionReason = reason;
    }

    public void Activate(DateOnly today)
    {
        EnsureStatus(ContractStatus.Approved, "activate");
        if (StartDate > today)
            throw new ContractActivationException($"Contract cannot be activated before its start date ({StartDate:yyyy-MM-dd}).");
        if (EndDate < today)
            throw new ContractActivationException("Contract cannot be activated because its end date has already passed.");
        if (!Company.IsActive)
            throw new ContractActivationException("Contract cannot be activated because the company is inactive.");
        if (_parties.Count == 0)
            throw new ContractActivationException("Contract cannot be activated without at least one party.");

        Status = ContractStatus.Active;
    }

    public void Terminate(int terminatedById, string reason)
    {
        EnsureStatus(ContractStatus.Active, "terminate");
        if (string.IsNullOrWhiteSpace(reason))
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