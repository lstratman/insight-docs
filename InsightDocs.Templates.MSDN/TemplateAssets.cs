using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MSDN;

public class TemplateAssets
{
    public static IAsset MsdnCss 
    { 
        get; 
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/msdn.css");

    public static IAsset DoconsFont
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/docons.woff2"); 
    
    public static IAsset SegoeUIFont
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/segoe-ui.woff2");

    public static IAsset SegoeUIRomanVfFont
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/segoe-ui-roman-vf.woff2");

    public static IAsset HighlightJsScript
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "js/highlight.min.js");

    public static IAsset SiteIndexScript
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "js/site-index.js");

    public static IAsset DotNetScript
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "js/dotnet.js");

    public static IAsset SiteIndexCss
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/site-index.css");
}
