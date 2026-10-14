namespace ContractManagement.Core.Application.DTOs.Contracts;

public sealed record ContractSummaryResponse(
    int TotalContracts,
    int Draft,
    int UnderReview,
    int Approved,
    int Rejected,
    int Active,
    int Expired,
    int Terminated,
    decimal TotalActiveContractValue);