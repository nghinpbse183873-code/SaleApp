using SaleApp.Models;
using SaleApp.Repositories;

namespace SaleApp.Services;

public class ProductService
{
    private readonly ProductRepository  _productRepo  = new();
    private readonly CategoryRepository _categoryRepo = new();

    public List<Product> GetAll(int? categoryId = null, bool? isActive = true, string? search = null)
        => _productRepo.GetAll(categoryId, isActive, search);

    public Product? GetById(int id) => _productRepo.GetById(id);

    public List<Category> GetCategories(bool activeOnly = true)
        => _categoryRepo.GetAll(!activeOnly);

    public record SaveResult(bool Success, string? Error, int ProductId = 0);

    public SaveResult Create(Product product, int staffId)
    {
        var err = Validate(product);
        if (err != null) return new(false, err);

        int id = _productRepo.Create(product, staffId);
        return new(true, null, id);
    }

    public SaveResult Update(Product product)
    {
        var err = Validate(product);
        if (err != null) return new(false, err);

        _productRepo.Update(product);
        return new(true, null, product.ProductId);
    }

    public SaveResult Delete(int productId)
    {
        bool ok = _productRepo.Delete(productId);
        return ok ? new(true, null) : new(false, "Không tìm thấy sản phẩm.");
    }

    private static string? Validate(Product p)
    {
        if (string.IsNullOrWhiteSpace(p.ProductName)) return "Tên sản phẩm không được trống.";
        if (p.ProductName.Length > 200)               return "Tên sản phẩm tối đa 200 ký tự.";
        if (p.Price < 0)                              return "Giá bán không được âm.";
        if (p.StockQty < 0)                           return "Tồn kho không được âm.";
        if (p.CategoryId <= 0)                        return "Vui lòng chọn danh mục.";
        return null;
    }
}
