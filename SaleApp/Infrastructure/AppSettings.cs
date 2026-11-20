using System.Collections.Specialized;
using System.Configuration;

namespace SaleApp.Infrastructure;

public static class AppSettings
{
    public static string ConnectionString =>
        ConfigurationManager.AppSettings["ConnectionString"]
        ?? "Server=(localdb)\\MSSQLLocalDB;Database=SaleAppDB;Trusted_Connection=True;TrustServerCertificate=True;";
}
