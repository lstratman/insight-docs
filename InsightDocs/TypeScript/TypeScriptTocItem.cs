using InsightDocs.Abstractions;
using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace InsightDocs.TypeScript;

public class TypeScriptTocItem : TocItem
{
    protected TocItem _baseTocItem;
    public Dictionary<string, Matcher>? RootedDefinitionGlobMatchers;

    public TypeScriptTocItem(TocItem baseTocItem)
    {
        _baseTocItem = baseTocItem;
        RegisterExecutor(TypeScriptExecutor);
    }

    public Func<TypeScriptTypeDeclaration, bool>? TypeFilter
    {
        get;
        set;
    }

    public Func<TypeScriptModule, bool>? ModuleFilter
    {
        get;
        set;
    }

    public bool ExcludePackageRoot
    {
        get;
        set;
    } = false;

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

    public override Dictionary<string, string>? AdditionalData
    {
        get
        {
            return _baseTocItem.AdditionalData;
        }

        set
        {
            _baseTocItem.AdditionalData = value;
        }
    }

    public override void RegisterExecutor(Func<IServiceProvider, Task> executor)
    {
        _baseTocItem.RegisterExecutor(executor);
    }

    public override void RegisterPostExecutor(Func<IServiceProvider, Task> executor)
    {
        _baseTocItem.RegisterPostExecutor(executor);
    }

    protected async Task TypeScriptExecutor(IServiceProvider serviceProvider)
    {
        if (RootedDefinitionGlobMatchers != null)
        {
            List<string> definitionFilePaths = [];

            foreach (KeyValuePair<string, Matcher> matcherAndRoot in RootedDefinitionGlobMatchers)
            {
                definitionFilePaths.AddRange(matcherAndRoot.Value.GetResultsInFullPath(matcherAndRoot.Key));
            }

            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
                prefixProvider.UrlPrefix = FullUrlPrefix;

                ITypeScriptPublisher typeScriptPublisher = serviceScope.ServiceProvider.GetRequiredService<ITypeScriptPublisher>();
                await typeScriptPublisher.PublishTopics(this, definitionFilePaths, TypeFilter, ModuleFilter, ExcludePackageRoot);
            }
        }
    }
}
