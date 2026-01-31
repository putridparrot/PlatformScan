namespace Scanner.Plugin.Cve.Models;

internal sealed class NvdConfigurations
{
    public List<NvdNode> Nodes { get; set; } = new();
}