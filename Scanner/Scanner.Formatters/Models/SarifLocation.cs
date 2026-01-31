namespace Scanner.Formatters.Models;

internal sealed class SarifLocation
{
    public SarifPhysicalLocation PhysicalLocation { get; set; } = new();
}