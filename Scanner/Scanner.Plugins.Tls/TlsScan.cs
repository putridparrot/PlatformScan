using Scanner.Plugins.Tls.Models;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Scanner.Plugins.Tls;

internal static class TlsScan
{
    public static async Task<TlsScanResult> CheckAsync(string url, CancellationToken ct)
    {
        var uri = new Uri(url);

        using var client = new TcpClient();
        await client.ConnectAsync(uri.Host, uri.Port, ct);

        await using var ssl = new SslStream(client.GetStream(), false, (_, _, _, _) => true);

        await ssl.AuthenticateAsClientAsync(uri.Host);

        var negotiated = ssl.SslProtocol.ToString();
        var cipher = ssl.NegotiatedCipherSuite.ToString();
        var cert = new X509Certificate2(ssl.RemoteCertificate);

        var issues = new List<string>();
        var metadata = new Dictionary<string, string>
        {
            ["TlsVersion"] = negotiated,
            ["CipherSuite"] = cipher,
            ["CertSubject"] = cert.Subject,
            ["CertIssuer"] = cert.Issuer,
            ["CertNotBefore"] = cert.NotBefore.ToString("o"),
            ["CertNotAfter"] = cert.NotAfter.ToString("o")
        };

#pragma warning disable SYSLIB0039
        if (ssl.SslProtocol is SslProtocols.Tls or SslProtocols.Tls11)
#pragma warning restore SYSLIB0039
            issues.Add($"Deprecated TLS version negotiated: {negotiated}");

        if (cert.NotAfter < DateTime.UtcNow.AddDays(30))
            issues.Add("Certificate expires within 30 days");

        if (cert.NotAfter < DateTime.UtcNow)
            issues.Add("Certificate is expired");

        return new TlsScanResult
        {
            HasIssues = issues.Any(),
            Severity = issues.Any(i => i.Contains("expired")) ? "High" :
                issues.Any() ? "Medium" : "Info",
            Description = issues.Any()
                ? string.Join("; ", issues)
                : "No TLS issues detected",
            Metadata = metadata
        };
    }
}