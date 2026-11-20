using System.Windows.Input;
using SaleApp.MVVM;

namespace SaleApp.ViewModels.Staff;

public class StaffShellViewModel : ViewModelBase
{
    private ViewModelBase? _currentViewModel;
    private string _activeMenu = "Dashboard";

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

    public string StaffName { get; }

    // Nav commands
    public ICommand ShowDashboardCommand    { get; }
    public ICommand ShowProductsCommand     { get; }
    public ICommand ShowCategoriesCommand   { get; }
    public ICommand ShowOrdersCommand       { get; }
    public ICommand ShowHistoryCommand      { get; }
    public ICommand LogoutCommand           { get; }

    public StaffShellViewModel()
    {
        StaffName = Infrastructure.AppSession.CurrentUser?.DisplayName ?? "Staff";

        ShowDashboardCommand  = new RelayCommand(() => Navigate("Dashboard",  new DashboardViewModel()));
        ShowProductsCommand   = new RelayCommand(() => Navigate("Products",   new ProductManagementViewModel()));
        ShowCategoriesCommand = new RelayCommand(() => Navigate("Categories", new CategoryManagementViewModel()));
        ShowOrdersCommand     = new RelayCommand(() => Navigate("Orders",     new OrderManagementViewModel()));
        ShowHistoryCommand    = new RelayCommand(() => Navigate("History",    new CustomerHistoryViewModel()));
        LogoutCommand         = new RelayCommand(ExecuteLogout);

        // Start on dashboard
        Navigate("Dashboard", new DashboardViewModel());
    }

    private void Navigate(string menu, ViewModelBase vm)
    {
        ActiveMenu       = menu;
        CurrentViewModel = vm;
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
