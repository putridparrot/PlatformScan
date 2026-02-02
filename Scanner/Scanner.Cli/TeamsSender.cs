using System.Net.Http.Headers;
using System.Text;

namespace Scanner.Cli;

public sealed class TeamsSender(HttpClient httpClient)
{
    public async Task SendAsync(string webhookUrl, string jsonPayload, CancellationToken ct = default)
    {
        throw new NotImplementedException();

        if (string.IsNullOrWhiteSpace(webhookUrl))
            throw new ArgumentException("Webhook URL cannot be empty", nameof(webhookUrl));

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        
        var response = await httpClient.PostAsync(webhookUrl, content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Failed to send message to Teams. Status: {response.StatusCode}, Error: {error}");
        }
    }
}
