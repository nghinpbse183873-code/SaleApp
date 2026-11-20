using System.Windows.Input;
using SaleApp.MVVM;

namespace SaleApp.ViewModels.Customer;

public class CustomerShellViewModel : ViewModelBase
{
    private ViewModelBase? _currentViewModel;
    private string _activeMenu = "Store";
    private int _cartItemCount;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string ActiveMenu
    {
        get => _activeMenu;
        set => SetProperty(ref _activeMenu, value);
    }

    public int CartItemCount
    {
        get => _cartItemCount;
        set { SetProperty(ref _cartItemCount, value); OnPropertyChanged(nameof(CartBadge)); }
    }

    public string CartBadge => CartItemCount > 0 ? CartItemCount.ToString() : "";
    public string CustomerName { get; } = Infrastructure.AppSession.CurrentUser?.DisplayName ?? "Customer";

    public ICommand ShowStoreCommand   { get; }
    public ICommand ShowCartCommand    { get; }
    public ICommand ShowHistoryCommand { get; }
    public ICommand LogoutCommand      { get; }

    private StoreViewModel? _storeVm;
    private CartViewModel?  _cartVm;

    public CustomerShellViewModel()
    {
        _storeVm = new StoreViewModel(this);
        _cartVm  = new CartViewModel(this);

        ShowStoreCommand   = new RelayCommand(() => Navigate("Store",   _storeVm));
        ShowCartCommand    = new RelayCommand(() => Navigate("Cart",    _cartVm));
        ShowHistoryCommand = new RelayCommand(() => Navigate("History", new OrderHistoryViewModel()));
        LogoutCommand      = new RelayCommand(ExecuteLogout);

        Navigate("Store", _storeVm);
    }

    private void Navigate(string menu, ViewModelBase vm)
    {
        ActiveMenu       = menu;
        CurrentViewModel = vm;

        // Refresh cart count when switching to cart view
        if (vm is CartViewModel cartVm) cartVm.Refresh();
    }

    public void RefreshCartCount()
    {
        var cart = new Services.CartService().GetActiveCart(
            Infrastructure.AppSession.CurrentUser!.UserId);
        CartItemCount = cart?.TotalQuantity ?? 0;
    }

    private void ExecuteLogout()
    {
        Infrastructure.AppSession.Logout();
        var login = new Views.LoginWindow();
        login.Show();

        // Close all other windows
        var windowsToClose = System.Windows.Application.Current.Windows
            .Cast<System.Windows.Window>()
            .Where(w => w != login)
            .ToList();

        foreach (var window in windowsToClose)
        {
            window.Close();
        }
    }
}
