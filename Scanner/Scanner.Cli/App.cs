using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Scanner.Cli.Options;
using Scanner.Core;
using System.CommandLine;

namespace Scanner.Cli;

public sealed class App(IHost host)
{
    public async Task<int> RunAsync(string[] args)
    {
        var root = new RootCommand("Platform Scanner CLI");

        // Global --config option
        var configOption = new Option<string?>(
            name: "--config",
            description: "Path to custom appsettings.json file"
        );
        configOption.AddAlias("-c");
        root.AddGlobalOption(configOption);

        // scanner list
        var listCmd = new Command("list", "List all available scanners");

        listCmd.SetHandler(ListPlugins);
        root.AddCommand(listCmd);

        // scanner run
        var runCmd = new Command("run", "Run one or more scanners");
        var pluginOption = new Option<string[]>("--plugin", "Specific plugin(s) to run")
        {
            AllowMultipleArgumentsPerToken = true
        };
        pluginOption.SetDefaultValue(Array.Empty<string>());

        runCmd.AddOption(pluginOption);

        // formatters
        var formatOption = new Option<string>(
            name: "--format",
            description: "Output format (e.g., junit)" //, json, markdown, sarif
        );
        formatOption.SetDefaultValue("console");
        runCmd.AddOption(formatOption);

        // output file
        var outputOption = new Option<string?>(
            name: "--output",
            description: "Output file path (defaults to scan-results.{ext} if not specified)"
        );
        runCmd.AddOption(outputOption);

        // teams webhook
        var teamsOption = new Option<string?>(
            name: "--teams",
            description: "Send results to Microsoft Teams webhook URL (overrides config file)"
        );
        runCmd.AddOption(teamsOption);

        runCmd.SetHandler(
            async (string[] plugins, string format, string? output, string? teams) =>
            {
                await RunPlugins(plugins, format, output, teams);
            },
            pluginOption,
            formatOption,
            outputOption,
            teamsOption
        );

        root.AddCommand(runCmd);

        return await root.InvokeAsync(args);
    }

    private void ListPlugins()
    {
        var plugins = host.Services.GetRequiredService<IEnumerable<IScannerPlugin>>();

        Console.WriteLine("Available scanners:");
        foreach (var plugin in plugins)
        {
            Console.WriteLine($" - {plugin.Name}");
            Console.WriteLine($"     {plugin.Description}");
        }
    }

    private async Task RunPlugins(string[] pluginNames, string format, string? outputFile, string? teamsWebhook)
    {
        var runner = host.Services.GetRequiredService<ScanRunner>();
        var findings = await runner.RunAsync(pluginNames, CancellationToken.None);

        var formatter = ResolveFormatter(format);

        var output = formatter.Format(findings);

        // Send to Teams if webhook is provided
        if (!string.IsNullOrWhiteSpace(teamsWebhook))
        {
            await SendToTeams(teamsWebhook, findings);
        }

        // Write to file or console depending on format
        if (format.Equals("console", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(output);
        }
        else
        {
            var fileName = outputFile ?? $"scan-results.{formatter.FileExtension}";
            await File.WriteAllTextAsync(fileName, output);
            Console.WriteLine($"Results written to {fileName}");
        }
    }

    private IScanResultFormatter ResolveFormatter(string format)
    {
        var all = host.Services.GetRequiredService<IEnumerable<IScanResultFormatter>>();

        var match = all.FirstOrDefault(f =>
            f.Name.Equals(format, StringComparison.OrdinalIgnoreCase));

        if (match is null)
            throw new InvalidOperationException($"Unknown format '{format}'");

        return match;
    }

    private async Task SendToTeams(string? webhookUrl, IReadOnlyCollection<ScanFinding> findings)
    {
        // Get webhook from parameter or config
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            var teamsOptions = host.Services.GetRequiredService<IOptions<TeamsOptions>>();
            webhookUrl = teamsOptions.Value.WebhookUrl;
        }

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            Console.WriteLine("⚠️  Teams webhook URL not provided. Skipping Teams notification.");
            return;
        }

        try
        {
            var formatter = ResolveFormatter("teams");
            var payload = formatter.Format(findings);

            var sender = host.Services.GetRequiredService<TeamsSender>();
            await sender.SendAsync(webhookUrl, payload);

            Console.WriteLine("✅ Results sent to Microsoft Teams successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send to Teams: {ex.Message}");
        }
    }
}