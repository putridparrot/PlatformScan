namespace Scanner.Formatters.Models;

internal sealed class SarifLog
{
    public string Version { get; set; } = "2.1.0";
    public SarifRun[] Runs { get; set; } = [];
}