using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MSDN;

public class TemplateAssetProvider : ITemplateAssetProvider
{
    public List<ITemplateAsset> GetAssets()
    {
        return
        [
            TemplateAssets.MsdnCss,
            TemplateAssets.DoconsFont,
            TemplateAssets.SegoeUIFont,
            TemplateAssets.SegoeUIRomanVfFont,
            TemplateAssets.HighlightJsScript
        ];
    }
}