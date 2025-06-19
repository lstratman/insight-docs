using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace InsightDocs.DotNet;

public class DotNetOptions
{
    public ErrorBehavior UnsupportedXmlDocTagBehavior
    {
        get;
        set;
    } = ErrorBehavior.Error;

    public ErrorBehavior UnresolvedXmlDocLinkBehavior
    {
        get;
        set;
    } = ErrorBehavior.Error;

    public bool OmitPrivateMembers
    {
        get;
        set;
    } = true;

    public bool ResolveMicrosoftDocsUrls
    {
        get;
        set;
    } = true;

    // TODO: add option to omit protected members for external types
}

public static class DotNetExtensions
{
    public static InsightDocsBuilder UseDotNet(this InsightDocsBuilder builder, Action<DotNetOptions>? optionsFactory = null)
    {
        builder.Services.AddScoped<IXmlDocProcessor, XmlDocProcessor>();
        builder.Services.AddScoped<IXmlDocUrlResolver, XmlDocUrlResolver>();
        builder.Services.AddScoped<IDotNetLoader, DotNetLoader>();
        builder.Services.AddScoped<IDotNetPublisher, DotNetPublisher>();
        builder.Services.AddScoped<IMicrosoftDocsUrlResolver, MicrosoftDocsUrlResolver>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                DotNetOptions options = new();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new DotNetOptions());
        }

        return builder;
    }
}

public static class DotNetTocItemExtensions
{
    public static TocItem IncludeDotNetTypes(this TocItem tocItem, Func<Type, bool> filter)
    {
        if (tocItem is not DotNetTocItem dotNetTocItem)
        {
            dotNetTocItem = new DotNetTocItem(tocItem);
        }

        dotNetTocItem.TypeFilter = filter;
        return dotNetTocItem;
    }

    public static TocItem IncludeDotNetRuntimeAssemblies(this TocItem tocItem, string glob)
    {
        if (tocItem is not DotNetTocItem dotNetTocItem)
        {
            dotNetTocItem = new DotNetTocItem(tocItem);
        }

        if (!Path.IsPathRooted(glob))
        {
            glob = Path.Combine(AppContext.BaseDirectory, glob);
        }

        string root = Path.GetPathRoot(glob)!;

        dotNetTocItem.RootedRuntimeAssemblyGlobMatchers ??= [];

        if (!dotNetTocItem.RootedRuntimeAssemblyGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            dotNetTocItem.RootedRuntimeAssemblyGlobMatchers[root] = matcher;
        }

        matcher.AddInclude(glob[root.Length..]);

        return dotNetTocItem;
    }

    public static TocItem ExcludeDotNetRuntimeAssemblies(this TocItem tocItem, string glob)
    {
        if (tocItem is not DotNetTocItem dotNetTocItem)
        {
            dotNetTocItem = new DotNetTocItem(tocItem);
        }

        if (!Path.IsPathRooted(glob))
        {
            glob = Path.Combine(AppContext.BaseDirectory, glob);
        }

        string root = Path.GetPathRoot(glob)!;

        dotNetTocItem.RootedRuntimeAssemblyGlobMatchers ??= [];

        if (!dotNetTocItem.RootedRuntimeAssemblyGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            dotNetTocItem.RootedRuntimeAssemblyGlobMatchers[root] = matcher;
        }

        matcher.AddExclude(glob[root.Length..]);

        return dotNetTocItem;
    }

    public static TocItem IncludeDotNetAssemblies(this TocItem tocItem, string glob)
    {
        if (tocItem is not DotNetTocItem dotNetTocItem)
        {
            dotNetTocItem = new DotNetTocItem(tocItem);
        }

        if (!Path.IsPathRooted(glob))
        {
            glob = Path.Combine(AppContext.BaseDirectory, glob);
        }

        string root = Path.GetPathRoot(glob)!;

        dotNetTocItem.RootedAssemblyGlobMatchers ??= [];

        if (!dotNetTocItem.RootedAssemblyGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            dotNetTocItem.RootedAssemblyGlobMatchers[root] = matcher;
        }

        matcher.AddInclude(glob[root.Length..]);

        return dotNetTocItem;
    }

    public static TocItem ExcludeDotNetAssemblies(this TocItem tocItem, string glob)
    {
        if (tocItem is not DotNetTocItem dotNetTocItem)
        {
            dotNetTocItem = new DotNetTocItem(tocItem);
        }

        if (!Path.IsPathRooted(glob))
        {
            glob = Path.Combine(AppContext.BaseDirectory, glob);
        }

        string root = Path.GetPathRoot(glob)!;

        dotNetTocItem.RootedAssemblyGlobMatchers ??= [];

        if (!dotNetTocItem.RootedAssemblyGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            dotNetTocItem.RootedAssemblyGlobMatchers[root] = matcher;
        }

        matcher.AddExclude(glob[root.Length..]);

        return dotNetTocItem;
    }
}