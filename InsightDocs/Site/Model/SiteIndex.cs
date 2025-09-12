using InsightDocs.Abstractions;

namespace InsightDocs.Site.Model;

public class SiteIndex(string title, string? initialUrl, SiteToc toc, IAsset? faviconAsset = null, bool useDynamicToc = false, string? dynamicTocUrl = null) : ILinkTarget
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

    public SiteToc Toc
    {
        get;
        set;
    } = toc;

    public string? InitialUrl
    {
        get;
        set;
    } = initialUrl;

    public IAsset? FaviconAsset
    {
        get;
        set;
    } = faviconAsset;

    public bool UseDynamicToc
    {
        get;
        set;
    } = useDynamicToc;

    public string? DynamicTocUrl
    {
        get;
        set;
    } = dynamicTocUrl;
}