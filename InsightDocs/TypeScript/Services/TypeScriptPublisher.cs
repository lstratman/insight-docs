using InsightDocs.Abstractions;
using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using Microsoft.Extensions.Logging;

namespace InsightDocs.TypeScript.Services;

public partial class TypeScriptPublisher(
    ILoggerFactory loggerFactory,
    ITypeScriptLoader typeScriptLoader,
    IItemTemplateProvider<TypeScriptInterface> interfaceTemplate,
    IItemTemplateProvider<TypeScriptEnum> enumTemplate,
    IItemTemplateProvider<TypeScriptTypeAlias> typeAliasTemplate,
    IItemTemplateProvider<TypeScriptMethod> methodTemplate,
    IItemTemplateProvider<TypeScriptProperty> propertyTemplate,
    IItemTemplateProvider<TypeScriptNamespace> namespaceTemplate,
    IUrlProvider<TypeScriptModule> moduleUrlProvider,
    IUrlProvider<TypeScriptTypeDeclaration> typeUrlProvider,
    IUrlProvider<TypeScriptMethod> methodUrlProvider,
    IUrlProvider<TypeScriptProperty> propertyUrlProvider,
    IUrlProvider<TypeScriptNamespace> namespaceUrlProvider,
    IPublisher publisher,
    IUrlPrefixProvider urlPrefixProvider
) : ITypeScriptPublisher
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

    [LoggerMessage(LogLevel.Debug, "Publishing topic for type {module}")]
    public static partial void LogPublishingModule(ILogger logger, string module);

    [LoggerMessage(LogLevel.Debug, "Skipping type {type}")]
    public static partial void LogSkippingLoadingType(ILogger logger, string type);

    [LoggerMessage(LogLevel.Debug, "Skipping type {module}")]
    public static partial void LogSkippingLoadingModule(ILogger logger, string module);

    public virtual async Task PublishTopics(TocItem tocRoot, string typeScriptApiJsonFilePath, Func<TypeScriptTypeDeclaration, bool>? typeFilter, Func<TypeScriptModule, bool>? moduleFilter)
    {
        ILogger logger = loggerFactory.CreateLogger<TypeScriptPublisher>();
        TypeScriptProject api = await typeScriptLoader.LoadApiJson(typeScriptApiJsonFilePath);

        if (urlPrefixProvider.UrlPrefix != null)
        {
            LogPublishingTopicsForPrefix(logger, urlPrefixProvider.UrlPrefix);
        }

        else
        {
            LogPublishingTopics(logger);
        }

        List<TypeScriptTypeDeclaration> types = api.Types?.Values.ToList() ?? [];

        if (api.Modules != null && api.Modules.Count > 0)
        {
            TocItem modulesRoot = tocRoot.AddTocItem("Modules");
            Dictionary<string, TocItem> modulePathFolders = [];

            foreach (TypeScriptModule module in api.Modules.Values.Where(m => m.Exports != null))
            {
                if (moduleFilter != null && !moduleFilter(module))
                {
                    LogSkippingLoadingModule(logger, module.FullName);
                    continue;
                }

                TocItem parentTocItem = GetOrAddModulePathFolders(modulesRoot, modulePathFolders, module.FullName);

                string moduleUrl = moduleUrlProvider.GetUrl(module);
                TocItem tocItem = parentTocItem.AddTocItem(module.ShortName, moduleUrl);

                if (module.Exports != null)
                {
                    types.Remove(module.Exports);
                }

                LogPublishingModule(logger, module.FullName);
                await ProcessType(module.Exports!, moduleUrl, tocItem);
            }

            modulesRoot.SortChildren((a, b) =>
            {
                if (a.Title.EndsWith('/') && !b.Title.EndsWith('/'))
                {
                    return -1;
                }

                else if (!a.Title.EndsWith('/') && b.Title.EndsWith('/'))
                {
                    return 1;
                }

                else
                {
                    return a.Title.CompareTo(b.Title);
                }
            }, true);
        }

        if (types.Count > 0)
        {
            TocItem typesRoot = tocRoot.AddTocItem("Types");
            Dictionary<string, TocItem> namespaceFolders = [];
            string? previousNamespace = null;

            foreach (TypeScriptTypeDeclaration type in types.Where(t => !t.BuiltIn).OrderBy(t => t.FullName))
            {
                if (typeFilter != null && !typeFilter(type))
                {
                    LogSkippingLoadingType(logger, type.FullName);
                    continue;
                }

                if (type is TypeScriptInterface typeScriptInterface && typeScriptInterface.ExportedFromModule != null)
                {
                    continue;
                }

                TocItem? parentTocItem = typesRoot;

                if (type.FullName.Contains('.'))
                {
                    string ns = type.FullName[..type.FullName.LastIndexOf('.')];
                    parentTocItem = null;

                    if (!namespaceFolders.TryGetValue(ns, out parentTocItem))
                    {
                        string namespaceUrl = namespaceUrlProvider.GetUrl(api.Namespaces![ns]);

                        parentTocItem = typesRoot.AddTocItem(ns, namespaceUrl);
                        namespaceFolders[ns] = parentTocItem;

                        if (previousNamespace != null)
                        {
                            LogFinishedPublishingNamespace(logger, previousNamespace);
                        }

                        LogPublishingNamespace(logger, ns);

                        string namespaceHtml = await namespaceTemplate.GetContent(api.Namespaces[ns], namespaceUrl);
                        await publisher.Publish(namespaceUrl, namespaceHtml, "text/html", api.Namespaces![ns].Title);

                        previousNamespace = ns;
                    }
                }

                else if (previousNamespace != null)
                {
                    LogFinishedPublishingNamespace(logger, previousNamespace);
                    previousNamespace = null;
                }

                string typeUrl = typeUrlProvider.GetUrl(type);
                TocItem tocItem = parentTocItem.AddTocItem(type.Name, typeUrl);

                LogPublishingType(logger, type.FullName);
                await ProcessType(type, typeUrl, tocItem);
            }

            if (previousNamespace != null)
            {
                LogFinishedPublishingNamespace(logger, previousNamespace);
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

    public virtual async Task ProcessType(TypeScriptTypeDeclaration type, string url, TocItem tocItem)
    {
        if (type is TypeScriptInterface typeScriptInterface)
        {
            string html = await interfaceTemplate.GetContent(typeScriptInterface, url);
            await publisher.Publish(url, html, "text/html", type.Name);

            // TODO: constructor

            if (typeScriptInterface.Methods != null && typeScriptInterface.Methods.Any(m => m.Value.SourceTypeId == typeScriptInterface.Id))
            {
                TocItem methodsRootTocItem = tocItem.AddTocItem("Methods");

                foreach (TypeScriptMethod method in typeScriptInterface.Methods.Values.Where(m => m.SourceTypeId == typeScriptInterface.Id))
                {
                    string methodUrl = methodUrlProvider.GetUrl(method);
                    TocItem methodTocItem = methodsRootTocItem.AddTocItem(method.Name, methodUrl);

                    string methodHtml = await methodTemplate.GetContent(method, methodUrl);
                    await publisher.Publish(methodUrl, methodHtml, "text/html", method.Name);
                }
            }

            if (typeScriptInterface.Properties != null && typeScriptInterface.Properties.Any(p => p.SourceTypeId == typeScriptInterface.Id))
            {
                TocItem methodsRootTocItem = tocItem.AddTocItem("Properties");

                foreach (TypeScriptProperty property in typeScriptInterface.Properties.Where(m => m.SourceTypeId == typeScriptInterface.Id))
                {
                    string propertyUrl = propertyUrlProvider.GetUrl(property);
                    TocItem methodTocItem = methodsRootTocItem.AddTocItem(property.Name, propertyUrl);

                    string propertyHtml = await propertyTemplate.GetContent(property, propertyUrl);
                    await publisher.Publish(propertyUrl, propertyHtml, "text/html", property.Name);
                }
            }

            // TODO: indexer
        }

        else if (type is TypeScriptEnum typeScriptEnum)
        {
            string html = await enumTemplate.GetContent(typeScriptEnum, url);
            await publisher.Publish(url, html, "text/html", type.Name);
        }

        else if (type is TypeScriptTypeAlias typeScriptTypeAlias)
        {
            string html = await typeAliasTemplate.GetContent(typeScriptTypeAlias, url);
            await publisher.Publish(url, html, "text/html", type.Name);
        }

        else if (type is TypeScriptVariable typeScriptVariable)
        {
            // TODO
        }

        else
        {
            throw new Exception("Unsupported TypeScript type declaration type: " + type.GetType().FullName + ".");
        }
    }

    public static TocItem GetOrAddModulePathFolders(TocItem modulesRoot, Dictionary<string, TocItem> modulePathFolders, string modulePath)
    {
        if (!modulePathFolders.TryGetValue(modulePath, out TocItem? tocItem))
        {
            string[] pathComponents = (modulePath.StartsWith('@') ? modulePath[(modulePath.IndexOf('/') + 1)..] : modulePath).Split('/');

            if (modulePath.StartsWith('@'))
            {
                pathComponents[0] = modulePath[..modulePath.IndexOf('/')] + "/" + pathComponents[0];
            }

            tocItem = modulesRoot;

            string? currentPath = "";

            foreach (string component in pathComponents.Take(pathComponents.Length - 1))
            {
                if (tocItem.Children.All(c => c.Title != component + (String.IsNullOrEmpty(currentPath) ? "" : "/")))
                {
                    tocItem = tocItem.AddTocItem(component + (String.IsNullOrEmpty(currentPath) ? "" : "/"));
                }

                else
                {
                    tocItem = tocItem.Children.First(c => c.Title == component + (String.IsNullOrEmpty(currentPath) ? "" : "/"));
                }

                if (!String.IsNullOrEmpty(currentPath))
                {
                    currentPath += "/";
                }

                currentPath += component;
                modulePathFolders[currentPath] = tocItem;
            }
        }

        return tocItem;
    }
}
