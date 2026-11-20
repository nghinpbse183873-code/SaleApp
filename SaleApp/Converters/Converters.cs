using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SaleApp.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public Visibility TrueValue  { get; set; } = Visibility.Visible;
    public Visibility FalseValue { get; set; } = Visibility.Collapsed;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? TrueValue : FalseValue;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == TrueValue;
}

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}

[ValueConversion(typeof(decimal), typeof(string))]
public class PriceFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is decimal d ? d.ToString("N0") + " ₫" : "0 ₫";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => decimal.TryParse(value?.ToString()?.Replace("₫","").Replace(",","").Trim(),
            out var result) ? result : 0m;
}

/// <summary>Maps OrderStatus int to a color brush.</summary>
[ValueConversion(typeof(int), typeof(Brush))]
public class OrderStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is int status ? status : -1) switch
        {
            0 => new SolidColorBrush(Color.FromRgb(250, 173, 20)),   // Pending – amber
            1 => new SolidColorBrush(Color.FromRgb(24, 144, 255)),   // Confirmed – blue
            2 => new SolidColorBrush(Color.FromRgb(114, 46, 209)),   // Preparing – purple
            3 => new SolidColorBrush(Color.FromRgb(82, 196, 26)),    // Completed – green
            4 => new SolidColorBrush(Color.FromRgb(255, 77, 79)),    // Cancelled – red
            _ => new SolidColorBrush(Color.FromRgb(140, 140, 140))
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Maps PaymentStatus int to a color brush.</summary>
[ValueConversion(typeof(int), typeof(Brush))]
public class PaymentStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is int status ? status : -1) switch
        {
            0 => new SolidColorBrush(Color.FromRgb(250, 173, 20)),   // Pending – amber
            1 => new SolidColorBrush(Color.FromRgb(82, 196, 26)),    // Paid – green
            2 => new SolidColorBrush(Color.FromRgb(255, 77, 79)),    // Refunded – red
            _ => new SolidColorBrush(Color.FromRgb(140, 140, 140))
        };
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>Shows a colored badge for stock level.</summary>
[ValueConversion(typeof(int), typeof(Brush))]
public class StockStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int qty = value is int i ? i : 0;
        if (qty <= 0)  return new SolidColorBrush(Color.FromRgb(255, 77, 79));
        if (qty <= 10) return new SolidColorBrush(Color.FromRgb(250, 173, 20));
        return new SolidColorBrush(Color.FromRgb(82, 196, 26));
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

[ValueConversion(typeof(int), typeof(Visibility))]
public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int i && i > 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
