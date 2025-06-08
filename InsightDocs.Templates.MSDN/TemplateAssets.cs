using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MSDN;

public class TemplateAssets
{
    public static ITemplateAsset MsdnCss 
    { 
        get; 
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "css/msdn.css");

    public static ITemplateAsset DoconsFont
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "css/docons.woff2"); 
    
    public static ITemplateAsset SegoeUIFont
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "css/segoe-ui.woff2");

    public static ITemplateAsset SegoeUIRomanVfFont
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "css/segoe-ui-roman-vf.woff2");

    public static ITemplateAsset HighlightJsScript
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "js/highlight.min.js");

    public static ITemplateAsset SiteIndexScript
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "js/site-index.js");

    public static ITemplateAsset SiteIndexCss
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "css/site-index.css");
}
