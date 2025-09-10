using InsightDocs.Abstractions;
using InsightDocs.OpenApi.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.OpenApi;

public partial class OpenApiTocItem : TocItem
{
    protected TocItem _baseTocItem;
    public string? SpecFilePath;

    public OpenApiTocItem(TocItem baseTocItem)
    {
        _baseTocItem = baseTocItem;
        RegisterExecutor(OpenApiExecutor);
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

    protected async Task OpenApiExecutor(IServiceProvider serviceProvider)
    {
        if (!String.IsNullOrEmpty(SpecFilePath))
        {
            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
                prefixProvider.UrlPrefix = FullUrlPrefix;

                IOpenApiPublisher openApiPublisher = serviceScope.ServiceProvider.GetRequiredService<IOpenApiPublisher>();
                await openApiPublisher.PublishTopics(this, SpecFilePath);
            }
        }
    }
}
