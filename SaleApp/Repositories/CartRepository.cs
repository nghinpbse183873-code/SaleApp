using Microsoft.Data.SqlClient;
using SaleApp.Infrastructure;
using SaleApp.Models;

namespace SaleApp.Repositories;

public class CartRepository
{
    public int GetOrCreateActiveCart(int customerId)
    {
        var result = DatabaseHelper.ExecuteStoredProcedure(
            "dbo.sp_GetOrCreateActiveCart",
            p => p.AddWithValue("@CustomerId", customerId),
            r => r.SafeGetInt("CartId"));

        return result.FirstOrDefault();
    }

    public Cart? GetActiveCart(int customerId)
    {
        var cartId = GetOrCreateActiveCart(customerId);
        if (cartId == 0) return null;

        var cart = new Cart { CartId = cartId, CustomerId = customerId, Status = 0 };
        cart.Items = GetCartItems(cartId);
        return cart;
    }

    public List<CartItem> GetCartItems(int cartId)
    {
        const string sql = """
            SELECT ci.CartItemId, ci.CartId, ci.ProductId, p.ProductName,
                   c.CategoryName, p.ImageUrl, ci.Quantity, ci.UnitPrice,
                   p.StockQty, p.Unit, ci.AddedAt
            FROM dbo.CartItems ci
            INNER JOIN dbo.Products p   ON ci.ProductId = p.ProductId
            INNER JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
            WHERE ci.CartId = @CartId
            ORDER BY ci.AddedAt
            """;

        return DatabaseHelper.ExecuteQuery(sql,
            p => p.AddWithValue("@CartId", cartId),
            r => new CartItem
            {
                CartItemId   = r.SafeGetInt("CartItemId"),
                CartId       = r.SafeGetInt("CartId"),
                ProductId    = r.SafeGetInt("ProductId"),
                ProductName  = r.SafeGetString("ProductName"),
                CategoryName = r.SafeGet<string>("CategoryName"),
                ImageUrl     = r.SafeGet<string>("ImageUrl"),
                Quantity     = r.SafeGetInt("Quantity"),
                UnitPrice    = r.SafeGetDecimal("UnitPrice"),
                StockQty     = r.SafeGetInt("StockQty"),
                Unit         = r.SafeGet<string>("Unit"),
                AddedAt      = r.SafeGetDateTime("AddedAt") ?? DateTime.Now,
            });
    }

    /// <summary>Adds item or increases qty if already in cart.</summary>
    public void AddOrUpdateItem(int cartId, int productId, int quantity, decimal unitPrice)
    {
        DatabaseHelper.ExecuteNonQuery("""
            IF EXISTS (SELECT 1 FROM dbo.CartItems WHERE CartId = @CartId AND ProductId = @ProductId)
                UPDATE dbo.CartItems
                SET Quantity = Quantity + @Qty, UpdatedAt = GETDATE()
                WHERE CartId = @CartId AND ProductId = @ProductId
            ELSE
                INSERT INTO dbo.CartItems (CartId, ProductId, Quantity, UnitPrice)
                VALUES (@CartId, @ProductId, @Qty, @Price)
            """,
            p =>
            {
                p.AddWithValue("@CartId",    cartId);
                p.AddWithValue("@ProductId", productId);
                p.AddWithValue("@Qty",       quantity);
                p.AddWithValue("@Price",     unitPrice);
            });
    }

    public void UpdateItemQty(int cartItemId, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveItem(cartItemId);
            return;
        }
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE dbo.CartItems SET Quantity = @Qty, UpdatedAt = GETDATE() WHERE CartItemId = @Id",
            p => { p.AddWithValue("@Qty", quantity); p.AddWithValue("@Id", cartItemId); });
    }

    public void RemoveItem(int cartItemId)
    {
        DatabaseHelper.ExecuteNonQuery(
            "DELETE FROM dbo.CartItems WHERE CartItemId = @Id",
            p => p.AddWithValue("@Id", cartItemId));
    }

    public void ClearCart(int cartId)
    {
        DatabaseHelper.ExecuteNonQuery(
            "DELETE FROM dbo.CartItems WHERE CartId = @CartId",
            p => p.AddWithValue("@CartId", cartId));
    }
}
