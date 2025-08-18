using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace InsightDocs.TypeScript;

public class TypeScriptOptions
{
    public bool ResolveMDNUrls
    {
        get;
        set;
    } = true;
}

public static class TypeScriptExtensions
{
    public static InsightDocsBuilder UseTypeScript(this InsightDocsBuilder builder, Action<TypeScriptOptions>? optionsFactory = null)
    {
        builder.Services.AddScoped<ITypeScriptLoader, TypeScriptLoader>();
        builder.Services.AddScoped<ITypeScriptPublisher, TypeScriptPublisher>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                TypeScriptOptions options = new();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new TypeScriptOptions());
        }

        return builder;
    }
}

public static class TypeScriptTocItemExtensions
{
    public static TocItem IncludeTypeScriptDefinitionFiles(this TocItem tocItem, string glob)
    {
        if (tocItem is not TypeScriptTocItem typeScriptTocItem)
        {
            typeScriptTocItem = new TypeScriptTocItem(tocItem);
        }

        if (!Path.IsPathRooted(glob))
        {
            glob = Path.Combine(AppContext.BaseDirectory, glob);
            glob = Path.GetFullPath(glob);
        }

        string root = Path.GetPathRoot(glob)!;

        typeScriptTocItem.RootedDefinitionGlobMatchers ??= [];

        if (!typeScriptTocItem.RootedDefinitionGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            typeScriptTocItem.RootedDefinitionGlobMatchers[root] = matcher;
        }

        matcher.AddInclude(glob[root.Length..]);

        return typeScriptTocItem;
    }

    public static TocItem ExcludeTypeScriptDefinitionFiles(this TocItem tocItem, string glob)
    {
        if (tocItem is not TypeScriptTocItem typeScriptTocItem)
        {
            typeScriptTocItem = new TypeScriptTocItem(tocItem);
        }

        if (!Path.IsPathRooted(glob))
        {
            glob = Path.Combine(AppContext.BaseDirectory, glob);
            glob = Path.GetFullPath(glob);
        }

        string root = Path.GetPathRoot(glob)!;

        typeScriptTocItem.RootedDefinitionGlobMatchers ??= [];

        if (!typeScriptTocItem.RootedDefinitionGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            typeScriptTocItem.RootedDefinitionGlobMatchers[root] = matcher;
        }

        matcher.AddExclude(glob[root.Length..]);

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
