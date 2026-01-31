using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Scanner.Core;

public sealed class ScanContext
{
    public IConfiguration? Configuration { get; init; }
    public ILogger? Logger { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public IServiceProvider? Services { get; init; }
}