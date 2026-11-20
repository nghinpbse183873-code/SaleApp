using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.Services;

namespace SaleApp.Views.Staff.Dialogs;

public partial class ProductFormDialog : Window
{
    private readonly Product? _existing;
    private readonly ProductService _service = new();

    public ProductFormDialog(Product? existing, IEnumerable<Category> categories)
    {
        InitializeComponent();
        _existing = existing;

        // Populate categories (skip "All" entry)
        CmbCategory.ItemsSource = categories.Where(c => c.CategoryId > 0).ToList();

        if (existing != null)
        {
            TxtTitle.Text        = "Sửa sản phẩm";
            TxtName.Text         = existing.ProductName;
            TxtPrice.Text        = existing.Price.ToString("F0");
            TxtCostPrice.Text    = existing.CostPrice?.ToString("F0") ?? "";
            TxtStock.Text        = existing.StockQty.ToString();
            TxtUnit.Text         = existing.Unit ?? "phần";
            TxtDesc.Text         = existing.Description ?? "";
            ChkActive.IsChecked  = existing.IsActive;
            ChkFeatured.IsChecked= existing.IsFeatured;
            CmbCategory.SelectedValue = existing.CategoryId;
        }
    }

    private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorBorder.Visibility = Visibility.Collapsed;

        if (!decimal.TryParse(TxtPrice.Text.Replace(",", ""), out var price))
        { ShowError("Giá bán không hợp lệ."); return; }

        if (!int.TryParse(TxtStock.Text, out var stock))
        { ShowError("Tồn kho không hợp lệ."); return; }

        decimal? costPrice = null;
        if (!string.IsNullOrWhiteSpace(TxtCostPrice.Text))
        {
            if (!decimal.TryParse(TxtCostPrice.Text.Replace(",", ""), out var cp))
            { ShowError("Giá vốn không hợp lệ."); return; }
            costPrice = cp;
        }

        var product = new Product
        {
            ProductId   = _existing?.ProductId ?? 0,
            CategoryId  = (int)(CmbCategory.SelectedValue ?? 0),
            ProductName = TxtName.Text.Trim(),
            Description = TxtDesc.Text.Trim(),
            Price       = price,
            CostPrice   = costPrice,
            StockQty    = stock,
            Unit        = TxtUnit.Text.Trim(),
            IsActive    = ChkActive.IsChecked == true,
            IsFeatured  = ChkFeatured.IsChecked == true,
        };

        var staffId = Infrastructure.AppSession.CurrentUser!.UserId;
        var result = _existing == null
            ? _service.Create(product, staffId)
            : _service.Update(product);

        if (!result.Success) { ShowError(result.Error!); return; }

        DialogResult = true;
        Close();
    }

    private void ShowError(string msg)
    {
        TxtError.Text             = msg;
        ErrorBorder.Visibility    = Visibility.Visible;
    }
}
