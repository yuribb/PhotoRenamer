using Microsoft.Extensions.Logging;
using PhotoRenamer.Core;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

if (args.Length > 1)
{
    logger.LogError("Usage: PhotoRenamer.Console [folder]");
    return 1;
}

var path = args.Length == 1 ? args[0] : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
if (!Directory.Exists(path))
{
    logger.LogError("Invalid folder: {Path}", path);
    return 1;
}

logger.LogInformation("Processing files in {Path}", path);
var processor = new RenameProcessor(loggerFactory.CreateLogger<RenameProcessor>());

try
{
    var results = await processor.ProcessRenameFilesAsync(path, onRenameSuccess: (fileName, result) =>
    {
        if (!result.Success)
        {
            logger.LogError("{FileName}: {Error}", fileName, result.FailureMessage ?? result.Exception?.Message);
        }
    });

    return results.All(result => result.Success) ? 0 : 1;
}
catch (Exception)
{
    // The processor logs enumeration errors.
    return 1;
}
