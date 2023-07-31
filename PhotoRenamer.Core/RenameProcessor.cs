using GroupDocs.Metadata;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;
using Directory = System.IO.Directory;

namespace PhotoRenamer.Core;

public class RenameProcessor
{
    private static readonly DateTime MinDate = new(2006, 1, 1, 1, 1, 1, DateTimeKind.Local);

    private readonly ILogger _logger;

    public RenameProcessor(ILogger logger)
    {
        _logger = logger;
    }

    private Task RenameFilesAsync(string path)
    {
        try
        {
            // var subdirectories = Directory.GetDirectories(path);

            // foreach (var subDirectory in subdirectories)
            // {
            //     await RenameFilesAsync(subDirectory);
            // }

            var filePaths = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);

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

    public async Task RenameAllAsync(string rootPath)
    {
        await RenameFilesAsync(rootPath);
    }

    private void RenameFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var fi = new FileInfo(filePath);
        if (!fi.Exists)
        {
            _logger.LogWarning("File {FileName} is not exist", fi.Name);
            return;
        }
        if (fi.IsReadOnly)
        {
            _logger.LogWarning("File {FileName} is read only", fi.Name);
            return;
        }

        if (fi.Name.Replace(fi.Extension, string.Empty).Contains("Copy"))
        {
            File.Delete(filePath);
            return;
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
            else
                return;
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
            return;
        }
        var newPath = GiveNewPath(fi, dateTime.Value);
        File.Move(filePath, newPath);
        ChangeFileDate(newPath, dateTime.Value);

        var newName = Path.GetFileName(newPath);
        _logger.LogInformation("File renamed from {OldName} to {NewName}", fi.Name, newName);
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

        foreach (var meta in metadata)
        {
            foreach (var tag in meta.Tags)
            {
                if (tag.Description == null || !tag.Name.ToLower().Contains("date") || !tag.Name.Contains("Date") || tag.Description.Length < 19 || tag.Description.Contains("+")) continue;
                if (DateTime.TryParse(tag.Description, out var dateTime))
                {
                    if (dateTime.Month == 1 && dateTime.Day == 1 && dateTime.Hour == 0 && dateTime.Minute == 0 &&
                        dateTime.Second == 0 && dateTime.Millisecond == 0 || dates.Contains(dateTime))
                    {
                        continue;
                    }
                    dates.Add(dateTime);
                }
                else if (tag.Description.Contains(":"))
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
        var newName = $"{date.Year}-{date.Month:D2}-{date.Day:D2} {date.Hour:D2}-{date.Minute:D2}-{date.Second:D2}{fi.Extension.ToLower()}";

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