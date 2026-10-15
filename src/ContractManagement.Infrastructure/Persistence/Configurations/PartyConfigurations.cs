using ContractManagement.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractManagement.Infrastructure.Persistence.Configurations;

// Party hierarchy uses TPH: one "Parties" table for both kinds of party
public sealed class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> b)
    {
        b.ToTable("Parties");
        b.UseTphMappingStrategy();
        b.HasKey(x => x.Id);

        // a text column that says which kind each row is
        b.HasDiscriminator<string>("PartyType")
            .HasValue<IndividualParty>("Individual")
            .HasValue<OrganizationParty>("Organization");
        b.Property<string>("PartyType").HasMaxLength(20);

        b.Property(x => x.Name).IsRequired().HasMaxLength(200);   // shared by both kinds
        b.Property(x => x.Email).IsRequired().HasMaxLength(256);
        b.Property(x => x.Phone).IsRequired().HasMaxLength(30);
    }
}

public sealed class IndividualPartyConfiguration : IEntityTypeConfiguration<IndividualParty>
{
    public void Configure(EntityTypeBuilder<IndividualParty> b)
    {
        b.Property(x => x.NationalId).IsRequired().HasMaxLength(10);   // domain says exactly 10 digits
        b.HasIndex(x => x.NationalId).IsUnique();
    }
}

public sealed class OrganizationPartyConfiguration : IEntityTypeConfiguration<OrganizationParty>
{
    public void Configure(EntityTypeBuilder<OrganizationParty> b)
    {
        b.Property(x => x.RegistrationNumber).IsRequired().HasMaxLength(10);
        b.Property(x => x.ContactPerson).IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.RegistrationNumber).IsUnique();
    }
}