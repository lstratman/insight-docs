using InsightDocs.Abstractions;
using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using Microsoft.Extensions.Logging;

namespace InsightDocs.TypeScript.Services;

public class TypeScriptPublisher(
    ILoggerFactory loggerFactory,
    ITypeScriptLoader typeScriptLoader,
    IItemTemplateProvider<TypeScriptInterface> interfaceTemplate,
    IItemTemplateProvider<TypeScriptEnum> enumTemplate,
    IItemTemplateProvider<TypeScriptTypeAlias> typeAliasTemplate,
    IUrlProvider<TypeScriptModule> moduleUrlProvider,
    IUrlProvider<TypeScriptTypeDeclaration> typeUrlProvider,
    IPublisher publisher
) : ITypeScriptPublisher
{
    public virtual async Task PublishTopics(TocItem tocRoot, string typeScriptApiJsonFilePath, Func<TypeScriptTypeDeclaration, bool>? typeFilter, Func<TypeScriptModule, bool>? moduleFilter)
    {
        TypeScriptProject api = await typeScriptLoader.LoadApiJson(typeScriptApiJsonFilePath, typeFilter, moduleFilter);
        List<TypeScriptTypeDeclaration> types = api.Types?.Values.ToList() ?? [];

        if (api.Modules != null && api.Modules.Count > 0)
        {
            TocItem modulesRoot = tocRoot.AddTocItem("Modules");
            Dictionary<string, TocItem> modulePathFolders = [];

            foreach (TypeScriptModule module in api.Modules.Values.Where(m => m.Exports != null).OrderBy(m => m.Name))
            {
                TocItem parentTocItem = GetOrAddModulePathFolders(modulesRoot, modulePathFolders, module.FullName);

                string moduleUrl = moduleUrlProvider.GetUrl(module);
                TocItem tocItem = parentTocItem.AddTocItem(module.ShortName, moduleUrl);

                if (module.Exports != null)
                {
                    types.Remove(module.Exports);
                }

                await ProcessType(module.Exports!, moduleUrl, tocItem);
            }
        }

        if (types.Count > 0)
        {
            TocItem typesRoot = tocRoot.AddTocItem("Types");
            Dictionary<string, TocItem> namespaceFolders = [];

            foreach (TypeScriptTypeDeclaration type in types.Where(t => !t.BuiltIn).OrderBy(t => t.FullName))
            {
                TocItem? parentTocItem = typesRoot;

                if (type.FullName.Contains('.'))
                {
                    string ns = type.FullName[..type.FullName.LastIndexOf('.')];
                    parentTocItem = null;

                    if (!namespaceFolders.TryGetValue(ns, out parentTocItem))
                    {
                        parentTocItem = typesRoot.AddTocItem(ns);
                        namespaceFolders[ns] = parentTocItem;
                    }
                }

                string typeUrl = typeUrlProvider.GetUrl(type);
                TocItem tocItem = parentTocItem.AddTocItem(type.Name, typeUrl);

                await ProcessType(type, typeUrl, tocItem);
            }
        }
    }

    public virtual async Task ProcessType(TypeScriptTypeDeclaration type, string url, TocItem tocItem)
    {
        if (type is TypeScriptInterface typeScriptInterface)
        {
            string html = await interfaceTemplate.GetContent(typeScriptInterface);
            await publisher.Publish(url, html, "text/html", type.Title);
        }

        else if (type is TypeScriptEnum typeScriptEnum)
        {
            string html = await enumTemplate.GetContent(typeScriptEnum);
            await publisher.Publish(url, html, "text/html", type.Title);
        }

        else if (type is TypeScriptTypeAlias typeScriptTypeAlias)
        {
            string html = await typeAliasTemplate.GetContent(typeScriptTypeAlias);
            await publisher.Publish(url, html, "text/html", type.Title);
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
                    tocItem = tocItem.AddTocItem(component);
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
