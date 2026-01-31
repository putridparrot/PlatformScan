namespace Scanner.Core;

public interface IScannerPlugin
{
    string Name { get; }
    string Description { get; }
    Task<IReadOnlyCollection<ScanFinding>> ExecuteAsync(ScanContext context, CancellationToken cancellationToken);
}