using InsightDocs.Abstractions;
using InsightDocs.Extensions;
using InsightDocs.Model.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Templates.MSDN;

public static class TemplateExtensions
{
    public static Builder UseMSDNTemplate(this Builder builder)
    {
        builder.RegisterRazorItemTemplate<DotNetIndex, DotNet.DotNetIndex>();
        builder.RegisterRazorItemTemplate<DotNetNamespace, DotNet.DotNetNamespace>();
        builder.RegisterRazorItemTemplate<DotNetType, DotNet.DotNetType>();
        builder.RegisterRazorItemTemplate<DotNetMethod, DotNet.DotNetMethod>();
        
        builder.Services.AddSingleton<ITemplateAssetProvider, TemplateAssetProvider>();

        return builder;
    }
}