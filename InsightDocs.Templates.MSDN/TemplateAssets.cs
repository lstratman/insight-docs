using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MSDN;

public class TemplateAssets
{
    public static ITemplateAsset MsdnCss 
    { 
        get; 
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "msdn.css");

    public static ITemplateAsset DoconsFont
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "docons.woff2"); 
    
    public static ITemplateAsset SegoeUIFont
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "segoe-ui.woff2");

    public static ITemplateAsset SegoeUIRomanVfFont
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "segoe-ui-roman-vf.woff2");

    public static ITemplateAsset HighlightJsScript
    {
        get;
    } = new EmbeddedResourceTemplateAsset(typeof(TemplateAssets).Assembly, "Assets", "highlightjs/highlight.min.js");
}
