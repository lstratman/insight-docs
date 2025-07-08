using InsightDocs.Abstractions;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;
using Microsoft.Extensions.Logging;

namespace InsightDocs.DotNet.Services;

public partial class DotNetPublisher(
    ILoggerFactory loggerFactory, 
    IDotNetLoader dotNetLoader,
    IItemTemplateProvider<DotNetIndex> dotNetIndexTemplate,
    IItemTemplateProvider<DotNetNamespace> dotNetNamespaceTemplate,
    IItemTemplateProvider<DotNetType> dotNetTypeTemplate,
    IItemTemplateProvider<DotNetMethod> dotNetMethodTemplate,
    IItemTemplateProvider<DotNetProperty> dotNetPropertyTemplate,
    IItemTemplateProvider<DotNetField> dotNetFieldTemplate,
    IItemTemplateProvider<DotNetIndexer> dotNetIndexerTemplate,
    IUrlProvider<DotNetIndex> dotNetIndexUrlProvider,
    IUrlProvider<DotNetNamespace> dotNetNamespaceUrlProvider,
    IUrlProvider<DotNetType> dotNetTypeUrlProvider,
    IUrlProvider<DotNetMethod> dotNetMethodUrlProvider,
    IUrlProvider<DotNetProperty> dotNetPropertyUrlProvider,
    IUrlProvider<DotNetField> dotNetFieldUrlProvider,
    IUrlProvider<DotNetIndexer> dotNetIndexerUrlProvider,
    IPublisher publisher,
    IUrlPrefixProvider urlPrefixProvider,
    InsightDocsOptions insightDocsOptions) : IDotNetPublisher
{
    [LoggerMessage(LogLevel.Information, "Publishing topics")]
    public static partial void LogPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics")]
    public static partial void LogFinishedPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Publishing topics for {prefix}")]
    public static partial void LogPublishingTopicsForPrefix(ILogger logger, string prefix);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics for {prefix}")]
    public static partial void LogFinishedPublishingTopicsForPrefix(ILogger logger, string prefix);

    [LoggerMessage(LogLevel.Information, "Publishing topics for namespace {ns}")]
    public static partial void LogPublishingNamespace(ILogger logger, string ns);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics for namespace {ns}")]
    public static partial void LogFinishedPublishingNamespace(ILogger logger, string ns);

    [LoggerMessage(LogLevel.Debug, "Publishing topic for type {type}")]
    public static partial void LogPublishingType(ILogger logger, string type);

    public virtual async Task PublishTopics(TocItem tocRoot, List<string> assemblyPaths, List<string> runtimeAssemblyPaths, Func<Type, bool>? typeFilter)
    {
        ILogger logger = loggerFactory.CreateLogger<DotNetPublisher>();
        DotNetIndex indexData = dotNetLoader.LoadAssemblies(assemblyPaths, runtimeAssemblyPaths, typeFilter);

        if (urlPrefixProvider.UrlPrefix != null)
        {
            LogPublishingTopicsForPrefix(logger, urlPrefixProvider.UrlPrefix);
        }
        else
        {
            LogPublishingTopics(logger);
        }

        string url = dotNetIndexUrlProvider.GetUrl(indexData);
        string html = await dotNetIndexTemplate.GetContent(indexData, url);

        await publisher.Publish(url, html, "text/html", indexData.Title);

        if (insightDocsOptions.EnableParallelism)
        {
            List<TocItem> tocItems = new List<TocItem>();

            await Parallel.ForEachAsync(indexData.Namespaces, new ParallelOptions { MaxDegreeOfParallelism = insightDocsOptions.MaxDegreeOfParallelism }, async (ns, cancellationToken) =>
            {
                TocItem tocItem = await ProcessNamespace(logger, ns, tocRoot);
                tocItems.Add(tocItem);
            });

            tocItems.Sort((a, b) => String.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
            tocRoot.Children.AddRange(tocItems);
        }

        else
        {
            foreach (DotNetNamespace ns in indexData.Namespaces.OrderBy(n => n.FullName))
            {
                TocItem tocItem = await ProcessNamespace(logger, ns, tocRoot);
                tocRoot.Children.Add(tocItem);
            }
        }

        if (urlPrefixProvider.UrlPrefix != null)
        {
            LogFinishedPublishingTopicsForPrefix(logger, urlPrefixProvider.UrlPrefix);
        }

        else
        {
            LogFinishedPublishingTopics(logger);
        }
    }

    public virtual async Task<TocItem> ProcessNamespace(ILogger logger, DotNetNamespace ns, TocItem tocRoot)
    {
        LogPublishingNamespace(logger, ns.FullName);

        string namespaceUrl = dotNetNamespaceUrlProvider.GetUrl(ns);
        TocItem namespaceTocItem = new TocItem(ns.FullName, tocRoot, namespaceUrl);

        string html = await dotNetNamespaceTemplate.GetContent(ns, namespaceUrl);
        await publisher.Publish(namespaceUrl, html, "text/html", ns.Title);

        foreach (DotNetType type in ns.Types.OrderBy(t => t.Name))
        {
            LogPublishingType(logger, type.FullName);

            string typeUrl = dotNetTypeUrlProvider.GetUrl(type);
            TocItem typeTocItem = namespaceTocItem.AddTocItem(type.DisplayName, typeUrl);

            html = await dotNetTypeTemplate.GetContent(type, typeUrl);
            await publisher.Publish(typeUrl, html, "text/html", type.Title);

            if (type.TypeName != "Enum")
            {
                if (type.Constructor != null && type.Constructor.Overloads.Any(o => o.DeclaringType != null && o.DeclaringType.Type != null && o.DeclaringType.Type == type))
                {
                    string constructorUrl = dotNetMethodUrlProvider.GetUrl(type.Constructor);
                    typeTocItem.AddTocItem("Constructors", constructorUrl);

                    html = await dotNetMethodTemplate.GetContent(type.Constructor, constructorUrl);
                    await publisher.Publish(constructorUrl, html, "text/html", type.Constructor.Title);
                }

                if (type.Indexer != null && type.Indexer.Overloads.Any(o => o.DeclaringType != null && o.DeclaringType.Type != null && o.DeclaringType.Type == type))
                {
                    string indexerUrl = dotNetIndexerUrlProvider.GetUrl(type.Indexer);
                    typeTocItem.AddTocItem("Indexer", indexerUrl);

                    html = await dotNetIndexerTemplate.GetContent(type.Indexer, indexerUrl);
                    await publisher.Publish(indexerUrl, html, "text/html", type.Indexer.Title);
                }

                if (type.Methods != null)
                {
                    TocItem? methodsTocItem = null;

                    foreach (DotNetMethod method in type.Methods.Where(m => m.Overloads.Any(o => o.DeclaringType != null && o.DeclaringType.Type == type)).OrderBy(m => m.Name))
                    {
                        methodsTocItem ??= typeTocItem.AddTocItem("Methods");

                        string methodUrl = dotNetMethodUrlProvider.GetUrl(method);
                        methodsTocItem.AddTocItem(method.Name, methodUrl);

                        html = await dotNetMethodTemplate.GetContent(method, methodUrl);
                        await publisher.Publish(methodUrl, html, "text/html", method.Title);
                    }
                }

                if (type.Properties != null)
                {
                    TocItem? propertiesTocItem = null;

                    foreach (DotNetProperty property in type.Properties.Where(m => m.DeclaringType != null && m.DeclaringType.Type == type).OrderBy(p => p.Name))
                    {
                        propertiesTocItem ??= typeTocItem.AddTocItem("Properties");

                        string propertyUrl = dotNetPropertyUrlProvider.GetUrl(property);
                        propertiesTocItem.AddTocItem(property.Name, propertyUrl);

                        html = await dotNetPropertyTemplate.GetContent(property, propertyUrl);
                        await publisher.Publish(propertyUrl, html, "text/html", property.Title);
                    }
                }

                if (type.Fields != null)
                {
                    TocItem? fieldsTocItem = null;

                    foreach (DotNetField field in type.Fields.Where(f => f.DeclaringType != null && f.DeclaringType.Type == type).OrderBy(f => f.Name))
                    {
                        fieldsTocItem ??= typeTocItem.AddTocItem("Fields");

                        string fieldUrl = dotNetFieldUrlProvider.GetUrl(field);
                        fieldsTocItem.AddTocItem(field.Name, fieldUrl);

                        html = await dotNetFieldTemplate.GetContent(field, fieldUrl);
                        await publisher.Publish(fieldUrl, html, "text/html", field.Title);
                    }
                }
            }
        }

        LogFinishedPublishingNamespace(logger, ns.FullName);

        return namespaceTocItem;
    }
}
