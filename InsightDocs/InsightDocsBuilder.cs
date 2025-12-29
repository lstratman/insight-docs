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
    public string SiteTitle
    {
        get;
        set;
    } = "";

    public string? InitialUrl
    {
        get;
        set;
    }

    public IAsset? FaviconAsset
    {
        get;
        set;
    }

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

    public bool UseDynamicToc
    {
        get;
        set;
    } = false;

    public string? DynamicTocUrl
    {
        get;
        set;
    } = "/_toc";
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
        Services.AddSingleton(ApplicationExitService);
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

    public IApplicationExitService ApplicationExitService
    {
        get;
        private set;
    } = new ApplicationExitService();

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
        try
        {
            using (ServiceProvider serviceProvider = Services.BuildServiceProvider())
            {
                IUrlChecker? urlChecker = serviceProvider.GetService<IUrlChecker>();
                IPublisher publisher = serviceProvider.GetRequiredService<IPublisher>();
                IUrlProvider<SiteToc> tableOfContentsUrlProvider = serviceProvider.GetRequiredService<IUrlProvider<SiteToc>>();
                IUrlProvider<SiteIndex> siteIndexUrlProvider = serviceProvider.GetRequiredService<IUrlProvider<SiteIndex>>();
                IItemTemplateProvider<SiteIndex> siteIndexTemplateProvider = serviceProvider.GetRequiredService<IItemTemplateProvider<SiteIndex>>();
                InsightDocsOptions options = serviceProvider.GetRequiredService<InsightDocsOptions>();
                IEnumerable<IAssetProvider> assetProviders = serviceProvider.GetServices<IAssetProvider>();

                if (options.UseDynamicToc && !publisher.SupportsDynamicToc)
                {
                    throw new InvalidOperationException("The configured publisher does not support dynamic table of contents.");
                }

                if (options.UseDynamicToc && String.IsNullOrEmpty(options.DynamicTocUrl))
                {
                    throw new InvalidOperationException("A dynamic table of contents URL must be specified if dynamic table of contents is enabled.");
                }

                await publisher.Initialize();
                await TocRoot.Execute(serviceProvider);

                if (assetProviders != null && assetProviders.Any())
                {
                    IUrlProvider<IAsset> assetUrlProvider = serviceProvider.GetRequiredService<IUrlProvider<IAsset>>();

                    foreach (IAssetProvider assetProvider in assetProviders)
                    {
                        foreach (IAsset templateAsset in assetProvider.GetAssets())
                        {
                            await publisher.Publish(assetUrlProvider.GetUrl(templateAsset), await templateAsset.GetContents(), templateAsset.MimeType, null);
                        }
                    }
                }

                await TocRoot.PostExecute(serviceProvider);

                SiteToc siteTableOfContents = new SiteToc(TocRoot);
                string tableOfContentsUrl = tableOfContentsUrlProvider.GetUrl(siteTableOfContents);
                await publisher.Publish(tableOfContentsUrl, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(siteTableOfContents, typeof(SiteToc), SerializerOptions)), "application/json", null);

                SiteIndex siteIndex = new SiteIndex(options.SiteTitle, options.InitialUrl, siteTableOfContents, options.FaviconAsset, options.UseDynamicToc, options.DynamicTocUrl);
                string siteIndexUrl = siteIndexUrlProvider.GetUrl(siteIndex);
                await publisher.Publish(siteIndexUrl, await siteIndexTemplateProvider.GetContent(siteIndex, siteIndexUrl), "text/html", siteIndex.Title);

                if (urlChecker != null)
                {
                    await urlChecker.CheckUrls();
                }
            }
        }

        finally
        {
            await ApplicationExitService.Exit();
        }
    }
}