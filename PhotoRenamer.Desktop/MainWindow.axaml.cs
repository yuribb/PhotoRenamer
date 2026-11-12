using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace PhotoRenamer.Desktop;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private async void BrowseFolder(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        try
        {
            var path = viewModel.WorkingDirectory;
            using var startLocation = Directory.Exists(path)
                ? await StorageProvider.TryGetFolderFromPathAsync(Path.GetFullPath(path!))
                : null;
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select a folder",
                SuggestedStartLocation = startLocation
            });

            using var folder = folders.FirstOrDefault();
            if (folder?.TryGetLocalPath() is { } selectedPath)
            {
                viewModel.WorkingDirectory = selectedPath;
            }
        }
        catch (Exception ex)
        {
            viewModel.ReportError("Could not open the folder picker", ex);
        }
    }
}
