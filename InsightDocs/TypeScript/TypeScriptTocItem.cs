using InsightDocs.Abstractions;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.TypeScript;

public class TypeScriptTocItem : TocItem
{
    protected TocItem _baseTocItem;

    public TypeScriptTocItem(TocItem baseTocItem)
    {
        _baseTocItem = baseTocItem;
        RegisterExecutor(TypeScriptExecutor);
    }

    public string? TypeScriptApiJsonFilePath
    {
        get;
        set;
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

    protected async Task TypeScriptExecutor(IServiceProvider serviceProvider)
    {
        if (String.IsNullOrEmpty(TypeScriptApiJsonFilePath))
        {
            throw new Exception("TypeScriptApiJsonFilePath must be set before building the TypeScript topics.");
        }

        IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
        {
            IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
            prefixProvider.UrlPrefix = FullUrlPrefix;

            ITypeScriptPublisher typeScriptPublisher = serviceScope.ServiceProvider.GetRequiredService<ITypeScriptPublisher>();
            await typeScriptPublisher.PublishTopics(this, TypeScriptApiJsonFilePath, TypeFilter, ModuleFilter);
        }
    }
}
