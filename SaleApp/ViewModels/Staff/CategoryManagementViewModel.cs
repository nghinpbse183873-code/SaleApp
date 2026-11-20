using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Repositories;

namespace SaleApp.ViewModels.Staff;

public class CategoryManagementViewModel : ViewModelBase
{
    private readonly CategoryRepository _repo = new();

    private Category? _selectedCategory;
    private string _editName = "", _editDesc = "";
    private bool _isEditing;

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);
            if (value != null) { EditName = value.CategoryName; EditDesc = value.Description ?? ""; }
            OnPropertyChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection => SelectedCategory != null;
    public bool IsEditing    { get => _isEditing; set => SetProperty(ref _isEditing, value); }
    public string EditName   { get => _editName;  set => SetProperty(ref _editName,  value); }
    public string EditDesc   { get => _editDesc;  set => SetProperty(ref _editDesc,  value); }

    public ObservableCollection<Category> Categories { get; } = [];

    public ICommand NewCommand    { get; }
    public ICommand SaveCommand   { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }

    public CategoryManagementViewModel()
    {
        NewCommand    = new RelayCommand(ExecuteNew);
        SaveCommand   = new RelayCommand(ExecuteSave,   () => IsEditing && !string.IsNullOrWhiteSpace(EditName));
        CancelCommand = new RelayCommand(ExecuteCancel, () => IsEditing);
        DeleteCommand = new RelayCommand(ExecuteDelete, () => HasSelection && !IsEditing);
        Load();
    }

    private void Load()
    {
        var list = _repo.GetAll(includeInactive: true);
        Categories.Clear();
        foreach (var c in list) Categories.Add(c);
    }

    private void ExecuteNew()
    {
        SelectedCategory = null;
        EditName  = "";
        EditDesc  = "";
        IsEditing = true;
    }

    private void ExecuteSave()
    {
        if (string.IsNullOrWhiteSpace(EditName)) return;

        if (SelectedCategory == null)
        {
            _repo.Create(new Category { CategoryName = EditName.Trim(), Description = EditDesc.Trim(), IsActive = true });
        }
        else
        {
            SelectedCategory.CategoryName = EditName.Trim();
            SelectedCategory.Description  = EditDesc.Trim();
            _repo.Update(SelectedCategory);
        }

        IsEditing = false;
        Load();
    }

    private void ExecuteCancel() { IsEditing = false; }

    private void ExecuteDelete()
    {
        if (SelectedCategory == null) return;
        var r = MessageBox.Show($"Ẩn danh mục \"{SelectedCategory.CategoryName}\"?",
            "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r != MessageBoxResult.Yes) return;
        _repo.Delete(SelectedCategory.CategoryId);
        Load();
    }
}
