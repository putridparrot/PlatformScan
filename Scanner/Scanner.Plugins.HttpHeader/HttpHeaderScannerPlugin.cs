using Microsoft.Extensions.Options;
using Scanner.Plugins.HttpHeader.Models;
using Scanner.Plugins.HttpHeader.Options;
using System.Net.Http.Headers;
using Scanner.Core;

namespace Scanner.Plugins.HttpHeader;

public sealed class HttpHeaderScannerPlugin(IOptions<HttpHeaderScannerOptions> options, IHttpClientFactory httpClientFactory)
    : IScannerPlugin
{
    private readonly HttpHeaderScannerOptions _options = options.Value;
    private readonly HttpClient _http = httpClientFactory.CreateClient(nameof(HttpHeaderScannerPlugin));

    public string Name => "http-headers";
    public string Description => "Scans targets for missing HTTP security headers.";

    public async Task<IReadOnlyCollection<ScanFinding>> ExecuteAsync(
        ScanContext context,
        CancellationToken ct)
    {
        var findings = new List<ScanFinding>();

        foreach (var target in _options.Targets)
        {
            var result = await ScanTargetAsync(target, ct);

            foreach (var issue in result)
            {
                findings.Add(new ScanFinding
                {
                    Plugin = Name,
                    Id = $"http-header-{target}-{issue.Id}",
                    Title = issue.Title,
                    Severity = issue.Severity,
                    Target = target,
                    Description = issue.Description,
                    Metadata = issue.Metadata
                });
            }
        }

        return findings;
    }

    private async Task<IReadOnlyCollection<HeaderIssue>> ScanTargetAsync(
        string url,
        CancellationToken ct)
    {
        var issues = new List<HeaderIssue>();

        HttpResponseMessage? response = null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            response = await _http.SendAsync(request, ct);

            // Some servers don't support HEAD
            if (!response.IsSuccessStatusCode)
            {
                request = new HttpRequestMessage(HttpMethod.Get, url);
                response = await _http.SendAsync(request, ct);
            }
        }
        catch (Exception ex)
        {
            issues.Add(new HeaderIssue("unreachable","Site unreachable","High",ex.Message));
            return issues;
        }

        var headers = response.Headers;

        CheckHeader(headers, issues,
            name: "Strict-Transport-Security",
            severity: "High",
            description: "Missing HSTS header. Site may be vulnerable to downgrade attacks.");

        CheckHeader(headers, issues,
            name: "Content-Security-Policy",
            severity: "High",
            description: "Missing CSP header. Site may be vulnerable to XSS.");

        CheckHeader(headers, issues,
            name: "X-Frame-Options",
            severity: "Medium",
            description: "Missing X-Frame-Options header. Site may be vulnerable to clickjacking.");

        CheckHeader(headers, issues,
            name: "X-Content-Type-Options",
            severity: "Medium",
            description: "Missing X-Content-Type-Options header. MIME sniffing may be possible.");

        CheckHeader(headers, issues,
            name: "Referrer-Policy",
            severity: "Low",
            description: "Missing Referrer-Policy header. Referrer leakage may occur.");

        CheckHeader(headers, issues,
            name: "Permissions-Policy",
            severity: "Low",
            description: "Missing Permissions-Policy header. Browser features may be overly permissive.");

        return issues;
    }

    private static void CheckHeader(
        HttpResponseHeaders headers,
        List<HeaderIssue> issues,
        string name,
        string severity,
        string description)
    {
        if (!headers.Contains(name))
        {
            issues.Add(new HeaderIssue(name.ToLowerInvariant(), $"{name} header missing", severity,description));
        }
    }
}