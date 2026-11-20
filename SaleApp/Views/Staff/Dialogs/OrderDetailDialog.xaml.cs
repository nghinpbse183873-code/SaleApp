using System.Windows;
using System.Windows.Input;
using SaleApp.Models;

namespace SaleApp.Views.Staff.Dialogs;

public partial class OrderDetailDialog : Window
{
    public OrderDetailDialog(Order order)
    {
        InitializeComponent();
        DataContext = order;
    }

    private void DragBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
