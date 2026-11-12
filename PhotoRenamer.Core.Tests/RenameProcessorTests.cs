using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;

namespace PhotoRenamer.Core.Tests;

public class RenameProcessorTests : IDisposable
{
    private const string CameraFileName = "IMG_20230528_174703.jpg";
    private const string RenamedFileName = "2023-05-28 17-47-03.jpg";

    private readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("PhotoRenamer-");
    private readonly RenameProcessor _processor = new(NullLogger.Instance);

    [Fact]
    public async Task RenamesCameraFileAndUpdatesTimestamps()
    {
        var original = CopySample(CameraFileName);

        await _processor.RenameAllAsync(_folder.FullName);

        var renamed = Path.Combine(_folder.FullName, RenamedFileName);
        Assert.False(File.Exists(original));
        Assert.True(File.Exists(renamed));
        Assert.Equal(new DateTime(2023, 5, 28, 17, 47, 3), File.GetLastWriteTime(renamed));
    }

    [Fact]
    public async Task KeepsCopiesAndResolvesNameCollisionsInTheirOwnFolder()
    {
        var subfolder = _folder.CreateSubdirectory("IMG_20230528_174703 - Copy.jpg");
        var original = CopySample(CameraFileName, subfolder.FullName, subfolder.Name);
        var existing = CopySample(CameraFileName, subfolder.FullName, RenamedFileName);

        await _processor.ProcessRenameFilesAsync(_folder.FullName, recursive: true);

        Assert.False(File.Exists(original));
        Assert.True(File.Exists(existing));
        var renamedCopy = Path.Combine(subfolder.FullName, "2023-05-28 17-47-03_0001.jpg");
        Assert.True(File.Exists(renamedCopy));

        await _processor.ProcessRenameFilesAsync(_folder.FullName, recursive: true);
        Assert.True(File.Exists(renamedCopy));
        Assert.Equal(2, subfolder.GetFiles().Length);
    }

    [Fact]
    public void UsesModificationTimeForImageWithoutAnEmbeddedDate()
    {
        var original = CopySample("IMG_for_metadata.jpg");
        File.SetLastWriteTime(original, new DateTime(2023, 8, 1, 3, 11, 32));

        var result = _processor.RenameFile(original);

        Assert.True(result.Success, result.FailureMessage);
        Assert.False(File.Exists(original));
        Assert.True(File.Exists(Path.Combine(_folder.FullName, "2023-08-01 03-11-32.jpg")));
    }

    [Fact]
    public void PrefersEmbeddedMovieDateToEarlierFileModificationTime()
    {
        var original = CopySample("IMG_2610.mov");
        File.SetLastWriteTime(original, new DateTime(2010, 5, 15, 12, 0, 0));
        var creationTime = new DateTime(2023, 5, 15, 16, 37, 45, DateTimeKind.Utc).ToLocalTime();
        var expected = creationTime.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture) + ".mov";

        var result = _processor.RenameFile(original);

        Assert.True(result.Success, result.FailureMessage);
        Assert.False(File.Exists(original));
        Assert.True(File.Exists(Path.Combine(_folder.FullName, expected)));
    }

    [Fact]
    public void ParsesEmbeddedExifDateInNonGregorianCulture()
    {
        var original = Path.Combine(_folder.FullName, "photo.tiff");
        using (var writer = new BinaryWriter(File.Create(original)))
        {
            // Little-endian TIFF with one directory entry holding the EXIF date.
            writer.Write("II"u8);
            writer.Write((ushort)42);
            writer.Write(8u);
            writer.Write((ushort)1);
            writer.Write((ushort)306);
            writer.Write((ushort)2);
            writer.Write(20u);
            writer.Write(26u);
            writer.Write(0u);
            writer.Write("2023:08:01 03:11:32\0"u8);
        }

        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var result = _processor.RenameFile(original);
            Assert.True(result.Success, result.FailureMessage);
            Assert.True(File.Exists(Path.Combine(_folder.FullName, "2023-08-01 03-11-32.tiff")));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    private string CopySample(string sampleName, string? folder = null, string? fileName = null)
    {
        var source = Path.Combine(AppContext.BaseDirectory, "Content", sampleName);
        var destination = Path.Combine(folder ?? _folder.FullName, fileName ?? sampleName);
        File.Copy(source, destination);
        return destination;
    }

    public void Dispose() => _folder.Delete(recursive: true);
}
