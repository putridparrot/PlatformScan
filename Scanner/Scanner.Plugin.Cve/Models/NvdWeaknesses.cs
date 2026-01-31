namespace Scanner.Plugin.Cve.Models;

internal sealed class NvdWeaknesses
{
    public string Source { get; set; } = "";
    public string Type { get; set; } = "";
    public List<NvdWeaknessDescription> Description { get; set; } = new();
}