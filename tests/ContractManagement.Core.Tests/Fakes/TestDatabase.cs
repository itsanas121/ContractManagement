using ContractManagement.Core.Domain.Entities;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ContractManagement.Core.Tests;

// One in-memory SQLite database per instance. xUnit creates a new test-class
// instance for every test, so every test gets a fresh database.
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteRowVersionModelCustomizer>()
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

// SQLite has no rowversion, so give the column a default value; otherwise inserting a Contract fails.
internal sealed class SqliteRowVersionModelCustomizer : RelationalModelCustomizer
{
    public SqliteRowVersionModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        modelBuilder.Entity<Contract>()
            .Property(c => c.RowVersion)
            .HasDefaultValueSql("(randomblob(8))");

        // SQLite can't SUM decimals, so store ContractValue as double in tests only
        modelBuilder.Entity<Contract>()
            .Property(c => c.ContractValue)
            .HasConversion<double>();
    }
}