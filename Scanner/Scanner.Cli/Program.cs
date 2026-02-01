using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

// Extract config file argument before building host
string? configFile = null;
for (var i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--config" || args[i] == "-c")
    {
        configFile = args[i + 1];
        break;
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

builder.Services.AddLogging();
builder.Services.AddSingleton<ScanRunner>();

builder.Services.AddSingleton<IScannerPlugin, TlsScannerPlugin>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<IScannerPlugin, HttpHeaderScannerPlugin>();
builder.Services.AddSingleton<IScannerPlugin, CveScannerPlugin>();
builder.Services.AddSingleton<IScannerPlugin, AksHealthPlugin>();

builder.Services.Configure<TlsScannerOptions>(
    builder.Configuration.GetSection("TlsScanner"));

builder.Services
    .AddOptions<TlsScannerOptions>()
    .Bind(builder.Configuration.GetSection("TlsScanner"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<HttpHeaderScannerOptions>()
    .Bind(builder.Configuration.GetSection("HttpHeaderScanner"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<CveScannerOptions>()
    .Bind(builder.Configuration.GetSection("CveScanner"))
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
