namespace Scanner.Plugins.Tls.Models;

internal sealed class TlsScanResult
{
    public bool HasIssues { get; init; }
    public string? Severity { get; init; }
    public string? Description { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}