namespace PhotoRenamer.WpfApp.Models;

public class EventLogEntry
{
    public EventLogEntry()
    {
    }

    public EventLogEntry(string message, EventLogEntryStatus status)
    {
        Message = message;
        Status = status;
    }

    public EventLogEntry(string message)
        :this(message, EventLogEntryStatus.Default)
    {
    }

    public string Message { get; set; } = null!;

    public EventLogEntryStatus Status { get; set; }
}

public enum EventLogEntryStatus
{
    Default = 0,
    Success = 1,
    Error = 2,
}