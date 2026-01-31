namespace Scanner.Core;

public interface IScanResultFormatter
{
    string Name { get; }              // e.g., "junit"
    string FileExtension { get; }     // e.g., "xml"
    string Format(IEnumerable<ScanFinding> findings);
}