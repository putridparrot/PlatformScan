using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ContainerService;
using k8s;
using Microsoft.Extensions.Options;
using Scanner.Core;
using Scanner.Plugins.Aks.Options;

namespace Scanner.Plugins.Aks;

public sealed class AksHealthPlugin(IOptions<AksHealthOptions> options) : IScannerPlugin
{
    private readonly AksHealthOptions _options = options.Value;

    public string Name => "aks-health";
    public string Description => "Checks the health of an Azure Kubernetes Service (AKS) cluster and its Kubernetes components.";

    public async Task<IReadOnlyCollection<ScanFinding>> ExecuteAsync(
        ScanContext context,
        CancellationToken ct)
    {
        var findings = new List<ScanFinding>();

        if (_options.IncludeAzureChecks)
            findings.AddRange(await CheckAzureAksAsync(ct));

        if (_options.IncludeKubernetesChecks)
            findings.AddRange(await CheckKubernetesAsync(ct));

        return findings;
    }

    private async Task<IEnumerable<ScanFinding>> CheckAzureAksAsync(CancellationToken ct)
    {
        var arm = new ArmClient(new DefaultAzureCredential());
        var sub = await arm.GetDefaultSubscriptionAsync(ct);

        var resourceGroupResource = await sub.GetResourceGroups()
            .GetAsync(_options.ResourceGroup, ct);

        var aks = await resourceGroupResource.Value
            .GetContainerServiceManagedClusters()
            .GetAsync(_options.ClusterName, ct);

        var cluster = aks.Value.Data;

        var results = new List<ScanFinding>();

        // Check provisioning state
        if (!cluster.ProvisioningState.Equals("Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            results.Add(new ScanFinding
            {
                Plugin = Name,
                Id = "aks-provisioning-state",
                Title = "AKS provisioning state is not healthy",
                Severity = "High",
                Target = _options.ClusterName,
                Description = $"Provisioning state: {cluster.ProvisioningState}",
                Metadata = new Dictionary<string, string>
                {
                    ["State"] = cluster.ProvisioningState
                }
            });
        }

        // Check version drift
        if (cluster.KubernetesVersion != cluster.CurrentKubernetesVersion)
        {
            results.Add(new ScanFinding
            {
                Plugin = Name,
                Id = "aks-version-drift",
                Title = "AKS control plane version drift detected",
                Severity = "Medium",
                Target = _options.ClusterName,
                Description = $"Control plane: {cluster.KubernetesVersion}, Current: {cluster.CurrentKubernetesVersion}",
                Metadata = new Dictionary<string, string>
                {
                    ["ControlPlane"] = cluster.KubernetesVersion,
                    ["Current"] = cluster.CurrentKubernetesVersion
                }
            });
        }

        return results;
    }

    private async Task<IEnumerable<ScanFinding>> CheckKubernetesAsync(CancellationToken ct)
    {
        var config = KubernetesClientConfiguration.BuildDefaultConfig();
        var client = new Kubernetes(config);

        var results = new List<ScanFinding>();

        // Node readiness
        var nodes = await client.ListNodeAsync(cancellationToken: ct);

        foreach (var node in nodes.Items)
        {
            var ready = node.Status.Conditions.Any(c =>
                c.Type == "Ready" && c.Status == "True");

            if (!ready)
            {
                results.Add(new ScanFinding
                {
                    Plugin = Name,
                    Id = $"node-{node.Metadata.Name}-not-ready",
                    Title = "Node not ready",
                    Severity = "High",
                    Target = node.Metadata.Name,
                    Description = "Node is not reporting Ready condition",
                    Metadata = new Dictionary<string, string>
                    {
                        ["Node"] = node.Metadata.Name
                    }
                });
            }
        }

        // Pods stuck in CrashLoopBackOff
        var pods = await client.ListPodForAllNamespacesAsync(cancellationToken: ct);

        foreach (var pod in pods.Items)
        {
            var status = pod.Status.ContainerStatuses?.FirstOrDefault();
            if (status?.State?.Waiting?.Reason == "CrashLoopBackOff")
            {
                results.Add(new ScanFinding
                {
                    Plugin = Name,
                    Id = $"pod-{pod.Metadata.Name}-crashloop",
                    Title = "Pod in CrashLoopBackOff",
                    Severity = "Medium",
                    Target = pod.Metadata.Name,
                    Description = $"{pod.Metadata.NamespaceProperty}/{pod.Metadata.Name}",
                    Metadata = new Dictionary<string, string>
                    {
                        ["Namespace"] = pod.Metadata.NamespaceProperty
                    }
                });
            }
        }

        return results;
    }
}