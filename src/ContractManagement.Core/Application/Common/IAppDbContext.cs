using ContractManagement.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ContractManagement.Core.Application.Common;

// The services in Core only use this interface, never the real DbContext.
// That way Core doesn't need a reference to Infrastructure.
public interface IAppDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<User> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Contract> Contracts { get; }

    // ContractParty and ContractDocument sets are intentionally not exposed.
    // Add them through Contract.AddParty() / AddDocument() to enforce domain rules.
    DbSet<Party> Parties { get; }      // base type, use OfType<...>() for one kind
    DbSet<AuditLog> AuditLogs { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}