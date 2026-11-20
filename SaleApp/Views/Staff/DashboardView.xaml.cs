using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SaleApp.Views.Staff;

public partial class DashboardView : UserControl
{
    public DashboardView() => InitializeComponent();
}

/// <summary>Converts revenue decimal to a bar height between 8 and 120 px.</summary>
public class RevenueToHeightConverter : IValueConverter
{
    public static readonly RevenueToHeightConverter Instance = new();

    // We use a simple scaling: max bar = 120px maps to some reference max.
    // Since we don't know max at bind time, we use a simple logarithmic scale.
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not decimal rev || rev <= 0) return 8.0;
        // Log scale: 1 -> 8, 1_000_000 -> 120
        double log = Math.Log10((double)rev + 1);
        double maxLog = Math.Log10(1_000_000 + 1);
        return Math.Max(8, Math.Min(120, log / maxLog * 120));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
