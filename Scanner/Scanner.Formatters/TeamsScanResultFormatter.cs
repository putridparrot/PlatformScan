using Scanner.Core;
using System.Text.Json;

namespace Scanner.Formatters;

public sealed class TeamsScanResultFormatter : IScanResultFormatter
{
    public string Name => "teams";
    public string FileExtension => "json";

    public string Format(IEnumerable<ScanFinding> findings)
    {
        var findingsList = findings.ToList();
        
        var card = new
        {
            type = "message",
            attachments = new[]
            {
                new
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    content = new
                    {
                        type = "AdaptiveCard",
                        version = "1.4",
                        body = new List<object>
                        {
                            new
                            {
                                type = "TextBlock",
                                text = "?? Platform Scan Results",
                                weight = "Bolder",
                                size = "Large"
                            },
                            new
                            {
                                type = "FactSet",
                                facts = new[]
                                {
                                    new { title = "Total Findings", value = findingsList.Count.ToString() },
                                    new { title = "High Severity", value = findingsList.Count(f => f.Severity == "High").ToString() },
                                    new { title = "Medium Severity", value = findingsList.Count(f => f.Severity == "Medium").ToString() },
                                    new { title = "Low Severity", value = findingsList.Count(f => f.Severity == "Low").ToString() },
                                    new { title = "Scan Time", value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") }
                                }
                            }
                        }
                        .Concat(findingsList.Take(10).Select(f => CreateFindingSection(f)))
                        .ToList(),
                        schema = "http://adaptivecards.io/schemas/adaptive-card.json"
                    }
                }
            }
        };

        return JsonSerializer.Serialize(card, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    private static object CreateFindingSection(ScanFinding finding)
    {
        var color = finding.Severity switch
        {
            "High" => "Attention",
            "Medium" => "Warning",
            _ => "Default"
        };

        return new
        {
            type = "Container",
            style = color,
            items = new object[]
            {
                new
                {
                    type = "TextBlock",
                    text = $"**[{finding.Severity}]** {finding.Title}",
                    weight = "Bolder",
                    wrap = true
                },
                new
                {
                    type = "TextBlock",
                    text = finding.Description,
                    wrap = true,
                    isSubtle = true
                },
                new
                {
                    type = "FactSet",
                    facts = new[]
                    {
                        new { title = "Plugin", value = finding.Plugin },
                        new { title = "Target", value = finding.Target },
                        new { title = "ID", value = finding.Id }
                    }
                }
            },
            separator = true,
            spacing = "Medium"
        };
    }
}
