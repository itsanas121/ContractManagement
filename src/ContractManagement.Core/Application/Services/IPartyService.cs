using ContractManagement.Core.Application.DTOs.Parties;

namespace ContractManagement.Core.Application.Services;

public interface IPartyService
{
    Task<PartyResponse> RegisterIndividualAsync(
        CreateIndividualPartyRequest request, CancellationToken cancellationToken = default);

    Task<PartyResponse> RegisterOrganizationAsync(
        CreateOrganizationPartyRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartyResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}