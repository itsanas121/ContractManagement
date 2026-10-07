using ContractManagement.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractManagement.Infrastructure.Persistence.Configurations;

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> b)
    {
        b.ToTable("Contracts");
        b.HasKey(x => x.Id);

        b.Property(x => x.ContractNumber).IsRequired().HasMaxLength(50);
        b.HasIndex(x => x.ContractNumber).IsUnique();          // the number must be unique
        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.ContractValue).HasPrecision(18, 2);   // money, 2 decimals
        b.Property(x => x.RejectionReason).HasMaxLength(1000);
        b.Property(x => x.TerminationReason).HasMaxLength(1000);

        // enums are saved as text so the table is readable
        b.Property(x => x.ContractType).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        b.Property(x => x.RowVersion).IsRowVersion();           // stops two people from overwriting each other

        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.EndDate);                             // the expiry job will filter on this

        // Restrict = deleting a company/user must not silently delete contracts
        b.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.ApprovedById).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.RejectedById).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.TerminatedById).OnDelete(DeleteBehavior.Restrict);

        // the contract owns its parties and documents, so they are deleted with it
        b.HasMany(x => x.Parties).WithOne().HasForeignKey(p => p.ContractId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Documents).WithOne().HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.Cascade);

        // Parties/Documents are read-only views of the private lists, so EF must fill the fields
        b.Navigation(x => x.Parties).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}