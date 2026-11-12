namespace PhotoRenamer.Desktop;

public sealed record EventLogEntry(string Message, EventLogEntryStatus Status = EventLogEntryStatus.Default)
{
    public bool IsSuccess => Status == EventLogEntryStatus.Success;
    public bool IsError => Status == EventLogEntryStatus.Error;
}

public enum EventLogEntryStatus
{
    Default,
    Success,
    Error
}
