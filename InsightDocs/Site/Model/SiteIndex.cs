using InsightDocs.Abstractions;

namespace InsightDocs.Site.Model;

public class SiteIndex(string title, string? initialUrl) : ILinkTarget
{
    public string LinkText
    {
        get;
        set;
    } = title;

    public string Title
    {
        get;
        set;
    } = title;

    public string? InitialUrl
    {
        get;
        set;
    } = initialUrl;
}