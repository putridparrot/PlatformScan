using Microsoft.Extensions.Options;
using Scanner.Core;
using Scanner.Plugins.Tls.Options;

namespace Scanner.Plugins.Tls;

public sealed class TlsScannerPlugin(IOptions<TlsScannerOptions> options) : IScannerPlugin
{
    private readonly TlsScannerOptions _options = options.Value;

    public string Name => "tls";
    public string Description => "Scans targets for TLS configuration issues.";

    public async Task<IReadOnlyCollection<ScanFinding>> ExecuteAsync(
        ScanContext context,
        CancellationToken ct)
    {
        var findings = new List<ScanFinding>();

        foreach (var target in _options.Targets)
        {
            var result = await TlsScan.CheckAsync(target, ct);

            if (result.HasIssues)
            {
                findings.Add(new ScanFinding
                {
                    Plugin = Name,
                    Id = $"tls-{target}",
                    Title = $"TLS issues detected for {target}",
                    Severity = result.Severity,
                    Target = target,
                    Description = result.Description,
                    Metadata = result.Metadata
                });
            }
        }

        return findings;
    }
}