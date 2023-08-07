using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PhotoRenamer.Core;
using PhotoRenamer.WpfApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PhotoRenamer.WpfApp;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ILogger _logger;

    readonly Dispatcher _dispatcher;

    public MainViewModel()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;

        using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddDebug());
        _logger = loggerFactory.CreateLogger<MainViewModel>();

        _logger.LogInformation("Start init MainViewModel");

        // Commands 
        ShowFolderBrowserDialogCommand = new RelayCommand(ShowFolderBrowserDialog);
        StartProcessingCommand = new AsyncRelayCommand(StartProcessingAsync);

        // Fields
        if (WorkingDirectory != string.Empty)
        {
            // skip
        }
        else
        {
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                ?? "/Users/iuriibanshchikov/Pictures/";
        }
    }

    private string? _workingDirectory = string.Empty;
    private int _currentProgress;
    private string? _currentProgressDescription;
    private ObservableCollection<EventLogEntry> _eventLog = new();
    private int _totalFilesCount;

    public string? WorkingDirectory
    {
        get => _workingDirectory;
        set
        {
            _workingDirectory = value;
            OnPropertyChanged(nameof(WorkingDirectory));
            OnPropertyChanged(nameof(IsWorkingDirectoryReady));
            _logger.LogDebug("WorkingDirectory={WorkingDirectory}", value);
        }
    }

    public bool IsWorkingDirectoryReady
    {
        get => !string.IsNullOrWhiteSpace(WorkingDirectory) && Directory.Exists(WorkingDirectory);
    }

    public int CurrentProgress
    {
        get => _currentProgress;
        set
        {
            _currentProgress = value;
            OnPropertyChanged(nameof(CurrentProgress));
            _logger.LogDebug("CurrentStatus={CurrentStatus}", value);
        }
    }

    public int TotalFilesCount
    {
        get => _totalFilesCount;
        set
        {
            _totalFilesCount = value;
            OnPropertyChanged(nameof(TotalFilesCount));
            _logger.LogDebug("TotalFilesCount={TotalFilesCount}", value);
        }
    }

    public string? CurrentProgressDescription
    {
        get => _currentProgressDescription;
        set
        {
            _currentProgressDescription = value;
            OnPropertyChanged(nameof(CurrentProgressDescription));
            _logger.LogDebug("CurrentStatusDescription={CurrentStatusDescription}", value);
        }
    }

    public ObservableCollection<EventLogEntry> EventLog
    {
        get => _eventLog;
        set
        {
            _eventLog = value;
            OnPropertyChanged(nameof(EventLog));
            _logger.LogDebug("EventLog count={EventLogCount}", _eventLog.Count);
        }
    }

    #region Commands
    public IRelayCommand ShowFolderBrowserDialogCommand { get; }

    public IAsyncRelayCommand StartProcessingCommand { get; }

    private void ShowFolderBrowserDialog()
    {
        _logger.LogDebug("ShowFolderBrowserDialog command");

        var folderBrowserDialog = new VistaFolderBrowserDialog
        {
            ShowNewFolderButton = false,
            Description = "Please select a folder.",
            UseDescriptionForTitle = true
        };
        if (Directory.Exists(WorkingDirectory))
        {
            folderBrowserDialog.SelectedPath = WorkingDirectory;
        }

        var dialogResult = folderBrowserDialog.ShowDialog();
        if (dialogResult == true)
        {
            WorkingDirectory = folderBrowserDialog.SelectedPath;
        }
    }

    private async Task StartProcessingAsync()
    {
        _logger.LogDebug("StartProcessing command");

        if (string.IsNullOrWhiteSpace(WorkingDirectory))
        {
            _logger.LogError("WorkingDirectory is null");
            return;
        }

        var photoRenamer = new RenameProcessor(_logger);
        try
        {
            TotalFilesCount = await photoRenamer.CountFilesAsync(WorkingDirectory, recursive: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when counting files");
            EventLog.Add(new EventLogEntry("Error when counting files", EventLogEntryStatus.Error));
            return;
        }

        EventLog.Add(new EventLogEntry($"Start processing {TotalFilesCount} files"));

        await Task.Run(() => Process(photoRenamer, WorkingDirectory));

        _logger.LogDebug("Finish StartProcessing command");
    }

    private async Task Process(RenameProcessor photoRenamer, string directory)
    {
        try
        {
            int i = 0;
            await photoRenamer.ProcessRenameFilesAsync(directory, recursive: false, onRenameSuccess: async (fileName, result) =>
                   {
                       _dispatcher.Invoke(() =>
               {
                   CurrentProgress++;
                   CurrentProgressDescription = $"{++i}/{TotalFilesCount} - {fileName}";
                   if (result.Success)
                   {
                       if (result is StringOperationResult sResult)
                           EventLog.Add(new EventLogEntry(sResult.Result!, EventLogEntryStatus.Success));
                       else
                           EventLog.Add(new EventLogEntry(fileName, EventLogEntryStatus.Success));
                   }
                   else
                   {
                       EventLog.Add(new EventLogEntry(result.FailureMessage ?? result.Exception?.Message ?? "???", EventLogEntryStatus.Error));
                   }
               });
                   });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when processing files");
            EventLog.Add(new EventLogEntry("Error when processing files", EventLogEntryStatus.Error));
        }
    }

    #endregion

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
