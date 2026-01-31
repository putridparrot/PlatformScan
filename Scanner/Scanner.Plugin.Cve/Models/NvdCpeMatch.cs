namespace Scanner.Plugin.Cve.Models;

internal sealed class NvdCpeMatch
{
    public string Criteria { get; set; } = "";

    public string? CriteriaVendor()
    {
        // cpe:2.3:a:microsoft:edge:...
        var parts = Criteria.Split(':');
        return parts.Length > 4 ? parts[3] : null;
    }

    public string? CriteriaProduct()
    {
        var parts = Criteria.Split(':');
        return parts.Length > 5 ? parts[4] : null;
    }
}