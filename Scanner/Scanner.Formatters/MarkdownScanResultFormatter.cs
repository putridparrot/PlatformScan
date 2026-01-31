using Scanner.Core;
using System.Text;

namespace Scanner.Formatters;

public sealed class MarkdownScanResultFormatter : IScanResultFormatter
{
    public string Name => "markdown";
    public string FileExtension => "md";

    public string Format(IEnumerable<ScanFinding> findings)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Scan Results");
        sb.AppendLine();

        var grouped = findings
            .GroupBy(f => f.Plugin ?? "unknown")
            .OrderBy(g => g.Key);

        foreach (var pluginGroup in grouped)
        {
            sb.AppendLine($"## {Escape(pluginGroup.Key)}");
            sb.AppendLine();

            if (!pluginGroup.Any())
            {
                sb.AppendLine("_No findings_");
                sb.AppendLine();
                continue;
            }

            sb.AppendLine("| Severity | Title | Target | Description |");
            sb.AppendLine("|----------|--------|---------|-------------|");

            foreach (var f in pluginGroup)
            {
                sb.AppendLine(
                    $"| {Escape(f.Severity)} " +
                    $"| {Escape(f.Title)} " +
                    $"| {Escape(f.Target)} " +
                    $"| {Escape(f.Description)} |");
            }

            sb.AppendLine();

            // Metadata section (optional)
            foreach (var f in pluginGroup.Where(f => f.Metadata?.Any() == true))
            {
                sb.AppendLine($"### Metadata for {Escape(f.Id ?? f.Title ?? "item")}");
                sb.AppendLine();

                foreach (var kvp in f.Metadata!)
                {
                    sb.AppendLine($"- **{Escape(kvp.Key)}:** {Escape(kvp.Value)}");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return value
            .Replace("|", "\\|")
            .Replace("\n", " ")
            .Replace("\r", " ");
    }
}