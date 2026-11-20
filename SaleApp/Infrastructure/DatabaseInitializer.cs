using Microsoft.Data.SqlClient;

namespace SaleApp.Infrastructure;

/// <summary>
/// Runs once on startup:
/// 1. Verifies DB connection
/// 2. Seeds default admin/customer accounts with proper BCrypt hashes
///    if they still have placeholder hashes from the SQL script.
/// </summary>
public static class DatabaseInitializer
{
    private const string DefaultStaffPassword   = "Admin@123";
    private const string DefaultCustomerPassword = "Customer@123";

    public static (bool success, string message) Initialize()
    {
        try
        {
            // 1. Auto-create & seed DB from script
            ExecuteInitScript();

            // 2. Connect to the (now guaranteed existing) database
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();

            // 3. Fix placeholder hashes in seed data
            FixPlaceholderHashes(conn);

            return (true, "OK");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private static void ExecuteInitScript()
    {
        var scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "init_database.sql");
        if (!System.IO.File.Exists(scriptPath)) 
        {
            scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Database", "init_database.sql");
            if (!System.IO.File.Exists(scriptPath)) return;
        }

        var scriptText = System.IO.File.ReadAllText(scriptPath);
        var commands = System.Text.RegularExpressions.Regex.Split(scriptText, @"^\s*GO\s*$", 
            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Connect to 'master' so we can CREATE DATABASE if it doesn't exist.
        // The script contains 'USE SaleAppDB', which will naturally switch the context.
        var originalConnStr = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
        var builder = new SqlConnectionStringBuilder(originalConnStr) { InitialCatalog = "master" };

        using var conn = new SqlConnection(builder.ConnectionString);
        conn.Open();

        foreach (var cmdText in commands)
        {
            var sql = cmdText.Trim();
            if (string.IsNullOrWhiteSpace(sql)) continue;

            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }

    private static void FixPlaceholderHashes(SqlConnection conn)
    {
        // Find users still carrying the SQL-seed placeholder hash
        const string findSql = """
            SELECT u.UserId, u.Username, r.RoleName
            FROM   dbo.Users u
            INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
            WHERE  u.PasswordHash LIKE '$2a$10$PLACEHOLDER%'
            """;

        var toFix = new List<(int Id, string Username, string Role)>();

        using (var cmd = new SqlCommand(findSql, conn))
        using (var reader = cmd.ExecuteReader())
            while (reader.Read())
                toFix.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));

        if (toFix.Count == 0) return;

        const string updateSql = "UPDATE dbo.Users SET PasswordHash = @Hash WHERE UserId = @Id";
        foreach (var (id, _, role) in toFix)
        {
            var rawPassword = role == "StoreStaff" ? DefaultStaffPassword : DefaultCustomerPassword;
            var hash        = BCrypt.Net.BCrypt.HashPassword(rawPassword, 10);

            using var cmd = new SqlCommand(updateSql, conn);
            cmd.Parameters.AddWithValue("@Hash", hash);
            cmd.Parameters.AddWithValue("@Id",   id);
            cmd.ExecuteNonQuery();
        }
    }
}
