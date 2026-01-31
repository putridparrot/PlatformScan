using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scanner.Core;

namespace Scanner.Cli;

public sealed class ScanRunner(IEnumerable<IScannerPlugin> plugins, ILogger<ScanRunner> logger,
    IConfiguration config, IServiceProvider services)
{
    public async Task<IReadOnlyCollection<ScanFinding>> RunAsync(
        IEnumerable<string> pluginNames,
        CancellationToken ct)
    {
        var selected = plugins
            .Where(p => !pluginNames.Any() || pluginNames.Contains(p.Name))
            .ToList();

        var context = new ScanContext
        {
            Configuration = config,
            Logger = logger,
            Timestamp = DateTimeOffset.UtcNow,
            Services = services
        };

        var findings = new List<ScanFinding>();

        foreach (var plugin in selected)
        {
            logger.LogInformation("Running plugin {Plugin}", plugin.Name);
            var result = await plugin.ExecuteAsync(context, ct);
            findings.AddRange(result);
        }

        return findings;
    }
}