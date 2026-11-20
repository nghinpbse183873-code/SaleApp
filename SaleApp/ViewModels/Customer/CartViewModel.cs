using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Services;

namespace SaleApp.ViewModels.Customer;

public class CartViewModel : ViewModelBase
{
    private readonly CartService _cartService = new();
    private readonly CustomerShellViewModel _shell;

    private Cart? _cart;
    private string _selectedPayment = "Cash";
    private string _note = "";
    private bool _isCheckingOut;

    public Cart? Cart      { get => _cart;     set { SetProperty(ref _cart, value); OnPropertyChanged(nameof(HasItems)); OnPropertyChanged(nameof(IsEmpty)); } }
    public bool HasItems   => _cart?.Items.Count > 0;
    public bool IsEmpty    => !HasItems;
    public bool IsCheckingOut { get => _isCheckingOut; set => SetProperty(ref _isCheckingOut, value); }

    public string SelectedPayment { get => _selectedPayment; set => SetProperty(ref _selectedPayment, value); }
    public string Note            { get => _note;            set => SetProperty(ref _note,            value); }

    public ObservableCollection<CartItem> Items { get; } = [];

    public List<string> PaymentMethods { get; } = ["Cash", "Transfer", "MoMo", "VNPay"];

    public ICommand RemoveItemCommand { get; }
    public ICommand IncreaseQtyCommand{ get; }
    public ICommand DecreaseQtyCommand{ get; }
    public ICommand CheckoutCommand   { get; }
    public ICommand ClearCartCommand  { get; }

    public CartViewModel(CustomerShellViewModel shell)
    {
        _shell = shell;
        RemoveItemCommand  = new RelayCommand<CartItem>(ExecuteRemove);
        IncreaseQtyCommand = new RelayCommand<CartItem>(item => UpdateQty(item, 1));
        DecreaseQtyCommand = new RelayCommand<CartItem>(item => UpdateQty(item, -1));
        CheckoutCommand    = new RelayCommand(ExecuteCheckout, () => HasItems && !IsCheckingOut);
        ClearCartCommand   = new RelayCommand(ExecuteClearCart, () => HasItems);
        Refresh();
    }

    public void Refresh()
    {
        var customerId = Infrastructure.AppSession.CurrentUser!.UserId;
        Cart = _cartService.GetActiveCart(customerId);
        Items.Clear();
        if (Cart != null) foreach (var item in Cart.Items) Items.Add(item);
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Cart));
    }

    private void ExecuteRemove(CartItem? item)
    {
        if (item == null) return;
        _cartService.RemoveItem(item.CartItemId);
        Refresh();
        _shell.RefreshCartCount();
    }

    private void UpdateQty(CartItem? item, int delta)
    {
        if (item == null) return;
        int newQty = item.Quantity + delta;
        _cartService.UpdateQty(item.CartItemId, newQty);
        Refresh();
        _shell.RefreshCartCount();
    }

    private void ExecuteClearCart()
    {
        if (Cart == null) return;
        var r = MessageBox.Show("Xoá toàn bộ giỏ hàng?", "Xác nhận",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;
        _cartService.ClearCart(Cart.CartId);
        Refresh();
        _shell.RefreshCartCount();
    }

    private void ExecuteCheckout()
    {
        if (Cart == null || Cart.Items.Count == 0) return;

        IsCheckingOut = true;
        try
        {
            var customerId = Infrastructure.AppSession.CurrentUser!.UserId;
            // Always pass "Cash" or "Trực tiếp" since it's simplified.
            var result = _cartService.Checkout(customerId, "Cash", Note?.Trim());

            if (!result.Success)
            {
                MessageBox.Show(result.Error, "Lỗi thanh toán", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Thanh toán thành công!\nMã đơn: {result.OrderCode}",
                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

            Note = string.Empty;
            _shell.RefreshCartCount();
            Refresh();
        }
        finally
        {
            IsCheckingOut = false;
        }
    }
}
