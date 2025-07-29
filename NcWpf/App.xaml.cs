namespace NcWpf;

public partial class App : Application
{
    public static IContainer Container { get; private set; } = default!;

    public App()
    {
        // DI registration
        var builder = new ContainerBuilder();
        builder.RegisterType<NcNavigationService>().As<INcNavigationService>().SingleInstance();
        Container = builder.Build();
    }
}