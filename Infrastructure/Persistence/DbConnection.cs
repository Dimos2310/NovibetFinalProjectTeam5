namespace Infrastructure.Persistence;

// Ένα σημείο για το connection string — το χρησιμοποιούν ο DbContext,
// ο Dapper και το design-time factory για τα migrations.
// ΠΡΟΣΟΧΗ: local dev credential για το SQL Server στο Docker.
// Σε πραγματικό project θα πήγαινε σε user-secrets / appsettings / env variable.
public static class DbConnection //we do not want  to make for example new DbConnection() because we want to use the same connection string for all the different ways of connecting to the database
{
    public const string ConnectionString =
        "Server=localhost,1433;Database=ExpressYourself;User Id=sa;Password=Test123@;TrustServerCertificate=True"; //Endeiktika
  }