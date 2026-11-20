using System.Windows;
using System.Windows.Input;
using SaleApp.ViewModels;

namespace SaleApp.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow() => InitializeComponent();

    private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
        {
            vm.Password        = PwdPassword.Password;
            vm.ConfirmPassword = PwdConfirm.Password;
            if (vm.RegisterCommand.CanExecute(null))
                vm.RegisterCommand.Execute(null);
        }
    }
}
