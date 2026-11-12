using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;

namespace PhotoRenamer.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            desktop.Exit += (_, _) => loggerFactory.Dispose();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(loggerFactory.CreateLogger<MainViewModel>())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
