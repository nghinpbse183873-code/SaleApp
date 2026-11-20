using Microsoft.Data.SqlClient;
using SaleApp.Infrastructure;
using SaleApp.Models;

namespace SaleApp.Repositories;

public class AuthRepository
{
    public User? GetByUsername(string username)
    {
        const string sql = """
            SELECT u.UserId, u.Username, u.Email, u.PasswordHash, u.RoleId,
                   r.RoleName, u.IsActive, u.IsEmailVerified, u.FailedLoginCount,
                   u.LockedUntil, u.LastLoginAt, u.CreatedAt,
                   up.FullName, up.PhoneNumber, up.Address, up.AvatarUrl,
                   up.DateOfBirth, up.Gender
            FROM dbo.Users u
            INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
            LEFT  JOIN dbo.UserProfiles up ON u.UserId = up.UserId
            WHERE u.Username = @Username
            """;

        var list = DatabaseHelper.ExecuteQuery(sql,
            p => p.AddWithValue("@Username", username),
            MapUser);

        return list.FirstOrDefault();
    }

    public User? GetByEmail(string email)
    {
        const string sql = """
            SELECT u.UserId, u.Username, u.Email, u.PasswordHash, u.RoleId,
                   r.RoleName, u.IsActive, u.IsEmailVerified, u.FailedLoginCount,
                   u.LockedUntil, u.LastLoginAt, u.CreatedAt,
                   up.FullName, up.PhoneNumber, up.Address, up.AvatarUrl,
                   up.DateOfBirth, up.Gender
            FROM dbo.Users u
            INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId
            LEFT  JOIN dbo.UserProfiles up ON u.UserId = up.UserId
            WHERE u.Email = @Email
            """;

        var list = DatabaseHelper.ExecuteQuery(sql,
            p => p.AddWithValue("@Email", email),
            MapUser);

        return list.FirstOrDefault();
    }

    public bool UsernameExists(string username)
    {
        var result = DatabaseHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM dbo.Users WHERE Username = @u",
            p => p.AddWithValue("@u", username));
        return result is int i && i > 0;
    }

    public bool EmailExists(string email)
    {
        var result = DatabaseHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM dbo.Users WHERE Email = @e",
            p => p.AddWithValue("@e", email));
        return result is int i && i > 0;
    }

    public int Register(string username, string email, string passwordHash, int roleId, string? fullName, string? phone)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            const string insertUser = """
                INSERT INTO dbo.Users (Username, Email, PasswordHash, RoleId, IsActive, IsEmailVerified)
                VALUES (@Username, @Email, @PasswordHash, @RoleId, 1, 1);
                SELECT SCOPE_IDENTITY();
                """;

            using var cmd = new SqlCommand(insertUser, conn, tx);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            var userId = System.Convert.ToInt32(cmd.ExecuteScalar());

            const string insertProfile = """
                INSERT INTO dbo.UserProfiles (UserId, FullName, PhoneNumber)
                VALUES (@UserId, @FullName, @Phone)
                """;
            using var cmd2 = new SqlCommand(insertProfile, conn, tx);
            cmd2.Parameters.AddWithValue("@UserId", userId);
            cmd2.Parameters.AddWithValue("@FullName", (object?)fullName ?? DBNull.Value);
            cmd2.Parameters.AddWithValue("@Phone", (object?)phone ?? DBNull.Value);
            cmd2.ExecuteNonQuery();

            tx.Commit();
            return userId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void UpdateLastLogin(int userId)
    {
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE dbo.Users SET LastLoginAt = GETDATE(), FailedLoginCount = 0 WHERE UserId = @Id",
            p => p.AddWithValue("@Id", userId));
    }

    public void IncrementFailedLogin(int userId)
    {
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE dbo.Users SET FailedLoginCount = FailedLoginCount + 1 WHERE UserId = @Id",
            p => p.AddWithValue("@Id", userId));
    }

    public void LockAccount(int userId, int minutes = 15)
    {
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE dbo.Users SET LockedUntil = DATEADD(MINUTE, @m, GETDATE()) WHERE UserId = @Id",
            p => { p.AddWithValue("@m", minutes); p.AddWithValue("@Id", userId); });
    }

    public List<Role> GetAllRoles()
    {
        return DatabaseHelper.ExecuteQuery(
            "SELECT RoleId, RoleName, Description, CreatedAt FROM dbo.Roles ORDER BY RoleId",
            null,
            r => new Role
            {
                RoleId      = r.SafeGetInt("RoleId"),
                RoleName    = r.SafeGetString("RoleName"),
                Description = r.SafeGet<string>("Description"),
                CreatedAt   = r.SafeGetDateTime("CreatedAt") ?? DateTime.Now
            });
    }

    private static User MapUser(SqlDataReader r) => new()
    {
        UserId           = r.SafeGetInt("UserId"),
        Username         = r.SafeGetString("Username"),
        Email            = r.SafeGetString("Email"),
        PasswordHash     = r.SafeGetString("PasswordHash"),
        RoleId           = r.SafeGetInt("RoleId"),
        RoleName         = r.SafeGetString("RoleName"),
        IsActive         = r.SafeGetBool("IsActive"),
        IsEmailVerified  = r.SafeGetBool("IsEmailVerified"),
        FailedLoginCount = r.SafeGetInt("FailedLoginCount"),
        LockedUntil      = r.SafeGetDateTime("LockedUntil"),
        LastLoginAt      = r.SafeGetDateTime("LastLoginAt"),
        CreatedAt        = r.SafeGetDateTime("CreatedAt") ?? DateTime.Now,
        FullName         = r.SafeGet<string>("FullName"),
        PhoneNumber      = r.SafeGet<string>("PhoneNumber"),
        Address          = r.SafeGet<string>("Address"),
        AvatarUrl        = r.SafeGet<string>("AvatarUrl"),
        DateOfBirth      = r.SafeGet<DateTime?>("DateOfBirth"),
        Gender           = r.SafeGet<byte?>("Gender"),
    };
}
