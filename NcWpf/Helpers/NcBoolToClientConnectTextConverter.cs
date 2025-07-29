namespace NcWpf.Helpers;

/// <summary> Convert client connection flag to text </summary>
public sealed class NcBoolToClientConnectTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
        value is bool connected && connected ? "Client connected" : "Connect client";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
