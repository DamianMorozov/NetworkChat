namespace NcWpf;

public partial class NcMainWindow : Window
{
    public NcMainWindow()
    {
        InitializeComponent();

        // DI using Autofac
        var scope = App.Container.BeginLifetimeScope();
        var navigationService = scope.Resolve<INcNavigationService>(new TypedParameter(typeof(Frame), MainFrame));
        navigationService.NavigateTo(new NcMainPage());
    }
}