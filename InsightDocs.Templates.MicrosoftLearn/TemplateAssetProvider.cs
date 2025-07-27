using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MicrosoftLearn;

public class TemplateAssetProvider : IAssetProvider
{
    public List<IAsset> GetAssets()
    {
        return
        [
            TemplateAssets.MicrosoftLearnCss,
            TemplateAssets.DoconsFont,
            TemplateAssets.SegoeUIFont,
            TemplateAssets.SegoeUIRomanVfFont,
            TemplateAssets.HighlightJsScript,
            TemplateAssets.SiteIndexScript,
            TemplateAssets.DotNetScript,
            TemplateAssets.SiteIndexCss,
            TemplateAssets.CommonScript
        ];
    }
}