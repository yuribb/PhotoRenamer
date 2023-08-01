
using Microsoft.Extensions.Logging;
using PhotoRenamer.Core;

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var path = string.Empty;

// Console.WriteLine("Path: ");
// path = Console.ReadLine();
if (path != string.Empty)
{
    // skip
}
else if (args.Any())
{
    path = args[0];
}
else
{
    path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        ?? "/Users/iuriibanshchikov/Pictures/";
}

if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
{
    logger.LogError("Incorrect path \"{Path}\"", path);
    Exit();
}

logger.LogInformation("Start processing in directory \"{Path}\"", path);
var processor = new RenameProcessor(loggerFactory.CreateLogger<RenameProcessor>());
await processor.RenameAllAsync(path);
Exit();

void Exit()
{
    logger!.LogInformation("Done");
    Console.ReadLine();
}