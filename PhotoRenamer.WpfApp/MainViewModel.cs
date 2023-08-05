using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PhotoRenamer.WpfApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PhotoRenamer.WpfApp;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ILogger _logger;

    public MainViewModel()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        _logger = loggerFactory.CreateLogger<MainViewModel>();

        _logger.LogInformation("Start init MainViewModel");

        // Commands 
        ShowFolderBrowserDialogCommand = new RelayCommand(ShowFolderBrowserDialog);
        StartProcessingCommand = new AsyncRelayCommand(StartProcessing);

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

        EventLog.Add(new EventLogEntry("Gello"));
    }

    private string? _workingDirectory = string.Empty;
    private int _currentProgress;
    private string? _currentProgressDescription;
    private ObservableCollection<EventLogEntry> _eventLog = new();

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

    private async Task StartProcessing()
    {
        _logger.LogDebug("StartProcessing command");

        EventLog.Add(new EventLogEntry("Start", EventLogEntryStatus.Success));

        CurrentProgress += 10;
    }

    #endregion

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
