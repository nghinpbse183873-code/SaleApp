namespace SaleApp.Models;

public class Cart
{
    public int CartId { get; set; }
    public int CustomerId { get; set; }
    public int Status { get; set; } // 0=Active, 1=CheckedOut, 2=Abandoned
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<CartItem> Items { get; set; } = [];

    public decimal Total => Items.Sum(i => i.SubTotal);
    public int TotalQuantity => Items.Sum(i => i.Quantity);
    public string TotalDisplay => Total.ToString("N0") + " ₫";
}

public class CartItem
{
    public int CartItemId { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int StockQty { get; set; }
    public string? Unit { get; set; }
    public DateTime AddedAt { get; set; }

    public decimal SubTotal => Quantity * UnitPrice;
    public string UnitPriceDisplay => UnitPrice.ToString("N0") + " ₫";
    public string SubTotalDisplay => SubTotal.ToString("N0") + " ₫";
}
