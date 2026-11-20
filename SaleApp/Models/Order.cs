namespace SaleApp.Models;

public class Order
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerUsername { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public int PaymentStatus { get; set; }
    public int OrderStatus { get; set; }
    public string? Note { get; set; }
    public int? ServedByStaffId { get; set; }
    public string? StaffUsername { get; set; }
    public string? StaffName { get; set; }
    public DateTime OrderedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = [];

    public string TotalDisplay => TotalAmount.ToString("N0") + " ₫";
    public string SubTotalDisplay => SubTotal.ToString("N0") + " ₫";
    public string DiscountDisplay => DiscountAmount.ToString("N0") + " ₫";

    public string PaymentStatusText => PaymentStatus switch
    {
        0 => "Chờ thanh toán",
        1 => "Đã thanh toán",
        2 => "Đã hoàn tiền",
        _ => "N/A"
    };

    public string OrderStatusText => OrderStatus switch
    {
        0 => "Chờ xử lý",
        1 => "Đã xác nhận",
        2 => "Đang chuẩn bị",
        3 => "Hoàn thành",
        4 => "Đã huỷ",
        _ => "N/A"
    };
}

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ProductImageUrl { get; set; }

    public string UnitPriceDisplay => UnitPrice.ToString("N0") + " ₫";
    public string TotalPriceDisplay => TotalPrice.ToString("N0") + " ₫";
}
