using SaleApp.Models;
using SaleApp.Repositories;

namespace SaleApp.Services;

public class CartService
{
    private readonly CartRepository    _cartRepo    = new();
    private readonly ProductRepository _productRepo = new();
    private readonly OrderRepository   _orderRepo   = new();

    public Cart? GetActiveCart(int customerId) => _cartRepo.GetActiveCart(customerId);

    public record AddResult(bool Success, string? Error);

    public AddResult AddToCart(int customerId, int productId, int quantity = 1)
    {
        if (quantity <= 0) return new(false, "Số lượng phải lớn hơn 0.");

        var product = _productRepo.GetById(productId);
        if (product == null || !product.IsActive)
            return new(false, "Sản phẩm không tồn tại hoặc ngừng bán.");

        var cartId = _cartRepo.GetOrCreateActiveCart(customerId);
        var currentQty = _cartRepo.GetCartItems(cartId).FirstOrDefault(x => x.ProductId == productId)?.Quantity ?? 0;

        if (product.StockQty < quantity + currentQty)
        {
            if (currentQty > 0)
                return new(false, $"Tồn kho chỉ còn {product.StockQty} {product.Unit} (bạn đã có {currentQty} trong giỏ).");
            return new(false, $"Tồn kho chỉ còn {product.StockQty} {product.Unit}.");
        }

        _cartRepo.AddOrUpdateItem(cartId, productId, quantity, product.Price);

        return new(true, null);
    }

    public void UpdateQty(int cartItemId, int quantity) => _cartRepo.UpdateItemQty(cartItemId, quantity);
    public void RemoveItem(int cartItemId) => _cartRepo.RemoveItem(cartItemId);
    public void ClearCart(int cartId) => _cartRepo.ClearCart(cartId);

    public record CheckoutResult(bool Success, string? Error, int OrderId = 0, string OrderCode = "");

    public CheckoutResult Checkout(int customerId, string paymentMethod, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
            return new(false, "Vui lòng chọn phương thức thanh toán.");

        var cart = _cartRepo.GetActiveCart(customerId);
        if (cart == null || cart.Items.Count == 0)
            return new(false, "Giỏ hàng trống.");

        // Re-check stock for each item
        foreach (var item in cart.Items)
        {
            var product = _productRepo.GetById(item.ProductId);
            if (product == null || !product.IsActive)
                return new(false, $"Sản phẩm \"{item.ProductName}\" không còn bán.");
            if (product.StockQty < item.Quantity)
                return new(false, $"Sản phẩm \"{item.ProductName}\" chỉ còn {product.StockQty} {product.Unit}.");
        }

        var (orderId, orderCode, _) = _orderRepo.Checkout(cart.CartId, paymentMethod, note);
        return new(true, null, orderId, orderCode);
    }
}
