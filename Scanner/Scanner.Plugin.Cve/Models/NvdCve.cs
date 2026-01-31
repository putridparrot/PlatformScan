namespace Scanner.Plugin.Cve.Models;

internal sealed class NvdCve
{
    public string Id { get; set; } = "";
    public string Published { get; set; } = "";
    public string LastModified { get; set; } = "";

    public List<NvdDescription> Descriptions { get; set; } = new();
    public NvdMetrics? Metrics { get; set; }
    public NvdWeaknesses[]? Weaknesses { get; set; }
    public NvdConfigurations[]? Configurations { get; set; }
}