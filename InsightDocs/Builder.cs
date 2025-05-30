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
        using (ServiceProvider serviceProvider = Services.BuildServiceProvider())
        {
            IPublisher publisher = serviceProvider.GetService<IPublisher>() ?? throw new Exception("No IPublisher service was registered.");
            IUrlProvider<SiteTableOfContents> tableOfContentsUrlProvider = serviceProvider.GetService<IUrlProvider<SiteTableOfContents>>() ?? throw new Exception("No IUrlProvider service was registered for SiteTableOfContents.");
            IUrlProvider<SiteIndex> siteIndexUrlProvider = serviceProvider.GetService<IUrlProvider<SiteIndex>>() ?? throw new Exception("No IUrlProvider service was registered for SiteIndex.");
            IItemTemplateProvider<SiteIndex> siteIndexTemplateProvider = serviceProvider.GetService<IItemTemplateProvider<SiteIndex>>() ?? throw new Exception("No IItemTemplateProvider service was registered for SiteIndex.");
            ITemplateAssetProvider? templateAssetProvider = serviceProvider.GetService<ITemplateAssetProvider>();
            SiteIndex siteIndex = new();

            await publisher.Initialize();
            await TocRoot.Execute(serviceProvider);

            string siteIndexUrl = siteIndexUrlProvider.GetUrl(siteIndex);
            await publisher.Publish(siteIndexUrl, await siteIndexTemplateProvider.GetContent(siteIndex));

            SiteTableOfContents siteTableOfContents = new(TocRoot);
            string tableOfContentsUrl = tableOfContentsUrlProvider.GetUrl(siteTableOfContents);
            await publisher.Publish(tableOfContentsUrl, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(siteTableOfContents, typeof(SiteTableOfContents), SerializerOptions)));

            if (templateAssetProvider != null)
            {
                await templateAssetProvider.PublishAssets();
            }
        }
    }
}