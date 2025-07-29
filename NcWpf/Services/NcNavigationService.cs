namespace NcWpf.Services;

/// <summary> Navigation service for WPF applications </summary>
public sealed class NcNavigationService : INcNavigationService
{
    private readonly Frame _frame;

    public NcNavigationService(Frame frame)
    {
        _frame = frame;
    }

    public void NavigateTo(Page page) => _frame.Navigate(page);
}