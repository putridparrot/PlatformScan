namespace Scanner.Plugin.Cve.Models;

internal sealed class NvdCvssData
{
    public string Version { get; set; } = "";
    public string VectorString { get; set; } = "";
    public string BaseSeverity { get; set; } = "";
    public double BaseScore { get; set; }
}