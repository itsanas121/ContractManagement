using ContractManagement.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractManagement.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(x => x.Id);
        b.Property(x => x.EntityType).HasConversion<string>().HasMaxLength(100).IsRequired();
        b.Property(x => x.Action).HasConversion<string>().HasMaxLength(50).IsRequired();
        b.HasIndex(x => new { x.EntityType, x.EntityId });   // to read the history of one record
        b.HasIndex(x => x.PerformedAt);
        // no foreign key on PerformedById on purpose: the log must stay even if the user is removed
    }
}