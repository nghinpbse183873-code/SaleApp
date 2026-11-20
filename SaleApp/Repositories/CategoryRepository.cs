using Microsoft.Data.SqlClient;
using SaleApp.Infrastructure;
using SaleApp.Models;

namespace SaleApp.Repositories;

public class CategoryRepository
{
    public List<Category> GetAll(bool includeInactive = false)
    {
        string sql = """
            SELECT c.CategoryId, c.CategoryName, c.Description, c.ImageUrl,
                   c.DisplayOrder, c.IsActive, c.CreatedAt, c.UpdatedAt,
                   (SELECT COUNT(*) FROM dbo.Products p WHERE p.CategoryId = c.CategoryId AND p.IsActive = 1) AS ProductCount
            FROM dbo.Categories c
            """;

        if (!includeInactive) sql += " WHERE c.IsActive = 1";
        sql += " ORDER BY c.DisplayOrder, c.CategoryName";

        return DatabaseHelper.ExecuteQuery(sql, null, MapCategory);
    }

    public Category? GetById(int id)
    {
        const string sql = """
            SELECT c.CategoryId, c.CategoryName, c.Description, c.ImageUrl,
                   c.DisplayOrder, c.IsActive, c.CreatedAt, c.UpdatedAt,
                   (SELECT COUNT(*) FROM dbo.Products p WHERE p.CategoryId = c.CategoryId AND p.IsActive = 1) AS ProductCount
            FROM dbo.Categories c
            WHERE c.CategoryId = @Id
            """;

        return DatabaseHelper.ExecuteQuery(sql, p => p.AddWithValue("@Id", id), MapCategory)
            .FirstOrDefault();
    }

    public int Create(Category cat)
    {
        var result = DatabaseHelper.ExecuteScalar("""
            INSERT INTO dbo.Categories (CategoryName, Description, ImageUrl, DisplayOrder, IsActive)
            VALUES (@Name, @Desc, @Img, @Order, @Active);
            SELECT SCOPE_IDENTITY();
            """,
            p =>
            {
                p.AddWithValue("@Name",   cat.CategoryName);
                p.AddWithValue("@Desc",   (object?)cat.Description  ?? DBNull.Value);
                p.AddWithValue("@Img",    (object?)cat.ImageUrl      ?? DBNull.Value);
                p.AddWithValue("@Order",  cat.DisplayOrder);
                p.AddWithValue("@Active", cat.IsActive);
            });
        return System.Convert.ToInt32(result);
    }

    public void Update(Category cat)
    {
        DatabaseHelper.ExecuteNonQuery("""
            UPDATE dbo.Categories
            SET CategoryName = @Name, Description = @Desc, ImageUrl = @Img,
                DisplayOrder = @Order, IsActive = @Active, UpdatedAt = GETDATE()
            WHERE CategoryId = @Id
            """,
            p =>
            {
                p.AddWithValue("@Name",   cat.CategoryName);
                p.AddWithValue("@Desc",   (object?)cat.Description  ?? DBNull.Value);
                p.AddWithValue("@Img",    (object?)cat.ImageUrl      ?? DBNull.Value);
                p.AddWithValue("@Order",  cat.DisplayOrder);
                p.AddWithValue("@Active", cat.IsActive);
                p.AddWithValue("@Id",     cat.CategoryId);
            });
    }

    public bool Delete(int categoryId)
    {
        // Soft-delete: just deactivate
        var rows = DatabaseHelper.ExecuteNonQuery(
            "UPDATE dbo.Categories SET IsActive = 0, UpdatedAt = GETDATE() WHERE CategoryId = @Id",
            p => p.AddWithValue("@Id", categoryId));
        return rows > 0;
    }

    private static Category MapCategory(SqlDataReader r) => new()
    {
        CategoryId   = r.SafeGetInt("CategoryId"),
        CategoryName = r.SafeGetString("CategoryName"),
        Description  = r.SafeGet<string>("Description"),
        ImageUrl     = r.SafeGet<string>("ImageUrl"),
        DisplayOrder = r.SafeGetInt("DisplayOrder"),
        IsActive     = r.SafeGetBool("IsActive"),
        CreatedAt    = r.SafeGetDateTime("CreatedAt") ?? DateTime.Now,
        UpdatedAt    = r.SafeGetDateTime("UpdatedAt") ?? DateTime.Now,
        ProductCount = r.SafeGetInt("ProductCount"),
    };
}
