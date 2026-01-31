namespace Scanner.Plugin.Cve.Models;

internal sealed class NvdNode
{
    public List<NvdCpeMatch> CpeMatch { get; set; } = new();
}