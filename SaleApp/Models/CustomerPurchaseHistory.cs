namespace SaleApp.Models;

public class CustomerPurchaseHistory
{
    public int CustomerId { get; set; }
    public string CustomerUsername { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderedAt { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public int PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public int OrderStatus { get; set; }
    public string? OrderStatusText { get; set; }
    public string? Note { get; set; }
    public string? StaffUsername { get; set; }
    public string? StaffName { get; set; }
    public int TotalItems { get; set; }
    public int TotalQuantity { get; set; }
    public int TotalCount { get; set; } // for paging

    public string TotalDisplay => TotalAmount.ToString("N0") + " ₫";
    public string DisplayName => CustomerName ?? CustomerUsername;
}

public class CustomerStats
{
    public int CustomerId { get; set; }
    public string CustomerUsername { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int TotalOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AvgOrderValue { get; set; }
    public DateTime? LastOrderAt { get; set; }
    public DateTime? FirstOrderAt { get; set; }

    public string TotalSpentDisplay => TotalSpent.ToString("N0") + " ₫";
    public string AvgOrderDisplay => AvgOrderValue.ToString("N0") + " ₫";
    public string DisplayName => CustomerName ?? CustomerUsername;
}
