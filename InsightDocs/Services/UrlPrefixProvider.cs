using InsightDocs.Abstractions;

namespace InsightDocs.Services;

public class UrlPrefixProvider : IUrlPrefixProvider
{
    public string? UrlPrefix
    {
        get;
        set;
    }
}
