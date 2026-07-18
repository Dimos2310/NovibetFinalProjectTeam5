namespace Infrastructure.Persistence;

// One single place for the connection string, used by both the real app (DependencyInjection.cs)
// and the design-time factory above (needed for Add-Migration/Update-Database). Keeping it in
// one spot means we can't end up with two different connection strings drifting apart again.
// NOTE: in a real production project this would live in appsettings/user-secrets/env vars,
// not committed to source control - fine for a bootcamp project, not fine for real credentials.
public static class DbConnection
{
    public const string ConnectionString =
        "Server=localhost;Database=ExpressYourself;Trusted_Connection=True;TrustServerCertificate=True;";
}
