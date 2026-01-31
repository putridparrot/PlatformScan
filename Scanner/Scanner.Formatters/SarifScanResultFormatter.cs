using System.Text.Json;
using System.Text.Json.Serialization;
using Scanner.Core;
using Scanner.Formatters.Models;

namespace Scanner.Formatters;

public sealed class SarifScanResultFormatter : IScanResultFormatter
{
    public string Name => "sarif";
    public string FileExtension => "sarif";

    public string Format(IEnumerable<ScanFinding> findings)
    {
        var sarif = new SarifLog
        {
            Version = "2.1.0",
            Runs =
            [
                new SarifRun
                {
                    Tool = new SarifTool
                    {
                        Driver = new SarifDriver
                        {
                            Name = "UnifiedScanner",
                            Rules = BuildRules(findings)
                        }
                    },
                    Results = BuildResults(findings)
                }
            ]
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(sarif, options);
    }

    private static SarifRule[] BuildRules(IEnumerable<ScanFinding> findings)
    {
        return findings
            .GroupBy(f => f.Id ?? f.Title ?? "unknown")
            .Select(g => new SarifRule
            {
                Id = g.Key,
                Name = g.First().Title ?? g.Key,
                ShortDescription = new SarifMessage { Text = g.First().Description }
            })
            .ToArray();
    }

    private static SarifResult[] BuildResults(IEnumerable<ScanFinding> findings)
    {
        return findings.Select(f => new SarifResult
        {
            RuleId = f.Id ?? f.Title ?? "unknown",
            Message = new SarifMessage { Text = f.Description },
            Level = MapSeverity(f.Severity),
            Locations = new[]
            {
                new SarifLocation
                {
                    PhysicalLocation = new SarifPhysicalLocation
                    {
                        ArtifactLocation = new SarifArtifactLocation
                        {
                            Uri = f.Target
                        }
                    }
                }
            }
        }).ToArray();
    }

    private static string MapSeverity(string? severity)
    {
        return severity?.ToLowerInvariant() switch
        {
            "high" => "error",
            "medium" => "warning",
            "low" => "note",
            _ => "none"
        };
    }
}