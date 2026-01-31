namespace Scanner.Plugin.Cve.Options;

// https://services.nvd.nist.gov/rest/json/cves/2.0?pubStartDate=2026-01-31T00:00:00.000&pubEndDate=2026-01-31T23:59:59.000
// ?lastModStartDate=... Last 24 hours

public sealed class CveScannerOptions
{
    public bool OnlyToday { get; set; } = true;

    public string[]? Vendors { get; set; }
    public string[]? Products { get; set; }

    public string? MinSeverity { get; set; } = "Medium"; // Low, Medium, High, Critical
}