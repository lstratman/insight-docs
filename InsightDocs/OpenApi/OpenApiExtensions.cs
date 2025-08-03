using InsightDocs.Abstractions;
using InsightDocs.OpenApi.Abstractions;
using InsightDocs.OpenApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.OpenApi;

public static class OpenApiExtensions
{
    public static InsightDocsBuilder UseOpenApi(this InsightDocsBuilder builder)
    {
        builder.Services.AddScoped<IOpenApiLoader, OpenApiLoader>();
        builder.Services.AddScoped<IOpenApiPublisher, OpenApiPublisher>();

        return builder;
    }
}

public static class OpenApiTocItemExtensions
{
    public static TocItem IncludeOpenApiSpecFile(this TocItem tocItem, string openApiSpecFilePath, string? title = null)
    {
        tocItem.RegisterExecutor(async (serviceProvider) =>
        {
            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
                prefixProvider.UrlPrefix = tocItem.FullUrlPrefix;

                IOpenApiPublisher openApiPublisher = serviceScope.ServiceProvider.GetRequiredService<IOpenApiPublisher>();
                await openApiPublisher.PublishTopics(tocItem, openApiSpecFilePath);

                if (!String.IsNullOrEmpty(title))
                {
                    tocItem.Title = title;
                }
            }
        });

        return tocItem;
    }
}
