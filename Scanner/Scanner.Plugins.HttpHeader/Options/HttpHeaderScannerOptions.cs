using System.ComponentModel.DataAnnotations;

namespace Scanner.Plugins.HttpHeader.Options;

public sealed class HttpHeaderScannerOptions
{
    [Required]
    public string[] Targets { get; set; } = [];
}