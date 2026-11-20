using System.Windows;
using System.Windows.Input;
using SaleApp.Infrastructure;
using SaleApp.MVVM;
using SaleApp.Services;

namespace SaleApp.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService = new();

    private string _username = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    // Password is passed from code-behind (PasswordBox doesn't bind directly)
    public string? Password { private get; set; }

    public LoginViewModel()
    {
        LoginCommand      = new RelayCommand(ExecuteLogin,    () => !IsLoading);
        GoToRegisterCommand = new RelayCommand(ExecuteGoToRegister);
    }

    private void ExecuteLogin()
    {
        ErrorMessage = string.Empty;
        IsLoading    = true;

        try
        {
            var result = _authService.Login(Username?.Trim() ?? "", Password ?? "");

            if (!result.Success)
            {
                ErrorMessage = result.Error ?? "Đăng nhập thất bại.";
                return;
            }

            AppSession.Login(result.User!);

            // Open the correct window based on role
            if (AppSession.IsStaff)
            {
                var staffShell = new Views.Staff.StaffShell();
                staffShell.Show();
            }
            else
            {
                var customerShell = new Views.Customer.CustomerShell();
                customerShell.Show();
            }

            // Close login window
            Application.Current.Windows
                .OfType<Views.LoginWindow>()
                .FirstOrDefault()?.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi kết nối: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExecuteGoToRegister()
    {
        var regWindow = new Views.RegisterWindow();
        regWindow.Show();

        Application.Current.Windows
            .OfType<Views.LoginWindow>()
            .FirstOrDefault()?.Close();
    }
}
