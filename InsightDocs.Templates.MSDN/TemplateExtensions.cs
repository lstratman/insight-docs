using InsightDocs.Abstractions;
using InsightDocs.DotNet.Model;
using InsightDocs.Extensions;
using InsightDocs.Site.Model;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Templates.MSDN;

public static class TemplateExtensions
{
    public static InsightDocsBuilder UseMSDNTemplate(this InsightDocsBuilder builder)
    {
        builder.RegisterRazorItemTemplate<SiteIndex, Site.SiteIndex>();
        builder.RegisterRazorItemTemplate<DotNetIndex, DotNet.DotNetIndex>();
        builder.RegisterRazorItemTemplate<DotNetNamespace, DotNet.DotNetNamespace>();
        builder.RegisterRazorItemTemplate<DotNetType, DotNet.DotNetType>();
        builder.RegisterRazorItemTemplate<DotNetMethod, DotNet.DotNetMethod>();
        builder.RegisterRazorItemTemplate<DotNetProperty, DotNet.DotNetProperty>();
        builder.RegisterRazorItemTemplate<DotNetField, DotNet.DotNetField>();
        
        builder.Services.AddSingleton<ITemplateAssetProvider, TemplateAssetProvider>();

        return builder;
    }
}