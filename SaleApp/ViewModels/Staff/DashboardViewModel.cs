using System.Collections.ObjectModel;
using System.Windows.Input;
using SaleApp.Models;
using SaleApp.MVVM;
using SaleApp.Repositories;

namespace SaleApp.ViewModels.Staff;

public class DashboardViewModel : ViewModelBase
{
    private readonly DashboardRepository _repo = new();

    private DashboardStats _stats = new();
    private bool _isLoading;

    public DashboardStats Stats      { get => _stats;     set => SetProperty(ref _stats,     value); }
    public bool           IsLoading  { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public ObservableCollection<TopProduct>    TopProducts  { get; } = [];
    public ObservableCollection<RevenueByDay>  RevenueData  { get; } = [];

    public ICommand RefreshCommand { get; }

    public string Greeting => $"Xin chào, {Infrastructure.AppSession.CurrentUser?.DisplayName}! 👋";
    public string TodayDate => DateTime.Now.ToString("dddd, dd/MM/yyyy");

    public DashboardViewModel()
    {
        RefreshCommand = new RelayCommand(LoadData);
        LoadData();
    }

    private void LoadData()
    {
        IsLoading = true;
        try
        {
            Stats = _repo.GetTodayStats();

            var revenue = _repo.GetRevenueByDay(7);
            RevenueData.Clear();
            foreach (var r in revenue) RevenueData.Add(r);

            var top = _repo.GetTopProducts(5);
            TopProducts.Clear();
            foreach (var p in top) TopProducts.Add(p);
        }
        catch { /* Silently handle */ }
        finally { IsLoading = false; }
    }
}
