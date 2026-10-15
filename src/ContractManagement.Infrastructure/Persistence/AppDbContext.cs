using ContractManagement.Core.Application.Common;
using ContractManagement.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ContractManagement.Infrastructure.Persistence;

// The real DbContext. It implements IAppDbContext so Core can use it through DI.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractParty> ContractParties => Set<ContractParty>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    => Database.BeginTransactionAsync(cancellationToken);
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // finds every IEntityTypeConfiguration<T> class in this project,
        // so I don't have to register each configuration by hand
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}