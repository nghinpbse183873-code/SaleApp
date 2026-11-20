namespace SaleApp.Models;

public class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQty { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int? CreatedByStaffId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // UI helpers
    public string PriceDisplay => Price.ToString("N0") + " ₫";
    public string StockDisplay => $"{StockQty} {Unit}";
    public bool IsLowStock => StockQty is > 0 and <= 10;
    public bool IsOutOfStock => StockQty <= 0;
    public decimal? Profit => CostPrice.HasValue ? Price - CostPrice.Value : null;
}
