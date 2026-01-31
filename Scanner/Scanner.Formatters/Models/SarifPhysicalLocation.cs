namespace Scanner.Formatters.Models;

internal sealed class SarifPhysicalLocation
{
    public SarifArtifactLocation ArtifactLocation { get; set; } = new();
}