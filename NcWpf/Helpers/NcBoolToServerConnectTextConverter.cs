namespace NcWpf.Helpers;

/// <summary> Server connection flag converter to text </summary>
public sealed class NcBoolToServerConnectTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        value is bool connected && connected ? "Server created" : "Create server";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
