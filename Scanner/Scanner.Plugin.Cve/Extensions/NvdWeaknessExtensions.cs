using Scanner.Plugin.Cve.Models;

namespace Scanner.Plugin.Cve.Extensions;

internal static class NvdWeaknessExtensions
{
    public static IEnumerable<string> Cwes(this NvdCve cve)
    {
        return cve.Weaknesses?
                   .SelectMany(w => w.Description)
                   .Select(d => d.Value)
                   .Where(v => !string.IsNullOrWhiteSpace(v))
               ?? Enumerable.Empty<string>();
    }
}