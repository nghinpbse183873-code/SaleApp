using System.Windows;
using System.Windows.Input;

namespace SaleApp.Views.Customer;

public partial class CustomerShell : Window
{
    public CustomerShell() => InitializeComponent();

    private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;
}
