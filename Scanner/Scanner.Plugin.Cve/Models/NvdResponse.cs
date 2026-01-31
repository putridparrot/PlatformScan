namespace Scanner.Plugin.Cve.Models;

internal sealed class NvdResponse
{
    public List<NvdVulnerability> Vulnerabilities { get; set; } = new();
}