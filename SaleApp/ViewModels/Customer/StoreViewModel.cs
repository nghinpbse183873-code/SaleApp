using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Services;

namespace SaleApp.ViewModels.Customer;

public class StoreViewModel : ViewModelBase
{
    private readonly ProductService _productService = new();
    private readonly CartService    _cartService    = new();
    private readonly CustomerShellViewModel _shell;

    private string _searchText = "";
    private Category? _selectedCategory;
    private Product? _selectedProduct;
    private bool _isLoading;

    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); LoadProducts(); }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set { SetProperty(ref _selectedCategory, value); LoadProducts(); }
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public ObservableCollection<ProductItemViewModel> Products { get; } = [];
    public ObservableCollection<Category> Categories { get; } = [];

    public ICommand AddToCartCommand { get; }
    public ICommand RefreshCommand   { get; }

    public StoreViewModel(CustomerShellViewModel shell)
    {
        _shell = shell;
        AddToCartCommand = new RelayCommand<ProductItemViewModel>(ExecuteAddToCart);
        RefreshCommand   = new RelayCommand(LoadProducts);
        LoadCategories();
        LoadProducts();
    }

    private void LoadCategories()
    {
        var cats = _productService.GetCategories(activeOnly: true);
        Categories.Clear();
        Categories.Add(new Category { CategoryId = 0, CategoryName = "🍽️ Tất cả" });
        foreach (var c in cats) Categories.Add(c);
        _selectedCategory = Categories.First();
    }

    private void LoadProducts()
    {
        IsLoading = true;
        try
        {
            int? catId = _selectedCategory?.CategoryId > 0 ? _selectedCategory.CategoryId : null;
            var list = _productService.GetAll(catId, isActive: true,
                string.IsNullOrWhiteSpace(_searchText) ? null : _searchText.Trim());
            Products.Clear();
            foreach (var p in list) Products.Add(new ProductItemViewModel(p));
        }
        finally { IsLoading = false; }
    }

    private void ExecuteAddToCart(ProductItemViewModel? item)
    {
        if (item == null) return;
        var customerId = Infrastructure.AppSession.CurrentUser!.UserId;
        var result = _cartService.AddToCart(customerId, item.Product.ProductId, item.Quantity);

        if (!result.Success)
        {
            MessageBox.Show(result.Error, "Không thể thêm", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _shell.RefreshCartCount();
        MessageBox.Show($"Đã thêm {item.Quantity} x \"{item.Product.ProductName}\" vào giỏ hàng! 🛒",
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            
        item.Quantity = 1; // Reset after adding
    }
}

public class ProductItemViewModel : ViewModelBase
{
    public Product Product { get; }
    private int _quantity = 1;
    
    public int Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }
    
    public ICommand IncreaseCommand { get; }
    public ICommand DecreaseCommand { get; }

    public ProductItemViewModel(Product product)
    {
        Product = product;
        IncreaseCommand = new RelayCommand(() => { if (Quantity < Product.StockQty) Quantity++; });
        DecreaseCommand = new RelayCommand(() => { if (Quantity > 1) Quantity--; });
    }
}

/// <summary>Generic RelayCommand with typed parameter.</summary>
public class RelayCommand<T> : System.Windows.Input.ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?>? _canExecute;

    public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
    {
        _execute    = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => System.Windows.Input.CommandManager.RequerySuggested += value;
        remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
    public void Execute(object? parameter) => _execute((T?)parameter);
}
