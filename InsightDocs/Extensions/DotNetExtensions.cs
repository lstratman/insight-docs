using System.Reflection;
using InsightDocs.Abstractions;
using InsightDocs.Model.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace InsightDocs.Extensions;

public partial class DotNetTocItem : TocItem
{
    protected TocItem _baseTocItem;
    public Dictionary<string, Matcher>? RootedAssemblyGlobMatchers;
    public Dictionary<string, Matcher>? RootedRuntimeAssemblyGlobMatchers;
    public Func<Type, bool>? TypeFilter;

    public DotNetTocItem(TocItem baseTocItem)
    {
        _baseTocItem = baseTocItem;
        RegisterExecutor(DotNetExecutor);
    }

    public override List<TocItem> Children
    {
        get
        {
            return _baseTocItem.Children;
        }
    }

    public override TocItem? Parent
    {
        get
        {
            return _baseTocItem.Parent;
        }

        set
        {
            _baseTocItem.Parent = value;
        }
    }

    public override string? Title
    {
        get
        {
            return _baseTocItem.Title;
        }

        set
        {
            _baseTocItem.Title = value;
        }
    }

    public override string? UrlPrefix
    {
        get
        {
            return _baseTocItem.UrlPrefix;
        }

        set
        {
            _baseTocItem.UrlPrefix = value;
        }
    }

    public override void RegisterExecutor(Func<IServiceProvider, Task> executor)
    {
        _baseTocItem.RegisterExecutor(executor);
    }

    [LoggerMessage(LogLevel.Information, "Loading {assemblyPath}")]
    public static partial void LogAssemblyLoad(ILogger logger, string assemblyPath);

    [LoggerMessage(LogLevel.Information, "Finished loading {assemblyPath}")]
    public static partial void LogFinishedAssemblyLoad(ILogger logger, string assemblyPath);

    [LoggerMessage(LogLevel.Information, "Publishing topics")]
    public static partial void LogPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics")]
    public static partial void LogFinishedPublishingTopics(ILogger logger);

    public async Task DotNetExecutor(IServiceProvider serviceProvider)
    {
        if (RootedAssemblyGlobMatchers != null)
        {
            using ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>()!;
            ILogger logger = loggerFactory.CreateLogger("DotNet");
            List<string> assemblyPaths = [];
            List<string> runtimeAssemblyPaths = [];
            
            foreach (KeyValuePair<string, Matcher> matcherAndRoot in RootedAssemblyGlobMatchers)
            {
                assemblyPaths.AddRange(matcherAndRoot.Value.GetResultsInFullPath(matcherAndRoot.Key));
            }

            if (RootedRuntimeAssemblyGlobMatchers != null)
            {
                foreach (KeyValuePair<string, Matcher> matcherAndRoot in RootedRuntimeAssemblyGlobMatchers)
                {
                    runtimeAssemblyPaths.AddRange(matcherAndRoot.Value.GetResultsInFullPath(matcherAndRoot.Key));
                }
            }

            List<Assembly> assemblies = [];
            Dictionary<string, DotNetNamespace> namespaces = [];

            PathAssemblyResolver pathAssemblyResolver = new(runtimeAssemblyPaths.Concat(assemblyPaths));

            foreach (string assemblyPath in assemblyPaths)
            {
                LogAssemblyLoad(logger, assemblyPath);
                MetadataLoadContext metadataLoadContext = new(pathAssemblyResolver);
                Assembly assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyPath);
                LogFinishedAssemblyLoad(logger, assemblyPath);

                assemblies.Add(assembly);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name.StartsWith('<') || type.Name.StartsWith("_Closure$") || type.Name.StartsWith("VB$StateMachine_"))
                    {
                        continue;
                    }

                    if (TypeFilter != null && !TypeFilter(type))
                    {
                        continue;
                    }

                    string ns = type.Namespace ?? "";

                    if (!namespaces.TryGetValue(ns, out DotNetNamespace? namespaceMetadata))
                    {
                        namespaceMetadata = DotNetNamespace.Resolve(ns);
                        namespaces[ns] = namespaceMetadata;
                    }

                    namespaceMetadata.Types.Add(DotNetType.Resolve(type));
                }
            }

            DotNetIndex indexData = new()
            {
                Namespaces = [.. namespaces.Values]
            };
            
            LogPublishingTopics(logger);

            IItemTemplateProvider<DotNetIndex> dotNetIndexTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetIndex>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetIndex.");
            IItemTemplateProvider<DotNetNamespace> dotNetNamespaceTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetNamespace>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetNamespace.");
            IItemTemplateProvider<DotNetType> dotNetTypeTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetType>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetType.");
            IItemTemplateProvider<DotNetMethod> dotNetMethodTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetMethod>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetMethod.");
            IUrlProvider<DotNetIndex> dotNetIndexUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetIndex>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetIndex.");
            IUrlProvider<DotNetNamespace> dotNetNamespaceUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetNamespace>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetNamespace.");
            IUrlProvider<DotNetType> dotNetTypeUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetType>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetType.");
            IUrlProvider<DotNetMethod> dotNetMethodUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetMethod>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetMethod.");
            IPublisher publisher = serviceProvider.GetService<IPublisher>() ?? throw new Exception("No IPublisher service was registered.");

            byte[] html = await dotNetIndexTemplate.GetContent(indexData);
            string? urlPrefix = FullUrlPrefix;

            await publisher.Publish(dotNetIndexUrlProvider.GetUrl(indexData, urlPrefix), html);

            foreach (DotNetNamespace ns in indexData.Namespaces)
            {
                html = await dotNetNamespaceTemplate.GetContent(ns);
                await publisher.Publish(dotNetNamespaceUrlProvider.GetUrl(ns, urlPrefix), html);

                foreach (DotNetType type in ns.Types)
                {
                    html = await dotNetTypeTemplate.GetContent(type);
                    await publisher.Publish(dotNetTypeUrlProvider.GetUrl(type, urlPrefix), html);

                    if (type.Methods != null)
                    {
                        foreach (DotNetMethod method in type.Methods.Where(m => m.DeclaringType != null && m.DeclaringType.Type == type))
                        {
                            html = await dotNetMethodTemplate.GetContent(method);
                            await publisher.Publish(dotNetMethodUrlProvider.GetUrl(method, urlPrefix), html);
                        }
                    }
                }
            }

            LogFinishedPublishingTopics(logger);
        }

        await base.Execute(serviceProvider);
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