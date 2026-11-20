using Microsoft.Data.SqlClient;
using SaleApp.Infrastructure;
using SaleApp.Models;

namespace SaleApp.Repositories;

public class ProductRepository
{
    public List<Product> GetAll(int? categoryId = null, bool? isActive = true, string? search = null)
    {
        string sql = """
            SELECT p.ProductId, p.CategoryId, c.CategoryName, p.ProductName, p.Description,
                   p.Price, p.CostPrice, p.StockQty, p.Unit, p.ImageUrl,
                   p.IsActive, p.IsFeatured, p.CreatedByStaffId, p.CreatedAt, p.UpdatedAt
            FROM dbo.Products p
            INNER JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
            WHERE 1=1
            """;

        if (isActive.HasValue)   sql += " AND p.IsActive = @Active";
        if (categoryId.HasValue) sql += " AND p.CategoryId = @CatId";
        if (!string.IsNullOrWhiteSpace(search)) sql += " AND (p.ProductName LIKE @Search OR p.Description LIKE @Search)";
        sql += " ORDER BY c.DisplayOrder, p.ProductName";

        return DatabaseHelper.ExecuteQuery(sql,
            p =>
            {
                if (isActive.HasValue)   p.AddWithValue("@Active", isActive.Value);
                if (categoryId.HasValue) p.AddWithValue("@CatId", categoryId.Value);
                if (!string.IsNullOrWhiteSpace(search)) p.AddWithValue("@Search", $"%{search}%");
            },
            MapProduct);
    }

    public Product? GetById(int productId)
    {
        const string sql = """
            SELECT p.ProductId, p.CategoryId, c.CategoryName, p.ProductName, p.Description,
                   p.Price, p.CostPrice, p.StockQty, p.Unit, p.ImageUrl,
                   p.IsActive, p.IsFeatured, p.CreatedByStaffId, p.CreatedAt, p.UpdatedAt
            FROM dbo.Products p
            INNER JOIN dbo.Categories c ON p.CategoryId = c.CategoryId
            WHERE p.ProductId = @Id
            """;

        return DatabaseHelper.ExecuteQuery(sql, p => p.AddWithValue("@Id", productId), MapProduct)
            .FirstOrDefault();
    }

    public int Create(Product product, int staffId)
    {
        var result = DatabaseHelper.ExecuteScalar("""
            INSERT INTO dbo.Products
                (CategoryId, ProductName, Description, Price, CostPrice, StockQty, Unit, ImageUrl,
                 IsActive, IsFeatured, CreatedByStaffId)
            VALUES
                (@CatId, @Name, @Desc, @Price, @CostPrice, @Stock, @Unit, @Img,
                 @Active, @Featured, @StaffId);
            SELECT SCOPE_IDENTITY();
            """,
            p =>
            {
                p.AddWithValue("@CatId",    product.CategoryId);
                p.AddWithValue("@Name",     product.ProductName);
                p.AddWithValue("@Desc",     (object?)product.Description ?? DBNull.Value);
                p.AddWithValue("@Price",    product.Price);
                p.AddWithValue("@CostPrice",(object?)product.CostPrice   ?? DBNull.Value);
                p.AddWithValue("@Stock",    product.StockQty);
                p.AddWithValue("@Unit",     (object?)product.Unit        ?? DBNull.Value);
                p.AddWithValue("@Img",      (object?)product.ImageUrl    ?? DBNull.Value);
                p.AddWithValue("@Active",   product.IsActive);
                p.AddWithValue("@Featured", product.IsFeatured);
                p.AddWithValue("@StaffId",  staffId);
            });
        return System.Convert.ToInt32(result);
    }

    public void Update(Product product)
    {
        DatabaseHelper.ExecuteNonQuery("""
            UPDATE dbo.Products
            SET CategoryId = @CatId, ProductName = @Name, Description = @Desc,
                Price = @Price, CostPrice = @CostPrice, StockQty = @Stock,
                Unit = @Unit, ImageUrl = @Img, IsActive = @Active,
                IsFeatured = @Featured, UpdatedAt = GETDATE()
            WHERE ProductId = @Id
            """,
            p =>
            {
                p.AddWithValue("@CatId",    product.CategoryId);
                p.AddWithValue("@Name",     product.ProductName);
                p.AddWithValue("@Desc",     (object?)product.Description ?? DBNull.Value);
                p.AddWithValue("@Price",    product.Price);
                p.AddWithValue("@CostPrice",(object?)product.CostPrice   ?? DBNull.Value);
                p.AddWithValue("@Stock",    product.StockQty);
                p.AddWithValue("@Unit",     (object?)product.Unit        ?? DBNull.Value);
                p.AddWithValue("@Img",      (object?)product.ImageUrl    ?? DBNull.Value);
                p.AddWithValue("@Active",   product.IsActive);
                p.AddWithValue("@Featured", product.IsFeatured);
                p.AddWithValue("@Id",       product.ProductId);
            });
    }

    public bool Delete(int productId)
    {
        var rows = DatabaseHelper.ExecuteNonQuery(
            "UPDATE dbo.Products SET IsActive = 0, UpdatedAt = GETDATE() WHERE ProductId = @Id",
            p => p.AddWithValue("@Id", productId));
        return rows > 0;
    }

    public bool UpdateStock(int productId, int newQty)
    {
        var rows = DatabaseHelper.ExecuteNonQuery(
            "UPDATE dbo.Products SET StockQty = @Qty, UpdatedAt = GETDATE() WHERE ProductId = @Id",
            p => { p.AddWithValue("@Qty", newQty); p.AddWithValue("@Id", productId); });
        return rows > 0;
    }

    private static Product MapProduct(SqlDataReader r) => new()
    {
        ProductId        = r.SafeGetInt("ProductId"),
        CategoryId       = r.SafeGetInt("CategoryId"),
        CategoryName     = r.SafeGetString("CategoryName"),
        ProductName      = r.SafeGetString("ProductName"),
        Description      = r.SafeGet<string>("Description"),
        Price            = r.SafeGetDecimal("Price"),
        CostPrice        = r.SafeGet<decimal?>("CostPrice"),
        StockQty         = r.SafeGetInt("StockQty"),
        Unit             = r.SafeGet<string>("Unit"),
        ImageUrl         = r.SafeGet<string>("ImageUrl"),
        IsActive         = r.SafeGetBool("IsActive"),
        IsFeatured       = r.SafeGetBool("IsFeatured"),
        CreatedByStaffId = r.SafeGet<int?>("CreatedByStaffId"),
        CreatedAt        = r.SafeGetDateTime("CreatedAt") ?? DateTime.Now,
        UpdatedAt        = r.SafeGetDateTime("UpdatedAt") ?? DateTime.Now,
    };
}
