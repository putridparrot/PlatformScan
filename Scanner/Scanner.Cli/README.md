# Platform Scan CLI

## Basic Usage

### List available scanners
```bash
platformscan list
```

### Run scanners with default appsettings.json
```bash
platformscan run --plugin tls
platformscan run --plugin aks-health --format junit
```

### Use custom config file
```bash
platformscan run --plugin tls --config /path/to/custom-config.json
platformscan run --plugin tls -c ./production-settings.json
```

### Specify output file
```bash
platformscan run --plugin aks-health --format junit --output results.xml
platformscan run --plugin tls --format markdown --output scan-report.md
```

### Send results to Microsoft Teams
```bash
# Using webhook from appsettings.json
platformscan run --plugin tls --teams

# Override with specific webhook URL
platformscan run --plugin tls --teams https://outlook.office.com/webhook/YOUR_WEBHOOK_URL

# Combine with other options
platformscan run --plugin aks-health --config ./azure-config.json --format junit --output results.xml --teams
```

## Configuration

### appsettings.json Example

```json
{
  "Tls": {
    "Targets": ["https://example.com"]
  },
  "HttpHeader": {
    "Targets": ["https://example.com"]
  },
  "Cve": {
    "OnlyToday": true,
    "MinSeverity": "Medium",
    "Vendors": ["Microsoft", "VMware"],
    "Products": ["node.js", "react", "asp.net"]
  },
  "AksHealth": {
    "ClusterName": "my-aks",
    "ResourceGroup": "rg-platform",
    "IncludeAzureChecks": true,
    "IncludeKubernetesChecks": true
  },
  "Teams": {
    "WebhookUrl": "https://outlook.office.com/webhook/YOUR_WEBHOOK_URL_HERE"
  }
}
```

### Logging Configuration (Optional)

By default, logging is **disabled**. You can enable it with the `--verbose` flag, or configure custom logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning",
      "Scanner": "Debug"
    }
  }
}
```

**Logging Priority:**
1. `--verbose` flag ? enables logging (overrides everything)
2. `Logging` section in appsettings.json ? custom configuration
3. Default ? logging disabled (no output)

### CVE Scanner Filtering

The CVE scanner supports multiple filtering options to show only relevant vulnerabilities:

**Configuration Options:**
- `OnlyToday`: `true` | `false` - Only show CVEs published today (default: `true`)
- `MinSeverity`: `"Low"` | `"Medium"` | `"High"` | `"Critical"` - Minimum severity level
- `Vendors`: Array of vendor names - Filter by vendor (e.g., `["Microsoft", "VMware"]`)
- `Products`: Array of product/technology names - Filter by product (e.g., `["node.js", "react", "asp.net"]`)

**Example - Filter for your tech stack:**
```json
{
  "Cve": {
    "OnlyToday": true,
    "MinSeverity": "Medium",
    "Products": [
      "node.js",
      "react",
      "asp.net",
      "typescript",
      "postgresql"
    ]
  }
}
```

**How it works:**
- The scanner queries the NVD (National Vulnerability Database) API
- Filters are applied to CPE (Common Platform Enumeration) data
- Only CVEs matching **all** specified filters are returned
- Product names are case-insensitive

**Common product names:**
- Frontend: `react`, `vue.js`, `angular`, `typescript`
- Backend: `node.js`, `asp.net`, `django`, `spring`
- Databases: `postgresql`, `mongodb`, `redis`, `mysql`
- Infrastructure: `docker`, `kubernetes`, `nginx`, `apache`

**Output format:**
- CVE findings include a **short description** (truncated to 200 characters)
- Direct link to the NVD website is included in the description
- Full description is available in the metadata for detailed analysis
- Vendors and products extracted from CPE data are included in metadata
- Example output: `"Buffer overflow vulnerability in... More info: https://nvd.nist.gov/vuln/detail/CVE-2024-1234"`

**Troubleshooting:**
If you're not seeing filtered results:
1. Run with `--verbose` to see what's being filtered
2. Check the metadata in the output - it shows which vendors/products were detected
3. Note: Not all CVEs have vendor/product data in the NVD database
4. Try removing filters temporarily to see all CVEs, then check their metadata for correct vendor/product names

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
- `junit` - JUnit XML format (supports pass/fail for CI/CD)
- `json` - JSON format
- `markdown` - Markdown format
- `html` - HTML format
- `sarif` - SARIF format
- `teams` - Microsoft Teams Adaptive Card format

## Severity Levels

All plugins report findings with severity levels:

- **Info** - Informational, no issues detected (passes in JUnit)
- **Low** - Minor issues (passes in JUnit)
- **Medium** - Moderate issues requiring attention (fails in JUnit)
- **High** - Critical issues requiring immediate attention (fails in JUnit)

## JUnit Integration

The scanner reports both successes and failures, making it suitable for CI/CD pipelines:

```yaml
# Azure DevOps example
- task: PublishTestResults@2
  inputs:
    testResultsFormat: 'JUnit'
    testResultsFiles: 'scan-results.xml'
    failTaskOnFailedTests: true
```

**Test Results:**
- ? **Pass**: Findings with severity `Info` or `Low`
- ? **Fail**: Findings with severity `Medium` or `High`