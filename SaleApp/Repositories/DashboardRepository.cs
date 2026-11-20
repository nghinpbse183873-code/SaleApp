using Microsoft.Data.SqlClient;
using SaleApp.Infrastructure;
using SaleApp.Models;

namespace SaleApp.Repositories;

public class DashboardRepository
{
    public DashboardStats GetTodayStats()
    {
        const string sql = """
            SELECT
                ISNULL((SELECT SUM(TotalAmount) FROM dbo.Orders
                         WHERE CAST(OrderedAt AS DATE) = CAST(GETDATE() AS DATE)
                           AND OrderStatus = 3 AND PaymentStatus = 1), 0)        AS TodayRevenue,
                ISNULL((SELECT COUNT(*) FROM dbo.Orders
                         WHERE CAST(OrderedAt AS DATE) = CAST(GETDATE() AS DATE)
                           AND OrderStatus = 3), 0)                               AS TodayOrders,
                ISNULL((SELECT COUNT(DISTINCT CustomerId) FROM dbo.Orders
                         WHERE CAST(OrderedAt AS DATE) = CAST(GETDATE() AS DATE)), 0) AS TodayCustomers,
                (SELECT COUNT(*) FROM dbo.Products WHERE IsActive = 1)            AS TotalProducts,
                (SELECT COUNT(*) FROM dbo.Products WHERE IsActive = 1 AND StockQty > 0 AND StockQty <= 10) AS LowStockProducts,
                (SELECT COUNT(*) FROM dbo.Orders WHERE OrderStatus = 0)           AS PendingOrders
            """;

        var list = DatabaseHelper.ExecuteQuery(sql, null, r => new DashboardStats
        {
            TodayRevenue    = r.SafeGetDecimal("TodayRevenue"),
            TodayOrders     = r.SafeGetInt("TodayOrders"),
            TodayCustomers  = r.SafeGetInt("TodayCustomers"),
            TotalProducts   = r.SafeGetInt("TotalProducts"),
            LowStockProducts= r.SafeGetInt("LowStockProducts"),
            PendingOrders   = r.SafeGetInt("PendingOrders"),
        });

        return list.FirstOrDefault() ?? new DashboardStats();
    }

    public List<RevenueByDay> GetRevenueByDay(int days = 7)
    {
        string sql = $"""
            SELECT CAST(o.OrderedAt AS DATE) AS OrderDate,
                   SUM(o.TotalAmount) AS Revenue,
                   COUNT(*) AS Orders
            FROM dbo.Orders o
            WHERE o.OrderedAt >= DATEADD(DAY, -{days - 1}, CAST(GETDATE() AS DATE))
              AND o.OrderStatus = 3
              AND o.PaymentStatus = 1
            GROUP BY CAST(o.OrderedAt AS DATE)
            ORDER BY CAST(o.OrderedAt AS DATE)
            """;

        var data = DatabaseHelper.ExecuteQuery(sql, null, r => new RevenueByDay
        {
            Date    = r.SafeGetDateTime("OrderDate") ?? DateTime.Today,
            Revenue = r.SafeGetDecimal("Revenue"),
            Orders  = r.SafeGetInt("Orders"),
        });

        // Fill missing days with 0
        var result = new List<RevenueByDay>();
        for (int i = days - 1; i >= 0; i--)
        {
            var date  = DateTime.Today.AddDays(-i);
            var found = data.FirstOrDefault(d => d.Date.Date == date.Date);
            result.Add(found ?? new RevenueByDay { Date = date, Revenue = 0, Orders = 0 });
        }
        return result;
    }

    public List<TopProduct> GetTopProducts(int top = 10)
    {
        string sql = $"""
            SELECT TOP {top}
                ProductId, ProductName, CategoryName,
                TotalQuantitySold, TotalRevenue, OrderCount
            FROM dbo.vw_TopSellingProducts
            ORDER BY TotalRevenue DESC
            """;

        return DatabaseHelper.ExecuteQuery(sql, null, r => new TopProduct
        {
            ProductId         = r.SafeGetInt("ProductId"),
            ProductName       = r.SafeGetString("ProductName"),
            CategoryName      = r.SafeGetString("CategoryName"),
            TotalQuantitySold = r.SafeGetInt("TotalQuantitySold"),
            TotalRevenue      = r.SafeGetDecimal("TotalRevenue"),
            OrderCount        = r.SafeGetInt("OrderCount"),
        });
    }

    public List<(int Year, int Month, decimal Revenue, int Orders)> GetMonthlyRevenue(int year)
    {
        const string sql = """
            SELECT [Year], [Month], TotalRevenue, TotalOrders
            FROM dbo.vw_MonthlyRevenue
            WHERE [Year] = @Year
            ORDER BY [Month]
            """;

        return DatabaseHelper.ExecuteQuery(sql,
            p => p.AddWithValue("@Year", year),
            r => (
                r.SafeGetInt("Year"),
                r.SafeGetInt("Month"),
                r.SafeGetDecimal("TotalRevenue"),
                r.SafeGetInt("TotalOrders")
            ));
    }
}
