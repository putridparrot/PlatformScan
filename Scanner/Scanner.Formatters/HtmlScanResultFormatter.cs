using Scanner.Core;
using System.Text;

namespace Scanner.Formatters;

public sealed class HtmlScanResultFormatter : IScanResultFormatter
{
    public string Name => "html";
    public string FileExtension => "html";

    public string Format(IEnumerable<ScanFinding> findings)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <title>Scan Results</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; margin: 20px; }");
        sb.AppendLine("    h1 { margin-bottom: 0.5rem; }");
        sb.AppendLine("    h2 { margin-top: 2rem; }");
        sb.AppendLine("    table { border-collapse: collapse; width: 100%; margin-top: 0.5rem; }");
        sb.AppendLine("    th, td { border: 1px solid #ddd; padding: 6px 8px; font-size: 0.9rem; }");
        sb.AppendLine("    th { background-color: #f5f5f5; text-align: left; }");
        sb.AppendLine("    tr:nth-child(even) { background-color: #fafafa; }");
        sb.AppendLine("    .sev-High { color: #b00020; font-weight: 600; }");
        sb.AppendLine("    .sev-Medium { color: #e65100; font-weight: 600; }");
        sb.AppendLine("    .sev-Low { color: #33691e; font-weight: 600; }");
        sb.AppendLine("    .sev-Info { color: #455a64; }");
        sb.AppendLine("    .meta { margin: 0.25rem 0 0.75rem 0; font-size: 0.85rem; color: #555; }");
        sb.AppendLine("    .meta dt { font-weight: 600; display: inline; }");
        sb.AppendLine("    .meta dd { display: inline; margin: 0 1rem 0 0.25rem; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <h1>Scan Results</h1>");

        var grouped = findings
            .GroupBy(f => f.Plugin ?? "unknown")
            .OrderBy(g => g.Key);

        if (!grouped.Any())
        {
            sb.AppendLine("  <p>No findings.</p>");
        }
        else
        {
            foreach (var pluginGroup in grouped)
            {
                sb.AppendLine($"  <h2>{Html(pluginGroup.Key)}</h2>");

                sb.AppendLine("  <table>");
                sb.AppendLine("    <thead>");
                sb.AppendLine("      <tr>");
                sb.AppendLine("        <th>Severity</th>");
                sb.AppendLine("        <th>Title</th>");
                sb.AppendLine("        <th>Target</th>");
                sb.AppendLine("        <th>Description</th>");
                sb.AppendLine("      </tr>");
                sb.AppendLine("    </thead>");
                sb.AppendLine("    <tbody>");

                foreach (var f in pluginGroup)
                {
                    var sev = f.Severity ?? "Info";
                    var sevClass = $"sev-{Html(sev)}";

                    sb.AppendLine("      <tr>");
                    sb.AppendLine($"        <td class=\"{sevClass}\">{Html(sev)}</td>");
                    sb.AppendLine($"        <td>{Html(f.Title)}</td>");
                    sb.AppendLine($"        <td>{Html(f.Target)}</td>");
                    sb.AppendLine($"        <td>{Html(f.Description)}</td>");
                    sb.AppendLine("      </tr>");

                    if (f.Metadata is { Count: > 0 })
                    {
                        sb.AppendLine("      <tr>");
                        sb.AppendLine("        <td colspan=\"4\">");
                        sb.AppendLine("          <dl class=\"meta\">");
                        foreach (var kvp in f.Metadata)
                        {
                            sb.AppendLine($"            <dt>{Html(kvp.Key)}:</dt><dd>{Html(kvp.Value)}</dd>");
                        }
                        sb.AppendLine("          </dl>");
                        sb.AppendLine("        </td>");
                        sb.AppendLine("      </tr>");
                    }
                }

                sb.AppendLine("    </tbody>");
                sb.AppendLine("  </table>");
            }
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string Html(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }
}