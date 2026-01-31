namespace Scanner.Plugins.Aks.Options;

public sealed class AksHealthOptions
{
    public string ClusterName { get; set; } = "";
    public string ResourceGroup { get; set; } = "";
    public bool IncludeKubernetesChecks { get; set; } = true;
    public bool IncludeAzureChecks { get; set; } = true;
}