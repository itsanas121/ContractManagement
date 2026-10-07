using ContractManagement.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Core.Application.Common;

// The services in Core only use this interface, never the real DbContext.
// That way Core doesn't need a reference to Infrastructure.
public interface IAppDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<User> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<ContractParty> ContractParties { get; }
    DbSet<ContractDocument> ContractDocuments { get; }
    DbSet<Party> Parties { get; }      // base type, use OfType<...>() for one kind
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}