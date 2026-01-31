namespace Scanner.Formatters.Models;

internal sealed class SarifResult
{
    public string RuleId { get; set; } = "";
    public string Level { get; set; } = "none";
    public SarifMessage Message { get; set; } = new();
    public SarifLocation[] Locations { get; set; } = [];
}