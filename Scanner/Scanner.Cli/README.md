# Platform Scan CLI

## Basic Usage

### List available scanners
```bash
platformscan list
```

### Run scanners with default appsettings.json
```bash
platformscan run --plugin tls-scanner
platformscan run --plugin aks-health --format junit
```

### Use custom config file
```bash
platformscan run --plugin tls-scanner --config /path/to/custom-config.json
platformscan run --plugin tls-scanner -c ./production-settings.json
```

### Specify output file
```bash
platformscan run --plugin aks-health --format junit --output results.xml
platformscan run --plugin tls-scanner --format markdown --output scan-report.md
```

### Send results to Microsoft Teams
```bash
# Using webhook from appsettings.json
platformscan run --plugin tls-scanner --teams

# Override with specific webhook URL
platformscan run --plugin tls-scanner --teams https://outlook.office.com/webhook/YOUR_WEBHOOK_URL

# Combine with other options
platformscan run --plugin aks-health --config ./azure-config.json --format junit --output results.xml --teams
```

## Configuration

### appsettings.json Example

```json
{
  "Teams": {
    "WebhookUrl": "https://outlook.office.com/webhook/YOUR_WEBHOOK_URL_HERE"
  },
  "TlsScanner": {
    "Targets": ["https://example.com"]
  },
  "HttpHeaderScanner": {
    "Targets": ["https://example.com"]
  },
  "CveScanner": {
    "OnlyToday": true,
    "MinSeverity": "Medium",
    "Vendors": ["Microsoft", "VMware"]
  },
  "AksHealth": {
    "ClusterName": "my-aks",
    "ResourceGroup": "rg-platform",
    "IncludeAzureChecks": true,
    "IncludeKubernetesChecks": true
  }
}
```

## Setting up Teams Integration

1. **Create Incoming Webhook in Teams:**
   - Go to your Teams channel
   - Click "..." ? "Connectors" ? "Incoming Webhook"
   - Name it (e.g., "Platform Scanner")
   - Copy the webhook URL

2. **Configure the webhook:**
   - Add to `appsettings.json` under `Teams.WebhookUrl`, OR
   - Pass directly via `--teams <url>` parameter

3. **Run with Teams notification:**
   ```bash
   platformscan run --plugin tls-scanner --teams
   ```

## Available Formats

- `console` - Console output (default)
- `junit` - JUnit XML format
- `json` - JSON format
- `markdown` - Markdown format
- `html` - HTML format
- `sarif` - SARIF format
- `teams` - Microsoft Teams Adaptive Card format