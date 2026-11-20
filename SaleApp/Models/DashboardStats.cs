namespace SaleApp.Models;

public class DashboardStats
{
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int TodayCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int PendingOrders { get; set; }

    public string TodayRevenueDisplay => TodayRevenue.ToString("N0") + " ₫";
}

public class RevenueByDay
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public string DateLabel => Date.ToString("dd/MM");
}

public class TopProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public string RevenueDisplay => TotalRevenue.ToString("N0") + " ₫";
}
