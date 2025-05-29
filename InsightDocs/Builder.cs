using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsightDocs.Abstractions;
using InsightDocs.Model.Site;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs;

public class Builder(IServiceCollection services)
{
    public TocItem TocRoot
    {
        get;
        private set;
    } = new TocItem("", null);

    public IServiceCollection Services
    {
        get;
        private set;
    } = services;

    public TocItem AddTocItem(string title, string? urlPrefix = null)
    {
        return TocRoot.AddTocItem(title, urlPrefix);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task Execute()
    {
        IServiceProvider serviceProvider = Services.BuildServiceProvider();
        IPublisher publisher = serviceProvider.GetService<IPublisher>() ?? throw new Exception("No IPublisher service was registered.");
        IUrlProvider<SiteTableOfContents> tableOfContentsUrlProvider = serviceProvider.GetService<IUrlProvider<SiteTableOfContents>>() ?? throw new Exception("No IUrlProvider service was registered for SiteTableOfContents.");
        ITemplateAssetProvider? templateAssetProvider = serviceProvider.GetService<ITemplateAssetProvider>();

        await publisher.Initialize();
        await TocRoot.Execute(serviceProvider);

        SiteTableOfContents siteTableOfContents = new(TocRoot);
        string tableOfContentsUrl = tableOfContentsUrlProvider.GetUrl(siteTableOfContents);

        await publisher.Publish(tableOfContentsUrl, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(siteTableOfContents, typeof(SiteTableOfContents), SerializerOptions)));

        if (templateAssetProvider != null)
        {
            await templateAssetProvider.PublishAssets();
        }
    }
}