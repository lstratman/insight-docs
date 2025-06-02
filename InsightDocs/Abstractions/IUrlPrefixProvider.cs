namespace InsightDocs.Abstractions;

public interface IUrlPrefixProvider
{
    string? UrlPrefix
    {
        get;
        set;
    }
}
