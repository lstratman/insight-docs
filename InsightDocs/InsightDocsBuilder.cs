using InsightDocs.Abstractions;
using InsightDocs.Services;
using InsightDocs.Site.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InsightDocs;

public class InsightDocsOptions
{
    public bool AddSearch
    {
        get;
        set;
    } = true;

    public bool EnableParallelism
    {
        get;
        set;
    } = true;

    public int MaxDegreeOfParallelism
    {
        get;
        set;
    } = Environment.ProcessorCount;

    public ErrorBehavior UnknownCodeLanguageBehavior
    {
        get;
        set;
    } = ErrorBehavior.Error;
}

public class InsightDocsBuilder
{
    public static InsightDocsBuilder Create(Action<InsightDocsOptions>? optionsFactory = null)
    {
        InsightDocsBuilder builder = new InsightDocsBuilder();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                InsightDocsOptions options = new();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new InsightDocsOptions());
        }

        return builder;
    }

    private InsightDocsBuilder()
    {
        Services = new ServiceCollection();
        Services.AddScoped<IUrlPrefixProvider, UrlPrefixProvider>();
        Services.AddSingleton<ICodeLanguageService, CodeLanguageService>();
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

    public TocItem AddTocItem(string title, string? url = null, string? urlPrefix = null)
    {
        return TocRoot.AddTocItem(title, url, urlPrefix);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task Execute()
    {
        using (ServiceProvider serviceProvider = Services.BuildServiceProvider())
        {
            IPublisher publisher = serviceProvider.GetRequiredService<IPublisher>();
            IUrlProvider<SiteToc> tableOfContentsUrlProvider = serviceProvider.GetRequiredService<IUrlProvider<SiteToc>>();
            IUrlProvider<SiteIndex> siteIndexUrlProvider = serviceProvider.GetRequiredService<IUrlProvider<SiteIndex>>();
            IItemTemplateProvider<SiteIndex> siteIndexTemplateProvider = serviceProvider.GetRequiredService<IItemTemplateProvider<SiteIndex>>();
            IEnumerable<IAssetProvider> assetProviders = serviceProvider.GetServices<IAssetProvider>();
            SiteIndex siteIndex = new();

            await publisher.Initialize();
            await TocRoot.Execute(serviceProvider);

            string siteIndexUrl = siteIndexUrlProvider.GetUrl(siteIndex);
            await publisher.Publish(siteIndexUrl, await siteIndexTemplateProvider.GetContent(siteIndex));

            SiteToc siteTableOfContents = new(TocRoot);
            string tableOfContentsUrl = tableOfContentsUrlProvider.GetUrl(siteTableOfContents);
            await publisher.Publish(tableOfContentsUrl, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(siteTableOfContents, typeof(SiteToc), SerializerOptions)));

            if (assetProviders != null && assetProviders.Any())
            {
                IUrlProvider<IAsset> assetUrlProvider = serviceProvider.GetRequiredService<IUrlProvider<IAsset>>();

                foreach (IAssetProvider assetProvider in assetProviders)
                {
                    foreach (IAsset templateAsset in assetProvider.GetAssets())
                    {
                        await publisher.Publish(assetUrlProvider.GetUrl(templateAsset), await templateAsset.GetContents());
                    }
                }
            }
        }
    }
}