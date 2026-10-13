using ContractManagement.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}