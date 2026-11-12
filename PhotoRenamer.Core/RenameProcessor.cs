using System.Globalization;
using MetadataExtractor;
using MetadataExtractor.Formats.FileSystem;
using Microsoft.Extensions.Logging;
using Directory = System.IO.Directory;

namespace PhotoRenamer.Core;

public class RenameProcessor
{
    private const string CameraDateFormat = "yyyyMMdd_HHmmss";
    private const string RenamedDateFormat = "yyyy-MM-dd HH-mm-ss";
    private const string ExifDateFormat = "yyyy:MM:dd HH:mm:ss";
    private const string MetadataDateTimeOffsetFormat = "ddd MMM dd HH:mm:ss zzz yyyy";

    private static readonly DateTime MinDate = new(2006, 1, 1, 1, 1, 1, DateTimeKind.Local);

    private readonly ILogger _logger;

    public RenameProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public Task<int> CountFilesAsync(string path, bool recursive = false) =>
        Task.FromResult(GetFiles(path, recursive).Length);

    public Task<List<OperationResult>> ProcessRenameFilesAsync(
        string rootPath, bool recursive = false, Action<string, OperationResult>? onRenameSuccess = null)
    {
        try
        {
            var files = GetFiles(rootPath, recursive);
            var results = new List<OperationResult>(files.Length);
            foreach (var filePath in files)
            {
                var result = RenameFile(filePath);
                results.Add(result);
                onRenameSuccess?.Invoke(Path.GetFileName(filePath), result);
            }
            return Task.FromResult(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when processing files in {Path}", rootPath);
            throw;
        }
    }

    public async Task RenameAllAsync(string rootPath)
    {
        await ProcessRenameFilesAsync(rootPath);
    }

    public OperationResult RenameFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return OperationResult.FailureResult("File path is empty");
        }

        try
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                return OperationResult.ExceptionResult(new FileNotFoundException($"File {file.Name} not found", file.FullName));
            }
            if ((file.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                return OperationResult.FailureResult($"File {file.Name} is a link");
            }
            if (file.IsReadOnly)
            {
                return OperationResult.FailureResult($"File {file.Name} is read only");
            }

            var stem = Path.GetFileNameWithoutExtension(file.Name);
            if (HasRenamedDate(stem))
            {
                return StringOperationResult.SuccessResult($"File {file.Name} already has desired format");
            }

            var date = GetDateFromName(stem) ?? GetDateFromMetadata(file);
            if (date is null)
            {
                return OperationResult.FailureResult($"Could not extract date and time from {file.Name}");
            }

            var newPath = MoveToAvailablePath(file, date.Value);
            var newName = Path.GetFileName(newPath);
            try
            {
                File.SetCreationTime(newPath, date.Value);
                File.SetLastWriteTime(newPath, date.Value);
                File.SetLastAccessTime(newPath, date.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not update timestamps for {Path}", newPath);
                return OperationResult.FailureResult($"Renamed to {newName}, but could not update timestamps: {ex.Message}");
            }

            _logger.LogInformation("File renamed from {OldName} to {NewName}", file.Name, newName);
            return StringOperationResult.SuccessResult($"File renamed from {file.Name} to {newName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when renaming {Path}", filePath);
            return OperationResult.ExceptionResult(ex);
        }
    }

    private static string[] GetFiles(string path, bool recursive) =>
        Directory.GetFiles(path, "*", new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            AttributesToSkip = FileAttributes.ReparsePoint,
            IgnoreInaccessible = false
        });

    private static bool HasRenamedDate(ReadOnlySpan<char> stem)
    {
        var dateLength = RenamedDateFormat.Length;
        if (stem.Length < dateLength ||
            !DateTime.TryParseExact(stem[..dateLength], RenamedDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return false;
        }

        var suffix = stem[dateLength..];
        return suffix.IsEmpty || (suffix.Length >= 5 && suffix[0] == '_' && suffix[1..].IndexOfAnyExceptInRange('0', '9') < 0);
    }

    private static DateTime? GetDateFromName(ReadOnlySpan<char> stem)
    {
        if (stem.StartsWith(".trashed-", StringComparison.OrdinalIgnoreCase))
        {
            var pixelPrefix = stem.IndexOf("pxl_", StringComparison.OrdinalIgnoreCase);
            if (pixelPrefix < 0)
            {
                return null;
            }
            stem = stem[pixelPrefix..];
        }

        if (stem.StartsWith("img_", StringComparison.OrdinalIgnoreCase) ||
            stem.StartsWith("vid_", StringComparison.OrdinalIgnoreCase) ||
            stem.StartsWith("pxl_", StringComparison.OrdinalIgnoreCase))
        {
            stem = stem[4..];
        }
        else if (stem.StartsWith("wp_", StringComparison.OrdinalIgnoreCase))
        {
            stem = stem[3..];
        }

        return stem.Length >= CameraDateFormat.Length &&
            DateTime.TryParseExact(stem[..CameraDateFormat.Length], CameraDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static DateTime? GetDateFromMetadata(FileInfo file)
    {
        IReadOnlyList<MetadataExtractor.Directory> metadata;
        try
        {
            metadata = ImageMetadataReader.ReadMetadata(file.FullName);
        }
        catch (ImageProcessingException)
        {
            return null;
        }

        DateTime? earliestDate = null;
        foreach (var directory in metadata)
        {
            if (directory is FileMetadataDirectory)
            {
                continue;
            }

            foreach (var tag in directory.Tags)
            {
                if (!tag.Name.Contains("date", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var date = directory.GetObject(tag.Type) switch
                {
                    DateTime { Kind: DateTimeKind.Utc } value => value.ToLocalTime(),
                    DateTime value => value,
                    DateTimeOffset value => value.LocalDateTime,
                    { } value => ParseMetadataDate(value.ToString()),
                    null => null
                };
                if (date is { } candidate && IsValidMetadataDate(candidate) &&
                    (earliestDate is null || candidate < earliestDate.Value))
                {
                    earliestDate = candidate;
                }
            }
        }

        var modified = file.LastWriteTime;
        return earliestDate ?? (IsValidMetadataDate(modified) ? modified : null);
    }

    private static DateTime? ParseMetadataDate(string? text)
    {
        if (text is null || text.Length < ExifDateFormat.Length)
        {
            return null;
        }
        if (DateTimeOffset.TryParseExact(text, MetadataDateTimeOffsetFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var offsetDate))
        {
            return offsetDate.LocalDateTime;
        }
        if (DateTime.TryParseExact(text, ExifDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date) ||
            DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date))
        {
            return date;
        }
        return null;
    }

    private static bool IsValidMetadataDate(DateTime date) =>
        date > MinDate && !(date.Month == 1 && date.Day == 1 && date.TimeOfDay == TimeSpan.Zero);

    private static string MoveToAvailablePath(FileInfo file, DateTime date)
    {
        var stem = date.ToString(RenamedDateFormat, CultureInfo.InvariantCulture);
        var extension = file.Extension.ToLowerInvariant();
        for (var suffix = 0; ; suffix++)
        {
            var name = suffix == 0 ? stem + extension : FormattableString.Invariant($"{stem}_{suffix:D4}{extension}");
            var candidate = Path.Combine(file.DirectoryName!, name);
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                continue;
            }

            try
            {
                File.Move(file.FullName, candidate);
                return candidate;
            }
            catch (IOException) when (File.Exists(candidate) || Directory.Exists(candidate))
            {
                // Another operation claimed this name between the existence check and the move.
            }
        }
    }
}
