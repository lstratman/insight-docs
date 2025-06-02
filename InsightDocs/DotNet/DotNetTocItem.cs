using System.Reflection;
using InsightDocs.Abstractions;
using InsightDocs.DotNet.Model;
using InsightDocs.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace InsightDocs.DotNet;

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

    public override string Title
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

    [LoggerMessage(LogLevel.Information, "Publishing topics for namespace {ns}")]
    public static partial void LogPublishingNamespace(ILogger logger, string ns);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics for namespace {ns}")]
    public static partial void LogFinishedPublishingNamespace(ILogger logger, string ns);

    [LoggerMessage(LogLevel.Debug, "Publishing topic for type {type}")]
    public static partial void LogPublishingType(ILogger logger, string type);

    [LoggerMessage(LogLevel.Debug, "Skipping type {type}")]
    public static partial void LogSkippingLoadingType(ILogger logger, string type);

    [LoggerMessage(LogLevel.Debug, "Loading type {type}")]
    public static partial void LogLoadingType(ILogger logger, string type);

    protected async Task DotNetExecutor(IServiceProvider serviceProvider)
    {
        if (RootedAssemblyGlobMatchers != null)
        {
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>()!;
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

            Dictionary<string, DotNetNamespace> namespaces = [];
            PathAssemblyResolver pathAssemblyResolver = new(runtimeAssemblyPaths.Concat(assemblyPaths));

            foreach (string assemblyPath in assemblyPaths)
            {
                LogAssemblyLoad(logger, assemblyPath);
                MetadataLoadContext metadataLoadContext = new(pathAssemblyResolver);
                Assembly assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyPath);
                LogFinishedAssemblyLoad(logger, assemblyPath);

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name.StartsWith('<') || type.Name.StartsWith("_Closure$") || type.Name.StartsWith("VB$StateMachine_"))
                    {
                        LogSkippingLoadingType(logger, type.FullName!);
                        continue;
                    }

                    if (TypeFilter != null && !TypeFilter(type))
                    {
                        LogSkippingLoadingType(logger, type.FullName!);
                        continue;
                    }

                    LogLoadingType(logger, type.FullName!);

                    string ns = type.Namespace ?? "";

                    if (!namespaces.TryGetValue(ns, out DotNetNamespace? namespaceMetadata))
                    {
                        namespaceMetadata = DotNetNamespace.Resolve(ns);
                        namespaces[ns] = namespaceMetadata;
                    }

                    namespaceMetadata.Types.Add(DotNetType.Resolve(type, serviceProvider));
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
            IItemTemplateProvider<DotNetProperty> dotNetPropertyTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetProperty>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetProperty.");
            IItemTemplateProvider<DotNetField> dotNetFieldTemplate = serviceProvider.GetService<IItemTemplateProvider<DotNetField>>() ?? throw new Exception("No IItemTemplateProvider service was registered for DotNetField.");
            IUrlProvider<DotNetIndex> dotNetIndexUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetIndex>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetIndex.");
            IUrlProvider<DotNetNamespace> dotNetNamespaceUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetNamespace>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetNamespace.");
            IUrlProvider<DotNetType> dotNetTypeUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetType>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetType.");
            IUrlProvider<DotNetMethod> dotNetMethodUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetMethod>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetMethod.");
            IUrlProvider<DotNetProperty> dotNetPropertyUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetProperty>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetProperty.");
            IUrlProvider<DotNetField> dotNetFieldUrlProvider = serviceProvider.GetService<IUrlProvider<DotNetField>>() ?? throw new Exception("No IUrlProvider service was registered for DotNetField.");
            IPublisher publisher = serviceProvider.GetService<IPublisher>() ?? throw new Exception("No IPublisher service was registered.");

            byte[] html = await dotNetIndexTemplate.GetContent(indexData);
            string? urlPrefix = FullUrlPrefix;

            await publisher.Publish(dotNetIndexUrlProvider.GetUrl(indexData, urlPrefix), html);

            foreach (DotNetNamespace ns in indexData.Namespaces.OrderBy(n => n.FullName))
            {
                LogPublishingNamespace(logger, ns.FullName);

                string namespaceUrl = dotNetNamespaceUrlProvider.GetUrl(ns, urlPrefix);
                TocItem namespaceTocItem = AddTocItem(ns.FullName, namespaceUrl);

                html = await dotNetNamespaceTemplate.GetContent(ns);
                await publisher.Publish(namespaceUrl, html);

                foreach (DotNetType type in ns.Types.OrderBy(t => t.Name))
                {
                    LogPublishingType(logger, type.FullName);

                    string typeUrl = dotNetTypeUrlProvider.GetUrl(type, urlPrefix);
                    TocItem typeTocItem = namespaceTocItem.AddTocItem(type.DisplayName, typeUrl);

                    html = await dotNetTypeTemplate.GetContent(type);
                    await publisher.Publish(typeUrl, html);

                    if (type.Methods != null)
                    {
                        TocItem? methodsTocItem = null;

                        foreach (DotNetMethod method in type.Methods.Where(m => m.Overloads.Any(o => o.DeclaringType != null && o.DeclaringType.Type == type)))
                        {
                            methodsTocItem ??= typeTocItem.AddTocItem("Methods");

                            string methodUrl = dotNetMethodUrlProvider.GetUrl(method, urlPrefix);
                            methodsTocItem.AddTocItem(method.Name, methodUrl);

                            html = await dotNetMethodTemplate.GetContent(method);
                            await publisher.Publish(methodUrl, html);
                        }
                    }

                    if (type.Properties != null)
                    {
                        TocItem? propertiesTocItem = null;

                        foreach (DotNetProperty property in type.Properties.Where(m => m.DeclaringType != null && m.DeclaringType.Type == type))
                        {
                            propertiesTocItem ??= typeTocItem.AddTocItem("Properties");

                            string propertyUrl = dotNetPropertyUrlProvider.GetUrl(property, urlPrefix);
                            propertiesTocItem.AddTocItem(property.Name, propertyUrl);

                            html = await dotNetPropertyTemplate.GetContent(property);
                            await publisher.Publish(propertyUrl, html);
                        }
                    }

                    if (type.Fields != null)
                    {
                        TocItem? fieldsTocItem = null;

                        foreach (DotNetField field in type.Fields.Where(f => f.DeclaringType != null && f.DeclaringType.Type == type))
                        {
                            fieldsTocItem ??= typeTocItem.AddTocItem("Fields");

                            string fieldUrl = dotNetFieldUrlProvider.GetUrl(field, urlPrefix);
                            fieldsTocItem.AddTocItem(field.Name, fieldUrl);

                            html = await dotNetFieldTemplate.GetContent(field);
                            await publisher.Publish(fieldUrl, html);
                        }
                    }
                }

                LogFinishedPublishingNamespace(logger, ns.FullName);
            }

            LogFinishedPublishingTopics(logger);
        }

        await base.Execute(serviceProvider);
    }
}