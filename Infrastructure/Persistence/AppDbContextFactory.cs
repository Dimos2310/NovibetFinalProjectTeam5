using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

// EF's command-line tools (Add-Migration, Update-Database) run outside of the actual
// app, so there's no Program.cs/DI container available for them to get a DbContext from.
// This factory exists purely so those tools have a way to build one themselves.
// It is NOT used when the app is actually running - see DependencyInjection.cs for that.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(DbConnection.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}
