using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests;

// Builds a real (if temporary) database in memory using Sqlite, from the exact same
// AppDbContext model the real app uses. That means a passing test actually proves the
// entity config, keys, indexes and foreign key work - not just that the code compiles.
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");

        // Sqlite's in-memory database only exists while this connection is open -
        // closing it would wipe everything, so we keep it open for the whole test run.
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated(); // builds the tables from AppDbContext's model
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
