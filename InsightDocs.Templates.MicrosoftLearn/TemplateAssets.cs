using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MicrosoftLearn;

public class TemplateAssets
{
    public static IAsset MicrosoftLearnCss
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/microsoft-learn.css");

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

    public static IAsset CommonScript
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "js/common.js");

    public static IAsset SiteIndexCss
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/site-index.css");

    public static IAsset OpenApiScript
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "js/openapi.js");

    public static IAsset OpenApiCss
    {
        get;
    } = new EmbeddedResourceAsset(typeof(TemplateAssets).Assembly, "Assets", "css/openapi.css");
}
