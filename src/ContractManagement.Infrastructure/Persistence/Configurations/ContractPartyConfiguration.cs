using ContractManagement.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractManagement.Infrastructure.Persistence.Configurations;

public sealed class ContractPartyConfiguration : IEntityTypeConfiguration<ContractParty>
{
    public void Configure(EntityTypeBuilder<ContractParty> b)
    {
        b.ToTable("ContractParties");
        b.HasKey(x => new { x.ContractId, x.PartyId });   // the same party can't be added twice to a contract
        b.Property(x => x.PartyRole).HasConversion<string>().HasMaxLength(30).IsRequired();

        // the link to Contract is configured on the Contract side
        b.HasOne(x => x.Party).WithMany().HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
    }
}