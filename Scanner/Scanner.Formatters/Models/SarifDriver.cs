namespace Scanner.Formatters.Models;

internal sealed class SarifDriver
{
    public string Name { get; set; } = "Scanner";
    public SarifRule[] Rules { get; set; } = [];
}