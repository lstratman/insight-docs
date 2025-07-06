using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.TypeScript;

public static class TypeScriptExtensions
{
    public static InsightDocsBuilder UseTypeScript(this InsightDocsBuilder builder)
    {
        builder.Services.AddScoped<ITypeScriptLoader, TypeScriptLoader>();
        builder.Services.AddScoped<ITypeScriptPublisher, TypeScriptPublisher>();

        return builder;
    }
}

public static class TypeScriptTocItemExtensions
{
    public static TocItem IncludeTypeScriptApi(this TocItem tocItem, string typeScriptApiJsonFilePath)
    {
        if (tocItem is not TypeScriptTocItem typeScriptTocItem)
        {
            typeScriptTocItem = new TypeScriptTocItem(tocItem);
        }

        if (!Path.IsPathRooted(typeScriptApiJsonFilePath))
        {
            typeScriptApiJsonFilePath = Path.Combine(AppContext.BaseDirectory, typeScriptApiJsonFilePath);
            typeScriptApiJsonFilePath = Path.GetFullPath(typeScriptApiJsonFilePath);
        }

        typeScriptTocItem.TypeScriptApiJsonFilePath = typeScriptApiJsonFilePath;

        return typeScriptTocItem;
    }

    public static TocItem IncludeTypeScriptTypes(this TocItem tocItem, Func<TypeScriptTypeDeclaration, bool> filter)
    {
        if (tocItem is not TypeScriptTocItem typeScriptTocItem)
        {
            typeScriptTocItem = new TypeScriptTocItem(tocItem);
        }

        typeScriptTocItem.TypeFilter = filter;
        return typeScriptTocItem;
    }

    public static TocItem IncludeTypeScriptModules(this TocItem tocItem, Func<TypeScriptModule, bool> filter)
    {
        if (tocItem is not TypeScriptTocItem typeScriptTocItem)
        {
            typeScriptTocItem = new TypeScriptTocItem(tocItem);
        }

        typeScriptTocItem.ModuleFilter = filter;
        return typeScriptTocItem;
    }
}
