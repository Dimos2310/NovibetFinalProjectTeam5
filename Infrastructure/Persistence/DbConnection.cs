namespace Infrastructure.Persistence;

// Single source of truth for the connection string (used by both the real app and
// AppDbContextFactory). Would move to user-secrets/env vars in a real deployment.
public static class DbConnection
{
    public const string ConnectionString =
        "Server=localhost;Database=ExpressYourself;Trusted_Connection=True;TrustServerCertificate=True;";
}
