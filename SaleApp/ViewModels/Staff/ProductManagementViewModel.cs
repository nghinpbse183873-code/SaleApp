using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Services;

namespace SaleApp.ViewModels.Staff;

public class ProductManagementViewModel : ViewModelBase
{
    private readonly ProductService _service = new();

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
        set { SetProperty(ref _selectedProduct, value); OnPropertyChanged(nameof(HasSelection)); }
    }

    public bool HasSelection => SelectedProduct != null;
    public bool IsLoading    { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public ObservableCollection<Product>  Products   { get; } = [];
    public ObservableCollection<Category> Categories { get; } = [];

    public ICommand AddCommand    { get; }
    public ICommand EditCommand   { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand{ get; }

    public ProductManagementViewModel()
    {
        AddCommand     = new RelayCommand(ExecuteAdd);
        EditCommand    = new RelayCommand(ExecuteEdit,   () => HasSelection);
        DeleteCommand  = new RelayCommand(ExecuteDelete, () => HasSelection);
        RefreshCommand = new RelayCommand(LoadProducts);

        LoadCategories();
        LoadProducts();
    }

    private void LoadCategories()
    {
        var cats = _service.GetCategories(activeOnly: true);
        Categories.Clear();
        Categories.Add(new Category { CategoryId = 0, CategoryName = "Tất cả danh mục" });
        foreach (var c in cats) Categories.Add(c);
        _selectedCategory = Categories.First();
    }

    private void LoadProducts()
    {
        IsLoading = true;
        try
        {
            int? catId = _selectedCategory?.CategoryId > 0 ? _selectedCategory.CategoryId : null;
            var list = _service.GetAll(catId, isActive: null,
                string.IsNullOrWhiteSpace(_searchText) ? null : _searchText.Trim());
            Products.Clear();
            foreach (var p in list.OrderBy(x => x.ProductId)) Products.Add(p);
        }
        finally { IsLoading = false; }
    }

    private void ExecuteAdd()
    {
        var dialog = new Views.Staff.Dialogs.ProductFormDialog(null, Categories);
        if (dialog.ShowDialog() == true) LoadProducts();
    }

    private void ExecuteEdit()
    {
        if (SelectedProduct == null) return;
        var dialog = new Views.Staff.Dialogs.ProductFormDialog(SelectedProduct, Categories);
        if (dialog.ShowDialog() == true) LoadProducts();
    }

    private void ExecuteDelete()
    {
        if (SelectedProduct == null) return;

        var r = MessageBox.Show(
            $"Xoá sản phẩm \"{SelectedProduct.ProductName}\"?",
            "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (r != MessageBoxResult.Yes) return;

        var result = _service.Delete(SelectedProduct.ProductId);
        if (!result.Success) MessageBox.Show(result.Error, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        else LoadProducts();
    }
}
