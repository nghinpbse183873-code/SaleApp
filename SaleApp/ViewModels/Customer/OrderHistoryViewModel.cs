using System.Collections.ObjectModel;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Repositories;

namespace SaleApp.ViewModels.Customer;

public class OrderHistoryViewModel : ViewModelBase
{
    private readonly OrderRepository _orderRepo = new();

    private Order? _selectedOrder;
    private bool _isLoading;

    public Order? SelectedOrder
    {
        get => _selectedOrder;
        set { SetProperty(ref _selectedOrder, value); OnPropertyChanged(nameof(HasSelection)); }
    }

    public bool HasSelection => SelectedOrder != null;
    public bool IsLoading    { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public ObservableCollection<Order> Orders { get; } = [];

    public ICommand RefreshCommand    { get; }
    public ICommand ViewDetailCommand { get; }

    public OrderHistoryViewModel()
    {
        RefreshCommand    = new RelayCommand(LoadOrders);
        ViewDetailCommand = new RelayCommand(ExecuteViewDetail, () => HasSelection);
        LoadOrders();
    }

    private void LoadOrders()
    {
        IsLoading = true;
        try
        {
            var customerId = Infrastructure.AppSession.CurrentUser!.UserId;
            var list = _orderRepo.GetByCustomer(customerId);
            Orders.Clear();
            foreach (var o in list) Orders.Add(o);
        }
        finally { IsLoading = false; }
    }

    private void ExecuteViewDetail()
    {
        if (SelectedOrder == null) return;
        var (header, items) = _orderRepo.GetDetail(SelectedOrder.OrderId);
        if (header != null)
        {
            header.Items.AddRange(items);
            var dlg = new Views.Staff.Dialogs.OrderDetailDialog(header);
            dlg.ShowDialog();
        }
    }
}
