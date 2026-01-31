using Scanner.Core;
using System.Text;

namespace Scanner.Formatters;

public sealed class ConsoleScanResultFormatter : IScanResultFormatter
{
    public string Name => "console";
    public string FileExtension => ""; // not used

    public string Format(IEnumerable<ScanFinding> findings)
    {
        var sb = new StringBuilder();

        foreach (var f in findings)
        {
            sb.AppendLine($"{f.Severity}: {f.Title} ({f.Target})");
        }

        return sb.ToString();
    }
}
