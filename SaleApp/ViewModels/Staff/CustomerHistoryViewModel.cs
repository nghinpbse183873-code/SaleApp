using System.Collections.ObjectModel;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Repositories;

namespace SaleApp.ViewModels.Staff;

public class CustomerHistoryViewModel : ViewModelBase
{
    private readonly CustomerRepository _customerRepo = new();
    private readonly OrderRepository    _orderRepo    = new();

    private User? _selectedCustomer;
    private CustomerPurchaseHistory? _selectedHistory;
    private DateTime? _fromDate = DateTime.Today.AddMonths(-1);
    private DateTime? _toDate   = DateTime.Today;
    private int _filterStatus   = -1;
    private bool _isLoading;

    public User? SelectedCustomer
    {
        get => _selectedCustomer;
        set { SetProperty(ref _selectedCustomer, value); LoadHistory(); }
    }

    public CustomerPurchaseHistory? SelectedHistory
    {
        get => _selectedHistory;
        set { SetProperty(ref _selectedHistory, value); OnPropertyChanged(nameof(HasSelection)); }
    }

    public bool HasSelection => SelectedHistory != null;
    public bool IsLoading    { get => _isLoading;  set => SetProperty(ref _isLoading, value); }

    public DateTime? FromDate
    {
        get => _fromDate;
        set { SetProperty(ref _fromDate, value); LoadHistory(); }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set { SetProperty(ref _toDate, value); LoadHistory(); }
    }

    public int FilterStatus
    {
        get => _filterStatus;
        set { SetProperty(ref _filterStatus, value); LoadHistory(); }
    }

    public ObservableCollection<User>                    Customers { get; } = [];
    public ObservableCollection<CustomerPurchaseHistory> History   { get; } = [];
    public ObservableCollection<CustomerStats>           Stats     { get; } = [];

    public int TotalRecords    { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string TotalAmountDisplay => TotalAmount.ToString("N0") + " ₫";

    public record StatusOption(int Value, string Label);

    public List<StatusOption> StatusOptions { get; } =
    [
        new(-1, "Tất cả trạng thái"),
        new(0, "Chờ xử lý"),
        new(1, "Đã xác nhận"),
        new(2, "Đang chuẩn bị"),
        new(3, "Hoàn thành")
    ];

    public ICommand RefreshCommand    { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand ClearFilterCommand{ get; }

    public CustomerHistoryViewModel()
    {
        RefreshCommand     = new RelayCommand(LoadHistory);
        ViewDetailCommand  = new RelayCommand(ExecuteViewDetail, () => HasSelection);
        ClearFilterCommand = new RelayCommand(ClearFilters);
        LoadCustomers();
        LoadStats();
        LoadHistory();
    }

    private void LoadCustomers()
    {
        var list = _customerRepo.GetAllCustomers();
        Customers.Clear();
        Customers.Add(new User { UserId = 0, FullName = "Tất cả khách hàng", Username = "all" });
        foreach (var c in list) Customers.Add(c);
        _selectedCustomer = Customers.First();
    }

    private void LoadStats()
    {
        var list = _customerRepo.GetCustomerStats();
        Stats.Clear();
        foreach (var s in list) Stats.Add(s);
    }

    private void LoadHistory()
    {
        IsLoading = true;
        try
        {
            int? custId = _selectedCustomer?.UserId > 0 ? _selectedCustomer.UserId : null;
            int? status = _filterStatus >= 0 ? _filterStatus : null;

            var list = _customerRepo.GetPurchaseHistory(
                customerId:    custId,
                fromDate:      _fromDate,
                toDate:        _toDate,
                orderStatus:   status,
                pageSize:      100);

            History.Clear();
            foreach (var h in list) History.Add(h);

            TotalRecords = list.FirstOrDefault()?.TotalCount ?? list.Count;
            TotalAmount  = list.Where(h => h.PaymentStatus == 1).Sum(h => h.TotalAmount);
            OnPropertyChanged(nameof(TotalRecords));
            OnPropertyChanged(nameof(TotalAmount));
            OnPropertyChanged(nameof(TotalAmountDisplay));
        }
        finally { IsLoading = false; }
    }

    private void ExecuteViewDetail()
    {
        if (SelectedHistory == null) return;
        var (header, items) = _orderRepo.GetDetail(SelectedHistory.OrderId);
        if (header != null)
        {
            header.Items.AddRange(items);
            var dlg = new Views.Staff.Dialogs.OrderDetailDialog(header);
            dlg.ShowDialog();
        }
    }

    private void ClearFilters()
    {
        _fromDate       = DateTime.Today.AddMonths(-1);
        _toDate         = DateTime.Today;
        _filterStatus   = -1;
        _selectedCustomer = Customers.FirstOrDefault();
        OnPropertyChanged(nameof(FromDate));
        OnPropertyChanged(nameof(ToDate));
        OnPropertyChanged(nameof(FilterStatus));
        OnPropertyChanged(nameof(SelectedCustomer));
        LoadHistory();
    }
}
