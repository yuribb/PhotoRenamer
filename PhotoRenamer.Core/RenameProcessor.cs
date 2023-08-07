using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO;
using Directory = System.IO.Directory;

namespace PhotoRenamer.Core;

public class RenameProcessor
{
    private const string MetadataDateTimeOffsetFormat = "ddd MMM dd HH:mm:ss zzz yyyy";

    private const string YandexDateTimeFormat = "yyyy-MM-dd HH-mm-ss";

    private static readonly DateTime MinDate = new(2006, 1, 1, 1, 1, 1, DateTimeKind.Local);

    private readonly ILogger _logger;

    public RenameProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public Task<int> CountFilesAsync(string path, bool recursive = false)
    {
        var files = Directory.GetFiles(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        return Task.FromResult(files.Length);
    }

    public Task<List<OperationResult>> ProcessRenameFilesAsync(string rootPath, bool recursive = false, Action<string, OperationResult>? onRenameSuccess = null)
    {
        try
        {
            var results = new List<OperationResult>();
            var filePaths = Directory.GetFiles(rootPath, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            for (var i = 1; i <= filePaths.Length; i++)
            {
                var filePath = filePaths[i - 1];
                var result = RenameFile(filePath);
                results.Add(result);
                var fileName = new FileInfo(filePath).Name;
                onRenameSuccess?.Invoke(fileName, result);
            }
            return Task.FromResult(results);
        }
        catch (Exception e)
        {
            //Console.WriteLine(e);
            _logger.LogError(e, "Error when renaming files on path {Path}", rootPath);
            throw;
        }
    }

    public async Task RenameAllAsync(string rootPath)
    {
        await RenameFilesAsync(rootPath, recursive: false);
    }

    private Task RenameFilesAsync(string path, bool recursive = false)
    {
        try
        {
            var filePaths = Directory.GetFiles(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            for (var i = 1; i <= filePaths.Length; i++)
            {
                var filePath = filePaths[i - 1];
                RenameFile(filePath);
            }
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            //Console.WriteLine(e);
            _logger.LogError(e, "Error when renaming files on path {Path}", path);
            throw;
        }
    }

    public OperationResult RenameFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return OperationResult.FailureResult("File path is empty");
        }
        try
        {
            var fi = new FileInfo(filePath);
            if (!fi.Exists)
            {
                _logger.LogWarning("File {FileName} is not exist", fi.Name);
                return OperationResult.ExceptionResult(new FileNotFoundException($"File {fi.Name} not found"));
            }
            if (fi.IsReadOnly)
            {
                _logger.LogWarning("File {FileName} is read only", fi.Name);
                return OperationResult.FailureResult($"File {fi.Name} is read only");
            }

            if (fi.Name.Replace(fi.Extension, string.Empty).Contains("Copy"))
            {
                File.Delete(filePath);
                return OperationResult.FailureResult($"File {fi.Name} is copy file");
            }

            var fileName = fi.Name.ToLower();
            DateTime? dateTime;

            if (fileName.StartsWith("200") || fileName.StartsWith("201") || fileName.StartsWith("202"))
            {
                if (fileName.Contains("_") && fileName.Length <= 26)
                {
                    var year = int.Parse(fi.Name.Substring(0, 4));
                    var month = int.Parse(fi.Name.Substring(4, 2));
                    var day = int.Parse(fi.Name.Substring(6, 2));
                    var hour = int.Parse(fi.Name.Substring(9, 2));
                    var minute = int.Parse(fi.Name.Substring(11, 2));
                    var second = int.Parse(fi.Name.Substring(13, 2));
                    dateTime = new DateTime(year, month, day, hour, minute, second);
                }
                else if (fileName.Contains("-") && fileName.Contains(" "))
                {
                    _logger.LogWarning("File {FileName} already has desired format", fi.Name);
                    return StringOperationResult.SuccessResult($"File {fi.Name} already has desired format");
                }
                else
                    return OperationResult.FailureResult($"Invalid format of file {fi.Name}");
            }
            else if ((fileName.StartsWith("img_20") || fileName.StartsWith("vid_20")) && fileName.Length > 17)
            {
                var year = int.Parse(fi.Name.Substring(4, 4));
                var month = int.Parse(fi.Name.Substring(8, 2));
                var day = int.Parse(fi.Name.Substring(10, 2));
                var hour = int.Parse(fi.Name.Substring(13, 2));
                var minute = int.Parse(fi.Name.Substring(15, 2));
                var second = int.Parse(fi.Name.Substring(17, 2));
                dateTime = new DateTime(year, month, day, hour, minute, second);
            }
            else if (fileName.StartsWith("wp_20") && fileName.Length > 20)
            {
                var year = int.Parse(fi.Name.Substring(3, 4));
                var month = int.Parse(fi.Name.Substring(7, 2));
                var day = int.Parse(fi.Name.Substring(9, 2));
                var hour = int.Parse(fi.Name.Substring(12, 2));
                var minute = int.Parse(fi.Name.Substring(15, 2));
                var second = int.Parse(fi.Name.Substring(18, 2));
                dateTime = new DateTime(year, month, day, hour, minute, second);
            }
            else
            {
                dateTime = GetMetadataTime(fi);
            }

            if (!dateTime.HasValue)
            {
                return OperationResult.FailureResult($"Could not extract date and time form file {fi.Name}");
            }
            var newPath = GiveNewPath(fi, dateTime.Value);
            File.Move(filePath, newPath);
            ChangeFileDate(newPath, dateTime.Value);

            var newName = Path.GetFileName(newPath);
            _logger.LogInformation("File renamed from {OldName} to {NewName}", fi.Name, newName);

            return StringOperationResult.SuccessResult($"File renamed from {fi.Name} to {newName}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when renaming file on path {Path}", filePath);
            return OperationResult.ExceptionResult(e);
        }
    }

    private DateTime? GetMetadataTime(FileSystemInfo fi)
    {
        var dateTime = GetExifDate(fi);
        // if (dateTime.HasValue)
        // {
        //     return dateTime;
        // }

        // dateTime = GetMsInfoDateTime(fi);
        //
        // if (dateTime.HasValue)
        // {
        //     return dateTime;
        // }

        // dateTime = GetDateTakenFromImage(fi.FullName);

        return dateTime;
    }

    // private DateTime? GetMovDateTime(FileSystemInfo fi)
    // {
    //     var vlc = new LibVLC();
    //
    //     Media m = new(vlc, new Uri(fi.FullName));
    //     
    //     // async method, using awaiter here for simplicity
    //     m.Parse().GetAwaiter().GetResult();
    //
    //     // video duration in ms
    //     Console.WriteLine(m.SubItems.);
    // }

    // private DateTime? GetMsInfoDateTime(FileSystemInfo fi)
    // {
    //     DateTime? dateTime = null;
    //     var mf = new MediaInfoDotNet.MediaFile(fi.FullName);
    //
    //     if (mf.Image.Any())
    //     {
    //         dateTime = mf.Image.First().EncodedDate;
    //     }
    //
    //     else if (mf.Video.Any())
    //     {
    //         dateTime = mf.Video.First().EncodedDate;
    //     }
    //     if (dateTime.HasValue && dateTime.Value > MinDate)
    //     {
    //         return dateTime;
    //     }
    //     return null;
    // }

    // public static DateTime? GetDateTakenFromImage(string path)
    // {
    //     using (var metadata = new Metadata(path))
    //     {
    //
    //     }
    //
    //     return null;
    // }

    private DateTime? GetExifDate(FileSystemInfo fi)
    {
        IReadOnlyList<MetadataExtractor.Directory> metadata;
        try
        {
            metadata = MetadataExtractor.ImageMetadataReader.ReadMetadata(fi.FullName);
        }
        catch
        {
            return null;
        }

        IList<DateTime> dates = new List<DateTime>();

        foreach (var tag in metadata.SelectMany(meta => meta.Tags).Where(tag => tag.Name.Contains("date", StringComparison.OrdinalIgnoreCase)))
        {
            if (tag.Description == null || tag.Description.Length < 19) continue;

            if (DateTimeOffset.TryParseExact(tag.Description, MetadataDateTimeOffsetFormat, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var dateTimeOffset))
            {
                if (dateTimeOffset.Month == 1 && dateTimeOffset.Day == 1 && dateTimeOffset.Hour == 0 && dateTimeOffset.Minute == 0 &&
                    dateTimeOffset.Second == 0 && dateTimeOffset.Millisecond == 0 || dates.Contains(dateTimeOffset.LocalDateTime))
                {
                    continue;
                }
                dates.Add(dateTimeOffset.LocalDateTime);
            }
            else if (DateTime.TryParse(tag.Description, out var dateTime))
            {
                if (dateTime.Month == 1 && dateTime.Day == 1 && dateTime.Hour == 0 && dateTime.Minute == 0 &&
                    dateTime.Second == 0 && dateTime.Millisecond == 0 || dates.Contains(dateTime))
                {
                    continue;
                }
                dates.Add(dateTime);
            }
            else if (tag.Description.Contains(':'))
            {
                try
                {
                    var year = int.Parse(tag.Description.Substring(0, 4));
                    var month = int.Parse(tag.Description.Substring(5, 2));
                    var day = int.Parse(tag.Description.Substring(8, 2));
                    var hour = int.Parse(tag.Description.Substring(11, 2));
                    var minute = int.Parse(tag.Description.Substring(14, 2));
                    var second = int.Parse(tag.Description.Substring(17, 2));
                    dateTime = new DateTime(year, month, day, hour, minute, second);
                    if (dateTime.Month == 1 && dateTime.Day == 1 && dateTime.Hour == 0 && dateTime.Minute == 0 &&
                        dateTime.Second == 0 && dateTime.Millisecond == 0 || dates.Contains(dateTime))
                    {
                        continue;
                    }
                    dates.Add(dateTime);
                }
                catch
                {
                    // ignored
                }
            }
        }

        dates = dates.ToList();

        if (!dates.Any())
        {
            return null;
        }

        var dateTimeVal = dates.Min();
        if (dateTimeVal <= MinDate)
        {
            return null;
        }
        return dateTimeVal;
    }

    private string GiveNewPath(FileSystemInfo fi, DateTime date)
    {
        var newName = $"{date.ToString(YandexDateTimeFormat, CultureInfo.InvariantCulture)}{fi.Extension.ToLower()}";

        var newPath = fi.FullName.Replace(fi.Name, newName);
        ChangeFileDate(fi.FullName, date);
        if (!File.Exists(newPath))
        {
            return newPath;
        }
        var tempPath = newPath;
        var i = 0;
        while (File.Exists(tempPath))
        {
            i++;
            tempPath = $"{newPath.Replace(fi.Extension.ToLower(), string.Empty)}_{i:D4}{fi.Extension.ToLower()}";
        }
        newPath = tempPath;
        return newPath;
    }

    private static void ChangeFileDate(string filePath, DateTime date)
    {
        if (!File.Exists(filePath))
        {
            return;
        }
        File.SetCreationTime(filePath, date);
        File.SetLastWriteTime(filePath, date);
        File.SetLastAccessTime(filePath, date);
    }
}