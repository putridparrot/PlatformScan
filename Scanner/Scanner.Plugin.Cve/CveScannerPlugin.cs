using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Scanner.Core;
using Scanner.Plugin.Cve.Extensions;
using Scanner.Plugin.Cve.Models;
using Scanner.Plugin.Cve.Options;

namespace Scanner.Plugin.Cve;

public sealed class CveScannerPlugin(IOptions<CveScannerOptions> options, IHttpClientFactory httpClientFactory)
    : IScannerPlugin
{
    private readonly CveScannerOptions _options = options.Value;
    private readonly HttpClient _http = httpClientFactory.CreateClient(nameof(CveScannerPlugin));

    public string Name => "cve";
    public string Description => "Scans for known CVEs from the NVD database.";

    public async Task<IReadOnlyCollection<ScanFinding>> ExecuteAsync(ScanContext context, CancellationToken ct)
    {
        var url = BuildUrl();
        var response = await _http.GetFromJsonAsync<NvdResponse>(url, ct);

        if (response?.Vulnerabilities is null)
            return [];

        var findings = new List<ScanFinding>();

        foreach (var item in response.Vulnerabilities)
        {
            var cve = item.Cve;
            var id = cve.Id;
            var desc = cve.Descriptions.FirstOrDefault()?.Value ?? "No description";

            var severity = cve.Metrics?.CvssMetricV31?.FirstOrDefault()?.CvssData.BaseSeverity
                           ?? "Unknown";

            if (!PassesFilters(severity, cve))
                continue;

            findings.Add(new ScanFinding
            {
                Plugin = Name,
                Id = id,
                Title = id,
                Severity = severity,
                Target = id,
                Description = desc,
                Metadata = new Dictionary<string, string>
                {
                    ["Published"] = cve.Published,
                    ["LastModified"] = cve.LastModified,
                    ["Url"] = $"https://nvd.nist.gov/vuln/detail/{id}"
                }
            });
        }

        return findings;
    }

    private string BuildUrl()
    {
        if (_options.OnlyToday)
        {
            var today = DateTime.UtcNow.Date;
            var start = today.ToString("yyyy-MM-ddT00:00:00.000");
            var end = today.ToString("yyyy-MM-ddT23:59:59.000");

            return $"https://services.nvd.nist.gov/rest/json/cves/2.0?pubStartDate={start}&pubEndDate={end}";
        }

        return "https://services.nvd.nist.gov/rest/json/cves/2.0?resultsPerPage=100";
    }

    private bool PassesFilters(string severity, NvdCve cve)
    {
        if (_options.MinSeverity is not null)
        {
            if (!SeverityAtLeast(severity, _options.MinSeverity))
                return false;
        }

        if (_options.Vendors?.Any() == true)
        {
            var vendors = cve.Vendors();
            if (!vendors.Any(v => _options.Vendors.Contains(v, StringComparer.OrdinalIgnoreCase)))
                return false;
        }

        if (_options.Products?.Any() == true)
        {
            var products = cve.Products();
            if (!products.Any(p => _options.Products.Contains(p, StringComparer.OrdinalIgnoreCase)))
                return false;
        }

        return true;
    }

    private static bool SeverityAtLeast(string sev, string min)
    {
        string[] order = { "low", "medium", "high", "critical" };
        var s = Array.IndexOf(order, sev.ToLower());
        var m = Array.IndexOf(order, min.ToLower());
        return s >= m;
    }
}