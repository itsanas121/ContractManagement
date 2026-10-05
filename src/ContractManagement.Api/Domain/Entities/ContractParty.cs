using ContractManagement.Api.Domain.Enums;

namespace ContractManagement.Api.Domain.Entities;

public class ContractParty
{
    public int ContractId { get; private set; }
    public int PartyId { get; private set; }
    public Party Party { get; private set; } = null!;
    public PartyRole PartyRole { get; private set; }
    public DateTime AddedDate { get; private set; }

    private ContractParty() { } //Required By EF Core

    internal ContractParty(Party party, PartyRole partyRole)
    {
        Party = party;
        PartyId = party.Id;
        PartyRole = partyRole;
        AddedDate = DateTime.UtcNow;
    }
}