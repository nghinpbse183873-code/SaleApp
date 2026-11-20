using Microsoft.Data.SqlClient;
using SaleApp.Infrastructure;
using SaleApp.Models;

namespace SaleApp.Repositories;

public class OrderRepository
{
    /// <summary>Calls sp_CheckoutCart and returns (orderId, orderCode, total).</summary>
    public (int OrderId, string OrderCode, decimal Total) Checkout(
        int cartId, string paymentMethod, string? note = null)
    {
        int    orderId    = 0;
        string orderCode  = string.Empty;
        decimal total     = 0;

        DatabaseHelper.ExecuteStoredProcedureMultiResult(
            "dbo.sp_CheckoutCart",
            p =>
            {
                p.AddWithValue("@CartId",        cartId);
                p.AddWithValue("@PaymentMethod", paymentMethod);
                p.AddWithValue("@Note",          (object?)note ?? DBNull.Value);
            },
            r =>
            {
                orderId   = r.SafeGetInt("OrderId");
                orderCode = r.SafeGetString("OrderCode");
                total     = r.SafeGetDecimal("TotalAmount");
            });

        return (orderId, orderCode, total);
    }

    public List<Order> GetByCustomer(int customerId)
    {
        const string sql = """
            SELECT o.OrderId, o.CustomerId, u.Username AS CustomerUsername, up.FullName AS CustomerName,
                   up.PhoneNumber AS CustomerPhone, o.OrderCode, o.SubTotal, o.DiscountAmount,
                   o.TotalAmount, o.PaymentMethod, o.PaymentStatus, o.OrderStatus, o.Note,
                   o.ServedByStaffId, su.Username AS StaffUsername, sup.FullName AS StaffName,
                   o.OrderedAt, o.UpdatedAt
            FROM dbo.Orders o
            INNER JOIN dbo.Users u          ON o.CustomerId     = u.UserId
            LEFT  JOIN dbo.UserProfiles up  ON u.UserId          = up.UserId
            LEFT  JOIN dbo.Users su         ON o.ServedByStaffId = su.UserId
            LEFT  JOIN dbo.UserProfiles sup ON su.UserId         = sup.UserId
            WHERE o.CustomerId = @CustomerId
            ORDER BY o.OrderedAt DESC
            """;

        return DatabaseHelper.ExecuteQuery(sql,
            p => p.AddWithValue("@CustomerId", customerId),
            MapOrder);
    }

    public List<Order> GetAll(int? orderStatus = null, int? paymentStatus = null,
        DateTime? from = null, DateTime? to = null, string? search = null)
    {
        string sql = """
            SELECT o.OrderId, o.CustomerId, u.Username AS CustomerUsername, up.FullName AS CustomerName,
                   up.PhoneNumber AS CustomerPhone, o.OrderCode, o.SubTotal, o.DiscountAmount,
                   o.TotalAmount, o.PaymentMethod, o.PaymentStatus, o.OrderStatus, o.Note,
                   o.ServedByStaffId, su.Username AS StaffUsername, sup.FullName AS StaffName,
                   o.OrderedAt, o.UpdatedAt
            FROM dbo.Orders o
            INNER JOIN dbo.Users u          ON o.CustomerId     = u.UserId
            LEFT  JOIN dbo.UserProfiles up  ON u.UserId          = up.UserId
            LEFT  JOIN dbo.Users su         ON o.ServedByStaffId = su.UserId
            LEFT  JOIN dbo.UserProfiles sup ON su.UserId         = sup.UserId
            WHERE 1=1
            """;

        if (orderStatus.HasValue)   sql += " AND o.OrderStatus = @OS";
        if (paymentStatus.HasValue) sql += " AND o.PaymentStatus = @PS";
        if (from.HasValue)          sql += " AND CAST(o.OrderedAt AS DATE) >= @From";
        if (to.HasValue)            sql += " AND CAST(o.OrderedAt AS DATE) <= @To";
        if (!string.IsNullOrWhiteSpace(search))
            sql += " AND (o.OrderCode LIKE @S OR u.Username LIKE @S OR up.FullName LIKE @S)";
        sql += " ORDER BY o.OrderedAt DESC";

        return DatabaseHelper.ExecuteQuery(sql,
            p =>
            {
                if (orderStatus.HasValue)   p.AddWithValue("@OS", orderStatus.Value);
                if (paymentStatus.HasValue) p.AddWithValue("@PS", paymentStatus.Value);
                if (from.HasValue)          p.AddWithValue("@From", from.Value);
                if (to.HasValue)            p.AddWithValue("@To", to.Value);
                if (!string.IsNullOrWhiteSpace(search)) p.AddWithValue("@S", $"%{search}%");
            },
            MapOrder);
    }

