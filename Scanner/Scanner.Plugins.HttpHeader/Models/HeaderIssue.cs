namespace Scanner.Plugins.HttpHeader.Models;

internal sealed record HeaderIssue(
    string Id,
    string Title,
    string Severity,
    string Description)
{
    public IDictionary<string, string> Metadata { get; } =
        new Dictionary<string, string>();
}