using Microsoft.Data.SqlClient;
using SaleApp.Infrastructure;
using SaleApp.Models;

namespace SaleApp.Repositories;

public class CustomerRepository
{
    public List<CustomerPurchaseHistory> GetPurchaseHistory(
        int? customerId = null, DateTime? fromDate = null, DateTime? toDate = null,
        int? orderStatus = null, int? paymentStatus = null,
        int pageIndex = 1, int pageSize = 50)
    {
        return DatabaseHelper.ExecuteStoredProcedure(
            "dbo.sp_GetCustomerPurchaseHistory",
            p =>
            {
                p.AddWithValue("@CustomerId",    (object?)customerId    ?? DBNull.Value);
                p.AddWithValue("@FromDate",      (object?)fromDate      ?? DBNull.Value);
                p.AddWithValue("@ToDate",        (object?)toDate        ?? DBNull.Value);
                p.AddWithValue("@OrderStatus",   (object?)orderStatus   ?? DBNull.Value);
                p.AddWithValue("@PaymentStatus", (object?)paymentStatus ?? DBNull.Value);
                p.AddWithValue("@PageIndex",     pageIndex);
                p.AddWithValue("@PageSize",      pageSize);
            },
            MapHistory);
    }

    public List<CustomerStats> GetCustomerStats(int? customerId = null)
    {
        return DatabaseHelper.ExecuteStoredProcedure(
            "dbo.sp_GetCustomerStats",
            p => p.AddWithValue("@CustomerId", (object?)customerId ?? DBNull.Value),
            r => new CustomerStats
            {
                CustomerId       = r.SafeGetInt("CustomerId"),
                CustomerUsername = r.SafeGetString("CustomerUsername"),
                CustomerName     = r.SafeGet<string>("CustomerName"),
                PhoneNumber      = r.SafeGet<string>("PhoneNumber"),
                RegisteredAt     = r.SafeGetDateTime("RegisteredAt") ?? DateTime.Now,
                LastLoginAt      = r.SafeGetDateTime("LastLoginAt"),
                TotalOrders      = r.SafeGetInt("TotalOrders"),
                CancelledOrders  = r.SafeGetInt("CancelledOrders"),
                CompletedOrders  = r.SafeGetInt("CompletedOrders"),
                TotalSpent       = r.SafeGetDecimal("TotalSpent"),
                AvgOrderValue    = r.SafeGetDecimal("AvgOrderValue"),
                LastOrderAt      = r.SafeGetDateTime("LastOrderAt"),
                FirstOrderAt     = r.SafeGetDateTime("FirstOrderAt"),
            });
    }

    public List<User> GetAllCustomers()
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
            WHERE r.RoleName = 'Customer'
            ORDER BY up.FullName, u.Username
            """;

        return DatabaseHelper.ExecuteQuery(sql, null, r => new User
        {
            UserId           = r.SafeGetInt("UserId"),
            Username         = r.SafeGetString("Username"),
            Email            = r.SafeGetString("Email"),
            RoleId           = r.SafeGetInt("RoleId"),
            RoleName         = r.SafeGetString("RoleName"),
            IsActive         = r.SafeGetBool("IsActive"),
            CreatedAt        = r.SafeGetDateTime("CreatedAt") ?? DateTime.Now,
            FullName         = r.SafeGet<string>("FullName"),
            PhoneNumber      = r.SafeGet<string>("PhoneNumber"),
        });
    }

    private static CustomerPurchaseHistory MapHistory(SqlDataReader r) => new()
    {
        CustomerId       = r.SafeGetInt("CustomerId"),
        CustomerUsername = r.SafeGetString("CustomerUsername"),
        CustomerName     = r.SafeGet<string>("CustomerName"),
        CustomerPhone    = r.SafeGet<string>("CustomerPhone"),
        CustomerEmail    = r.SafeGet<string>("CustomerEmail"),
        OrderId          = r.SafeGetInt("OrderId"),
        OrderCode        = r.SafeGetString("OrderCode"),
        OrderedAt        = r.SafeGetDateTime("OrderedAt") ?? DateTime.Now,
        SubTotal         = r.SafeGetDecimal("SubTotal"),
        DiscountAmount   = r.SafeGetDecimal("DiscountAmount"),
        TotalAmount      = r.SafeGetDecimal("TotalAmount"),
        PaymentMethod    = r.SafeGet<string>("PaymentMethod"),
        PaymentStatus    = r.SafeGetInt("PaymentStatus"),
        PaymentStatusText= r.SafeGet<string>("PaymentStatusText"),
        OrderStatus      = r.SafeGetInt("OrderStatus"),
        OrderStatusText  = r.SafeGet<string>("OrderStatusText"),
        Note             = r.SafeGet<string>("Note"),
        StaffUsername    = r.SafeGet<string>("StaffUsername"),
        StaffName        = r.SafeGet<string>("StaffName"),
        TotalItems       = r.SafeGetInt("TotalItems"),
        TotalQuantity    = r.SafeGetInt("TotalQuantity"),
        TotalCount       = r.SafeGetInt("TotalCount"),
    };
}
