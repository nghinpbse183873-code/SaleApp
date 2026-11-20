using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Services;

namespace SaleApp.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly AuthService _authService = new();

    private string _username = "", _email = "", _fullName = "", _phone = "";
    private string _errorMessage = "", _successMessage = "";
    private bool _isLoading;
    private Role? _selectedRole;

    public string Username   { get => _username;   set => SetProperty(ref _username,   value); }
    public string Email      { get => _email;      set => SetProperty(ref _email,      value); }
    public string FullName   { get => _fullName;   set => SetProperty(ref _fullName,   value); }
    public string Phone      { get => _phone;      set => SetProperty(ref _phone,      value); }
    public bool   IsLoading  { get => _isLoading;  set => SetProperty(ref _isLoading,  value); }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public string SuccessMessage
    {
        get => _successMessage;
        set { SetProperty(ref _successMessage, value); OnPropertyChanged(nameof(HasSuccess)); }
    }
    public bool HasSuccess => !string.IsNullOrEmpty(_successMessage);

    public Role? SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    public ObservableCollection<Role> Roles { get; } = [];

    // Passwords come from code-behind
    public string? Password        { private get; set; }
    public string? ConfirmPassword { private get; set; }

    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public RegisterViewModel()
    {
        RegisterCommand  = new RelayCommand(ExecuteRegister,  () => !IsLoading);
        GoToLoginCommand = new RelayCommand(ExecuteGoToLogin);
        LoadRoles();
    }

    private void LoadRoles()
    {
        var roles = _authService.GetRoles();
        Roles.Clear();
        foreach (var r in roles) Roles.Add(r);
        SelectedRole = Roles.FirstOrDefault(r => r.RoleName == "Customer");
    }

    private void ExecuteRegister()
    {
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;
        IsLoading      = true;

        try
        {
            if (SelectedRole == null) { ErrorMessage = "Vui lòng chọn loại tài khoản."; return; }

            var result = _authService.Register(
                Username, Email, Password ?? "", ConfirmPassword ?? "",
                SelectedRole.RoleId, FullName, Phone);

            if (!result.Success) { ErrorMessage = result.Error!; return; }

            SuccessMessage = "Đăng ký thành công! Quay lại đăng nhập...";
            Task.Delay(1500).ContinueWith(_ =>
                Application.Current.Dispatcher.Invoke(ExecuteGoToLogin));
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExecuteGoToLogin()
    {
        var loginWindow = new Views.LoginWindow();
        loginWindow.Show();
        Application.Current.Windows.OfType<Views.RegisterWindow>().FirstOrDefault()?.Close();
    }
}
