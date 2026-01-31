namespace Scanner.Formatters.Models;

internal sealed class SarifRule
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public SarifMessage ShortDescription { get; set; } = new();
}