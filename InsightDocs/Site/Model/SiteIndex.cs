using InsightDocs.Abstractions;

namespace InsightDocs.Site.Model;

public class SiteIndex : ILinkTarget
{
    public string LinkText
    {
        get;
        set;
    } = "";
}