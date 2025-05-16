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
    public Matcher? AssemblyGlobMatcher;
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

    public async Task DotNetExecutor(IServiceProvider serviceProvider)
    {
        if (AssemblyGlobMatcher != null || RootedAssemblyGlobMatchers != null)
        {
            using ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>()!;
            
            ILogger logger = loggerFactory.CreateLogger("DotNet");
            List<string> assemblyPaths = [];

            if (AssemblyGlobMatcher != null)
            {
                assemblyPaths.AddRange(AssemblyGlobMatcher.GetResultsInFullPath(Directory.GetCurrentDirectory()));
            }

            if (RootedAssemblyGlobMatchers != null)
            {
                foreach (KeyValuePair<string, Matcher> matcherAndRoot in RootedAssemblyGlobMatchers)
                {
                    assemblyPaths.AddRange(matcherAndRoot.Value.GetResultsInFullPath(matcherAndRoot.Key));
                }
            }

            List<Assembly> assemblies = [];
            Dictionary<string, DotNetNamespace> namespaces = [];

            PathAssemblyResolver pathAssemblyResolver = 
                new(
                    // Directory
                    //     .GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll")
                    Directory
                        .GetFiles(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319", "*.dll")
                        // .Concat(Directory.GetFiles(@"C:\Windows\assembly\GAC_MSIL\System.Management.Automation\1.0.0.0__31bf3856ad364e35", "*.dll"))
                        // .Concat(Directory.GetFiles(@"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\PresentationFramework\v4.0_4.0.0.0__31bf3856ad364e35", "*.dll"))
                        .Concat(assemblyPaths)
                );

            foreach (string assemblyPath in assemblyPaths)
            {
                LogAssemblyLoad(logger, assemblyPath);

                MetadataLoadContext metadataLoadContext = new(pathAssemblyResolver);
                Assembly assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyPath);

                assemblies.Add(assembly);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name.StartsWith('<') || type.Name.StartsWith("_Closure$"))
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
            
            IItemTemplateProvider<DotNetIndex> dotNetIndexTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetIndex>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetIndex.");
            IItemTemplateProvider<DotNetNamespace> dotNetNamespaceTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetNamespace>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetNamespace.");
            IItemTemplateProvider<DotNetType> dotNetTypeTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetType>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetType.");
            IUrlProvider<DotNetIndex> dotNetIndexUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetIndex>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetIndex.");
            IUrlProvider<DotNetNamespace> dotNetNamespaceUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetNamespace>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetNamespace.");
            IUrlProvider<DotNetType> dotNetTypeUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetType>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetType.");
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
                }
            }
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

    public static TocItem IncludeDotNetAssemblies(this TocItem tocItem, string glob)
    {
        if (tocItem is not DotNetTocItem dotNetTocItem)
        {
            dotNetTocItem = new DotNetTocItem(tocItem);
        }

        if (Path.IsPathRooted(glob))
        {
            string root = Path.GetPathRoot(glob)!;

            dotNetTocItem.RootedAssemblyGlobMatchers ??= new Dictionary<string, Matcher>();

            if (!dotNetTocItem.RootedAssemblyGlobMatchers.TryGetValue(root, out Matcher? matcher))
            {
                matcher = new Matcher();
                dotNetTocItem.RootedAssemblyGlobMatchers[root] = matcher;
            }

            matcher.AddInclude(glob.Substring(root.Length));
        }

        else 
        {
            dotNetTocItem.AssemblyGlobMatcher ??= new Matcher();
            dotNetTocItem.AssemblyGlobMatcher.AddInclude(glob);
        }

        return dotNetTocItem;
    }

    public static TocItem ExcludeDotNetAssemblies(this TocItem tocItem, string glob)
    {
        if (tocItem is not DotNetTocItem dotNetTocItem)
        {
            dotNetTocItem = new DotNetTocItem(tocItem);
        }

        if (Path.IsPathRooted(glob))
        {
            string root = Path.GetPathRoot(glob)!;

            dotNetTocItem.RootedAssemblyGlobMatchers ??= [];

            if (!dotNetTocItem.RootedAssemblyGlobMatchers.TryGetValue(root, out Matcher? matcher))
            {
                matcher = new Matcher();
                dotNetTocItem.RootedAssemblyGlobMatchers[root] = matcher;
            }

            matcher.AddExclude(glob.Substring(root.Length));
        }

        else 
        {
            dotNetTocItem.AssemblyGlobMatcher ??= new Matcher();
            dotNetTocItem.AssemblyGlobMatcher.AddExclude(glob);
        }

        return dotNetTocItem;
    }
}