
using PhotoRenamer.Core;

var path = string.Empty;

while (path == string.Empty)
{
    // Console.WriteLine("Path: ");
    // path = Console.ReadLine();
    path = "/Users/iuriibanshchikov/Pictures/";

    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
    {
        Console.WriteLine($"Incorrect path \"{path}\"");
        continue;
    }

    var processor = new RenameProcessor();
    await processor.RenameAllAsync(path);
    break;
}
Console.WriteLine("Done");
Console.ReadLine();
