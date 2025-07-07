using InsightDocs.Abstractions;
using InsightDocs.DotNet.Model;
using InsightDocs.Extensions;
using InsightDocs.Markdown.Model;
using InsightDocs.Site.Model;
using InsightDocs.TypeScript.Model;
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
        builder.RegisterRazorItemTemplate<DotNetIndexer, DotNet.DotNetIndexer>();
        builder.RegisterRazorItemTemplate<MarkdownFile, Markdown.MarkdownFile>();
        builder.RegisterRazorItemTemplate<TypeScriptInterface, TypeScript.TypeScriptInterface>();
        builder.RegisterRazorItemTemplate<TypeScriptEnum, TypeScript.TypeScriptEnum>();
        builder.RegisterRazorItemTemplate<TypeScriptTypeAlias, TypeScript.TypeScriptTypeAlias>();
        builder.RegisterRazorItemTemplate<TypeScriptMethod, TypeScript.TypeScriptMethod>();

        builder.Services.AddSingleton<IAssetProvider, TemplateAssetProvider>();

        return builder;
    }
}