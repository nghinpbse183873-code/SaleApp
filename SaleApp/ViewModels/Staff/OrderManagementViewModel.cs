using System.Collections.ObjectModel;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Repositories;

namespace SaleApp.ViewModels.Staff;

public class OrderManagementViewModel : ViewModelBase
{
    private readonly OrderRepository _orderRepo = new();

    private Order? _selectedOrder;
    private string _searchText = "";
    private int _filterStatus = -1;
    private bool _isLoading;

    public Order? SelectedOrder
    {
        get => _selectedOrder;
        set { SetProperty(ref _selectedOrder, value); OnPropertyChanged(nameof(HasSelection)); }
    }

    public bool   HasSelection => SelectedOrder != null;
    public bool   IsLoading    { get => _isLoading;    set => SetProperty(ref _isLoading,    value); }

    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); LoadOrders(); }
    }

    public int FilterStatus
    {
        get => _filterStatus;
        set { SetProperty(ref _filterStatus, value); LoadOrders(); }
    }

    public ObservableCollection<Order> Orders { get; } = [];

    public record StatusOption(int Value, string Label);

    public List<StatusOption> StatusOptions { get; } =
    [
        new(-1, "Tất cả"),
        new(0,  "Chờ xử lý"),
        new(1,  "Đã xác nhận"),
        new(2,  "Đang chuẩn bị"),
        new(3,  "Hoàn thành")
    ];

    public ICommand RefreshCommand    { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand ConfirmCommand    { get; }
    public ICommand CompleteCommand   { get; }
    public ICommand CancelOrderCommand{ get; }

    public OrderManagementViewModel()
    {
        RefreshCommand     = new RelayCommand(LoadOrders);
        ViewDetailCommand  = new RelayCommand(ExecuteViewDetail,  () => HasSelection);
        ConfirmCommand     = new RelayCommand(ExecuteConfirm,     () => SelectedOrder?.OrderStatus == 0);
        CompleteCommand    = new RelayCommand(ExecuteComplete,    () => SelectedOrder?.OrderStatus is 1 or 2);
        CancelOrderCommand = new RelayCommand(ExecuteCancelOrder, () => SelectedOrder?.OrderStatus is 0 or 1);
        LoadOrders();
    }

    private void LoadOrders()
    {
        IsLoading = true;
        try
        {
            int? status = _filterStatus >= 0 ? _filterStatus : null;
            var list = _orderRepo.GetAll(orderStatus: status,
                search: string.IsNullOrWhiteSpace(_searchText) ? null : _searchText.Trim());
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
            LoadOrders();
        }
    }

    private void ExecuteConfirm()
    {
        if (SelectedOrder == null) return;
        UpdateStatus(1, SelectedOrder.PaymentStatus);
    }

    private void ExecuteComplete()
    {
        if (SelectedOrder == null) return;
        UpdateStatus(3, 1); // Completed + Paid
    }

    private void ExecuteCancelOrder()
    {
        if (SelectedOrder == null) return;
        UpdateStatus(4, SelectedOrder.PaymentStatus);
    }

    private void UpdateStatus(int orderStatus, int paymentStatus)
    {
        var staffId = Infrastructure.AppSession.CurrentUser!.UserId;
        _orderRepo.UpdateStatus(SelectedOrder!.OrderId, orderStatus, paymentStatus, staffId);
        LoadOrders();
    }
}
