namespace Scanner.Formatters.Models;

internal sealed class SarifRun
{
    public SarifTool Tool { get; set; } = new();
    public SarifResult[] Results { get; set; } = [];
}