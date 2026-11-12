using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PhotoRenamer.Core;

namespace PhotoRenamer.Desktop;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ILogger _logger;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartProcessingCommand))]
    private string? _workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

    [ObservableProperty]
    private bool _includeSubfolders;

    [ObservableProperty]
    private int _currentProgress;

    [ObservableProperty]
    private int _progressMaximum = 1;

    [ObservableProperty]
    private string _progressDescription = "Ready";

    public MainViewModel(ILogger logger)
    {
        _logger = logger;
    }

    public ObservableCollection<EventLogEntry> EventLog { get; } = new();

    private bool CanStartProcessing() => Directory.Exists(WorkingDirectory);

    [RelayCommand(CanExecute = nameof(CanStartProcessing))]
    private async Task StartProcessingAsync()
    {
        var directory = WorkingDirectory;
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        CurrentProgress = 0;
        ProgressMaximum = 1;
        ProgressDescription = "Counting files…";
        var processor = new RenameProcessor(_logger);
        var recursive = IncludeSubfolders;

        int totalFiles;
        try
        {
            totalFiles = await Task.Run(() => processor.CountFilesAsync(directory, recursive));
        }
        catch (Exception ex)
        {
            ProgressDescription = "Counting files failed";
            ReportError("Could not count files", ex);
            return;
        }

        ProgressMaximum = Math.Max(1, totalFiles);
        EventLog.Add(new($"Start processing {totalFiles} files"));
        ProgressDescription = $"0/{totalFiles}";

        try
        {
            await Task.Run(() => processor.ProcessRenameFilesAsync(directory, recursive, onRenameSuccess: (fileName, result) =>
                Dispatcher.UIThread.Invoke(() => RecordResult(fileName, result))));

            ProgressMaximum = Math.Max(1, CurrentProgress);
            ProgressDescription = $"Processed {CurrentProgress} files";
        }
        catch (Exception ex)
        {
            ProgressDescription = $"Processing stopped after {CurrentProgress} files";
            ReportError("Could not process files", ex);
        }
    }

    private void RecordResult(string fileName, OperationResult result)
    {
        var processed = CurrentProgress + 1;
        ProgressMaximum = Math.Max(ProgressMaximum, processed);
        CurrentProgress = processed;
        ProgressDescription = $"{processed}/{ProgressMaximum} - {fileName}";

        if (result.Success)
        {
            var message = (result as StringOperationResult)?.Result ?? fileName;
            EventLog.Add(new(message, EventLogEntryStatus.Success));
        }
        else
        {
            var error = result.FailureMessage ?? result.Exception?.Message ?? "Unknown error";
            EventLog.Add(new($"{fileName}: {error}", EventLogEntryStatus.Error));
        }
    }

    internal void ReportError(string message, Exception exception)
    {
        _logger.LogError(exception, "{Message}", message);
        EventLog.Add(new($"{message}: {exception.Message}", EventLogEntryStatus.Error));
    }
}
