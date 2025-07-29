namespace NcWpf.Views;

/// <summary> Main page of the application </summary>
public sealed partial class NcMainPage : Page
{
    public NcMainViewModel ViewModel { get; }

    public NcMainPage()
    {
        InitializeComponent();
        DataContext = ViewModel = new NcMainViewModel();
    }
}
