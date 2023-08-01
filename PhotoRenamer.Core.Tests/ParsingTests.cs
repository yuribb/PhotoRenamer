using Microsoft.Extensions.Logging;
using System.Globalization;
using Xunit.Abstractions;

namespace PhotoRenamer.Core.Tests;

public class ParsingTests
{
    private readonly ILogger _logger;

    public ParsingTests(ITestOutputHelper testOutput)
    {
        var loggerFactory = new LoggerFactory().AddXUnit(testOutput);
        _logger = loggerFactory.CreateLogger<RenameProcessor>();
    }

    [Fact]
    public void ParseDateTimeOffset()
    {
        var stringDate = "גע אגד. 01 03:11:32 +04:00 2023";
        var result = DateTimeOffset.TryParseExact(stringDate, "ddd MMM dd HH:mm:ss zzz yyyy", DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var dateTimeOffset);

        Assert.True(result);
        Assert.Equal(TimeSpan.FromHours(4), dateTimeOffset.Offset);
        Assert.Equal(3, dateTimeOffset.Hour);
    }

}