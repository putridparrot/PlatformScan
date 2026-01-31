using System.Text.Json;
using System.Text.Json.Serialization;
using Scanner.Core;

namespace Scanner.Formatters;

public sealed class JsonScanResultFormatter : IScanResultFormatter
{
    public string Name => "json";
    public string FileExtension => "json";

    public string Format(IEnumerable<ScanFinding> findings)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(findings, options);
    }
}