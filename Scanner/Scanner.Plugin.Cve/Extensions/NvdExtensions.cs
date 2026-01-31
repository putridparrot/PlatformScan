using Scanner.Plugin.Cve.Models;

namespace Scanner.Plugin.Cve.Extensions;

internal static class NvdExtensions
{
    extension(NvdCve cve)
    {
        public IEnumerable<string> Vendors()
        {
            return cve.Configurations?
                       .SelectMany(c => c.Nodes)
                       .SelectMany(n => n.CpeMatch)
                       .Select(m => m.CriteriaVendor())
                       .Where(v => v is not null)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                   ?? Enumerable.Empty<string>();
        }

        public IEnumerable<string> Products()
        {
            return cve.Configurations?
                       .SelectMany(c => c.Nodes)
                       .SelectMany(n => n.CpeMatch)
                       .Select(m => m.CriteriaProduct())
                       .Where(p => p is not null)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                   ?? Enumerable.Empty<string>();
        }
    }
}