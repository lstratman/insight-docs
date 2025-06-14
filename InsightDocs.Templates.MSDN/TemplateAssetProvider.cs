using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MSDN;

public class TemplateAssetProvider : IAssetProvider
{
    public List<IAsset> GetAssets()
    {
        return
        [
            TemplateAssets.MsdnCss,
            TemplateAssets.DoconsFont,
            TemplateAssets.SegoeUIFont,
            TemplateAssets.SegoeUIRomanVfFont,
            TemplateAssets.HighlightJsScript,
            TemplateAssets.SiteIndexScript,
            TemplateAssets.DotNetScript,
            TemplateAssets.SiteIndexCss
        ];
    }
}