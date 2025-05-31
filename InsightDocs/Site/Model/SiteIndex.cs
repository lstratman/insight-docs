using InsightDocs.Abstractions;

namespace InsightDocs.Model.Site;

public class SiteIndex : ILinkTarget
{
    public string LinkText
    {
        get;
        set;
    } = "";
}