using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scanner.Cli;
using Scanner.Cli.Options;
using Scanner.Core;
using Scanner.Formatters;
using Scanner.Plugin.Cve;
using Scanner.Plugin.Cve.Options;
using Scanner.Plugins.Aks;
using Scanner.Plugins.Aks.Options;
using Scanner.Plugins.HttpHeader;
using Scanner.Plugins.HttpHeader.Options;
using Scanner.Plugins.Tls;
using Scanner.Plugins.Tls.Options;

// Extract config file and verbose arguments before building host
string? configFile = null;
bool verbose = false;

for (var i = 0; i < args.Length; i++)
{
    if ((args[i] == "--config" || args[i] == "-c") && i + 1 < args.Length)
    {
        configFile = args[i + 1];
    }
    else if (args[i] == "--verbose" || args[i] == "-v")
    {
        verbose = true;
    }
}

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    DisableDefaults = false
});

// Configure custom appsettings.json if provided
if (!string.IsNullOrEmpty(configFile))
{
    builder.Configuration.Sources.Clear();
    builder.Configuration
        .AddJsonFile(configFile, optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args);
}

// Configure logging: off by default, enable with --verbose, or respect user's appsettings.json
builder.Logging.ClearProviders();

// Check if user has configured logging in appsettings.json
var loggingSection = builder.Configuration.GetSection("Logging");
var hasUserLoggingConfig = loggingSection.Exists();

if (verbose)
{
    // --verbose flag enables logging
    builder.Logging.AddConsole();
    builder.Logging.AddFilter("Microsoft", LogLevel.Information);
    builder.Logging.AddFilter("System", LogLevel.Information);
    builder.Logging.AddFilter("Scanner", LogLevel.Debug);
}
else if (hasUserLoggingConfig)
{
    // Respect user's logging configuration from appsettings.json
    builder.Logging.AddConfiguration(loggingSection);
    builder.Logging.AddConsole();
}
// else: logging stays disabled (no providers added)

builder.Services.AddLogging();
builder.Services.AddSingleton<ScanRunner>();

builder.Services.AddSingleton<IScannerPlugin, TlsScannerPlugin>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IScannerPlugin, HttpHeaderScannerPlugin>();
builder.Services.AddSingleton<IScannerPlugin, CveScannerPlugin>();
builder.Services.AddSingleton<IScannerPlugin, AksHealthPlugin>();

builder.Services
    .AddOptions<TlsScannerOptions>()
    .Bind(builder.Configuration.GetSection("Tls"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<HttpHeaderScannerOptions>()
    .Bind(builder.Configuration.GetSection("HttpHeader"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<CveScannerOptions>()
    .Bind(builder.Configuration.GetSection("Cve"))
    .ValidateOnStart();

builder.Services
    .AddOptions<AksHealthOptions>()
    .Bind(builder.Configuration.GetSection("AksHealth"))
    .ValidateOnStart();

builder.Services
    .AddOptions<TeamsOptions>()
    .Bind(builder.Configuration.GetSection("Teams"))
    .ValidateOnStart();

builder.Services.AddSingleton<IScanResultFormatter, JunitScanResultFormatter>();
builder.Services.AddSingleton<IScanResultFormatter, ConsoleScanResultFormatter>();
builder.Services.AddSingleton<IScanResultFormatter, MarkdownScanResultFormatter>();
builder.Services.AddSingleton<IScanResultFormatter, HtmlScanResultFormatter>();
builder.Services.AddSingleton<IScanResultFormatter, JsonScanResultFormatter>();
builder.Services.AddSingleton<IScanResultFormatter, SarifScanResultFormatter>();
builder.Services.AddSingleton<IScanResultFormatter, TeamsScanResultFormatter>();

builder.Services.AddSingleton<TeamsSender>();

var host = builder.Build();

var app = new App(host);
return await app.RunAsync(args);