    public (Order? Header, List<OrderItem> Items) GetDetail(int orderId)
    {
        Order? header = null;
        var items = new List<OrderItem>();

        DatabaseHelper.ExecuteStoredProcedureMultiResult(
            "dbo.sp_GetOrderDetail",
            p => p.AddWithValue("@OrderId", orderId),
            r =>
            {
                header = new Order
                {
                    OrderId         = r.SafeGetInt("OrderId"),
                    CustomerId      = r.SafeGetInt("CustomerId"),
                    CustomerUsername= r.SafeGetString("CustomerUsername"),
                    CustomerName    = r.SafeGet<string>("CustomerName"),
                    CustomerPhone   = r.SafeGet<string>("CustomerPhone"),
                    OrderCode       = r.SafeGetString("OrderCode"),
                    OrderedAt       = r.SafeGetDateTime("OrderedAt") ?? DateTime.Now,
                    SubTotal        = r.SafeGetDecimal("SubTotal"),
                    DiscountAmount  = r.SafeGetDecimal("DiscountAmount"),
                    TotalAmount     = r.SafeGetDecimal("TotalAmount"),
                    PaymentMethod   = r.SafeGet<string>("PaymentMethod"),
                    PaymentStatus   = r.SafeGetInt("PaymentStatus"),
                    OrderStatus     = r.SafeGetInt("OrderStatus"),
                    Note            = r.SafeGet<string>("Note"),
                    StaffUsername   = r.SafeGet<string>("StaffUsername"),
                    StaffName       = r.SafeGet<string>("StaffName"),
                };
            },
            r =>
            {
                items.Add(new OrderItem
                {
                    OrderItemId     = r.SafeGetInt("OrderItemId"),
                    OrderId         = orderId,
                    ProductId       = r.SafeGetInt("ProductId"),
                    ProductName     = r.SafeGetString("ProductName"),
                    CategoryName    = r.SafeGet<string>("CategoryName"),
                    Quantity        = r.SafeGetInt("Quantity"),
                    UnitPrice       = r.SafeGetDecimal("UnitPrice"),
                    TotalPrice      = r.SafeGetDecimal("TotalPrice"),
                    ProductImageUrl = r.SafeGet<string>("ProductImageUrl"),
                });
            });

        return (header, items);
    }

    public void UpdateStatus(int orderId, int orderStatus, int paymentStatus, int staffId)
    {
        DatabaseHelper.ExecuteStoredProcedure<int>(
            "dbo.sp_UpdateOrderPayment",
            p =>
            {
                p.AddWithValue("@OrderId",       orderId);
                p.AddWithValue("@PaymentStatus", paymentStatus);
                p.AddWithValue("@OrderStatus",   orderStatus);
                p.AddWithValue("@StaffId",       staffId);
            },
            r => r.SafeGetInt("RowsAffected"));
    }

    private static Order MapOrder(SqlDataReader r) => new()
    {
        OrderId         = r.SafeGetInt("OrderId"),
        CustomerId      = r.SafeGetInt("CustomerId"),
        CustomerUsername= r.SafeGetString("CustomerUsername"),
        CustomerName    = r.SafeGet<string>("CustomerName"),
        CustomerPhone   = r.SafeGet<string>("CustomerPhone"),
        OrderCode       = r.SafeGetString("OrderCode"),
        SubTotal        = r.SafeGetDecimal("SubTotal"),
        DiscountAmount  = r.SafeGetDecimal("DiscountAmount"),
        TotalAmount     = r.SafeGetDecimal("TotalAmount"),
        PaymentMethod   = r.SafeGet<string>("PaymentMethod"),
        PaymentStatus   = r.SafeGetInt("PaymentStatus"),
        OrderStatus     = r.SafeGetInt("OrderStatus"),
        Note            = r.SafeGet<string>("Note"),
        ServedByStaffId = r.SafeGet<int?>("ServedByStaffId"),
        StaffUsername   = r.SafeGet<string>("StaffUsername"),
        StaffName       = r.SafeGet<string>("StaffName"),
        OrderedAt       = r.SafeGetDateTime("OrderedAt") ?? DateTime.Now,
        UpdatedAt       = r.SafeGetDateTime("UpdatedAt") ?? DateTime.Now,
    };
}
