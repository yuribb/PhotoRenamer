using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace PhotoRenamer.Core.Tests;

public class RenameProcessorTests
{
    private const string OriginalImgFileName = "IMG_20230528_174703.jpg";
    private const string OriginalMetadataFileName = "IMG_for_metadata.jpg";
    private const string OriginalMovFileName = "IMG_2610.mov";
    private const string DesiredImgFileName = "2023-05-28 17-47-03.jpg";
    private const string DesiredMetadataFileName = "2023-08-01 03-11-32.jpg";
    private const string DesiredMovFileName = "2023-05-15 20-37-45.mov";

    /// <summary>
    /// Папка с оригинальным изображением, которая студия копирует при билде.
    /// </summary>
    private static readonly string OriginalImgPath = Path.Combine(Directory.GetCurrentDirectory(), "Content");
    private static readonly DateTime DesiredImgDateTime = new(2023, 05, 28, 17, 47, 03, DateTimeKind.Local);

    private readonly ILogger _logger;

    public RenameProcessorTests(ITestOutputHelper testOutput)
    {
        var loggerFactory = new LoggerFactory().AddXUnit(testOutput);
        _logger = loggerFactory.CreateLogger<RenameProcessor>();
    }

    [Fact]
    public async Task RenameImgByNameSuccess()
    {
        var (fileDir, filePath) = CopyImage(OriginalImgFileName);

        var processor = new RenameProcessor(_logger);
        await processor.RenameAllAsync(fileDir);

        Assert.False(File.Exists(filePath));
        var newFilePath = Path.Combine(fileDir, DesiredImgFileName);
        Assert.True(File.Exists(newFilePath));

        var createData = File.GetCreationTime(newFilePath);
        Assert.Equal(DesiredImgDateTime, createData);

        Cleanup(fileDir);
    }

    [Fact]
    public async Task RenameImgByNameDuplicateSuccess()
    {
        var (fileDir, filePath) = CopyImage(OriginalImgFileName);

        var processor = new RenameProcessor(_logger);
        await processor.RenameAllAsync(fileDir);

        Assert.False(File.Exists(filePath));
        var newFilePath = Path.Combine(fileDir, DesiredImgFileName);
        Assert.True(File.Exists(newFilePath));

        // Create original file again
        File.Copy(newFilePath, filePath);
        await processor.RenameAllAsync(fileDir);

        // assert processed file is alive
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(newFilePath));

        // assert conflicting file is created
        var newFilePath2 = Path.Combine(fileDir, DesiredImgFileName.Replace(".jpg", "_0001.jpg"));
        Assert.True(File.Exists(newFilePath2));

        Directory.Delete(fileDir, true);
    }

    [Fact]
    public async Task RenameImgByMetadataSuccess()
    {
        var (fileDir, filePath) = CopyImage(OriginalMetadataFileName);

        var processor = new RenameProcessor(_logger);
        await processor.RenameAllAsync(fileDir);

        Assert.False(File.Exists(filePath));
        var newFilePath = Path.Combine(fileDir, DesiredMetadataFileName);
        Assert.True(File.Exists(newFilePath));

        Cleanup(fileDir);
    }

    [Fact]
    public async Task RenameMovByMetadataSuccess()
    {
        var (fileDir, filePath) = CopyImage(OriginalMovFileName);

        var processor = new RenameProcessor(_logger);
        await processor.RenameAllAsync(fileDir);

        Assert.False(File.Exists(filePath));
        var newFilePath = Path.Combine(fileDir, DesiredMovFileName);
        Assert.True(File.Exists(newFilePath));

        Cleanup(fileDir);
    }

    /// <summary>
    /// Получить копию папки с оригинальным изображением.
    /// Так как тесты конфликтуют между собой, для каждого теста создаём свою подпапку.
    /// </summary>
    /// <returns>(DirectoryPath, FileName)</returns>
    private (string, string) CopyImage(string imageName)
    {
        var originalFilePath = Path.Combine(OriginalImgPath, imageName);
        Assert.True(File.Exists(originalFilePath));

        var tempDirectory = Directory.CreateTempSubdirectory().FullName;

        var newFilePath = Path.Combine(tempDirectory, imageName);
        File.Copy(originalFilePath, newFilePath);

        _logger.LogInformation("Create temp directory {Dir}", tempDirectory);

        return (tempDirectory, newFilePath);
    }

    private void Cleanup(string directory)
    {
        _logger.LogInformation("Delete temp directory {Dir}", directory);
        Directory.Delete(directory, true);
    }
}