namespace Scanner.Core;

public sealed class ScanFinding
{
    public string? Plugin { get; init; }
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Severity { get; init; }
    public string? Target { get; init; }
    public string? Description { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}