using InsightDocs.Abstractions;
using InsightDocs.Services;
using InsightDocs.Site.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InsightDocs;

public class InsightDocsBuilder
{
    public static InsightDocsBuilder Create()
    {
        return new InsightDocsBuilder();
    }

    private InsightDocsBuilder()
    {
        Services = new ServiceCollection();
        Services.AddScoped<IUrlPrefixProvider, UrlPrefixProvider>();
    }

    public TocItem TocRoot
    {
        get;
        private set;
    } = new TocItem("", null);

    public IServiceCollection Services
    {
        get;
        private set;
    }

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
            IUrlProvider<SiteToc> tableOfContentsUrlProvider = serviceProvider.GetService<IUrlProvider<SiteToc>>() ?? throw new Exception("No IUrlProvider service was registered for SiteToc.");
            IUrlProvider<SiteIndex> siteIndexUrlProvider = serviceProvider.GetService<IUrlProvider<SiteIndex>>() ?? throw new Exception("No IUrlProvider service was registered for SiteIndex.");
            IItemTemplateProvider<SiteIndex> siteIndexTemplateProvider = serviceProvider.GetService<IItemTemplateProvider<SiteIndex>>() ?? throw new Exception("No IItemTemplateProvider service was registered for SiteIndex.");
            ITemplateAssetProvider? templateAssetProvider = serviceProvider.GetService<ITemplateAssetProvider>();
            SiteIndex siteIndex = new();

            await publisher.Initialize();
            await TocRoot.Execute(serviceProvider);

            string siteIndexUrl = siteIndexUrlProvider.GetUrl(siteIndex);
            await publisher.Publish(siteIndexUrl, await siteIndexTemplateProvider.GetContent(siteIndex));

            SiteToc siteTableOfContents = new(TocRoot);
            string tableOfContentsUrl = tableOfContentsUrlProvider.GetUrl(siteTableOfContents);
            await publisher.Publish(tableOfContentsUrl, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(siteTableOfContents, typeof(SiteToc), SerializerOptions)));

            if (templateAssetProvider != null)
            {
                IUrlProvider<ITemplateAsset> templateAssetUrlProvider = serviceProvider.GetService<IUrlProvider<ITemplateAsset>>() ?? throw new Exception("No IUrlProvider service was registered for ITemplateAsset.");

                foreach (ITemplateAsset templateAsset in templateAssetProvider.GetAssets())
                {
                    await publisher.Publish(templateAssetUrlProvider.GetUrl(templateAsset), await templateAsset.GetContents());
                }
            }
        }
    }
}