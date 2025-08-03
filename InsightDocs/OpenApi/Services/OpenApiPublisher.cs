using InsightDocs.Abstractions;
using InsightDocs.OpenApi.Abstractions;
using InsightDocs.OpenApi.Model;
using Microsoft.Extensions.Logging;

namespace InsightDocs.OpenApi.Services;

public partial class OpenApiPublisher(
    ILoggerFactory loggerFactory,
    IOpenApiLoader openApiLoader,
    IItemTemplateProvider<OpenApiOperation> openApiOperationTemplateProvider,
    IUrlProvider<OpenApiOperation> openApiOperationUrlProvider,
    IPublisher publisher
) : IOpenApiPublisher
{
    [LoggerMessage(LogLevel.Information, "Publishing topics")]
    public static partial void LogPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics")]
    public static partial void LogFinishedPublishingTopics(ILogger logger); 
    
    [LoggerMessage(LogLevel.Debug, "Publishing topic for endpoint {method} {endpoint}")]
    public static partial void LogPublishingEndpoint(ILogger logger, string method, string endpoint);

    public async Task PublishTopics(TocItem tocRoot, string openApiSpecFilePath)
    {
        OpenApiSpec openApiSpec = await openApiLoader.LoadOpenApiSpecFile(openApiSpecFilePath);
        ILogger logger = loggerFactory.CreateLogger<OpenApiPublisher>();

        LogPublishingTopics(logger);

        if (openApiSpec.Operations.Any())
        {
            TocItem endpointsTocItem = tocRoot.AddTocItem("Endpoints");
            List<TocItem> tocStack = [endpointsTocItem];

            foreach (OpenApiOperation operation in openApiSpec.Operations.OrderBy(o => o.Url).ThenBy(o => o.Method))
            {
                string method = operation.Method.ToUpperInvariant();
                string endpoint = operation.Url;

                LogPublishingEndpoint(logger, method, endpoint);

                string[] pathComponents = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < pathComponents.Length - 1; i++)
                {
                    string pathComponent = pathComponents[i];
                    TocItem? tocItem = tocStack.Count <= i + 1 ? null : tocStack[i + 1];

                    if (tocItem?.Title == pathComponent)
                    {
                        continue;
                    }

                    tocStack = [.. tocStack.Take(i + 1)];

                    for (int j = i; j < pathComponents.Length - 1; j++)
                    {
                        tocStack.Add(tocStack.Last().AddTocItem(pathComponents[j]));
                    }

                    break;
                }

                if (tocStack.Count != pathComponents.Length)
                {
                    tocStack = [.. tocStack.Take(pathComponents.Length)];
                }

                string url = openApiOperationUrlProvider.GetUrl(operation);
                TocItem operationTocItem = tocStack.Last().AddTocItem($"{method} {pathComponents.Last()}", url);
                string operationHtml = await openApiOperationTemplateProvider.GetContent(operation, url);
                    
                await publisher.Publish(url, operationHtml, "text/html", operation.LinkText);
            }

            endpointsTocItem.SortChildren((a, b) =>
            {
                if (a.Children != null && a.Children.Count > 0 && (b.Children == null || b.Children.Count == 0))
                {
                    return -1;
                }

                if (b.Children != null && b.Children.Count > 0 && (a.Children == null || a.Children.Count == 0))
                {
                    return 1;
                }

                if (a.Children != null && a.Children.Count > 0 && b.Children != null && b.Children.Count > 0)
                {
                    return String.Compare(a.Title, b.Title);
                }

                string[] aComponents = a.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] bComponents = a.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (aComponents[1] == bComponents[1])
                {
                    return String.Compare(aComponents[0], bComponents[0]);
                }

                return String.Compare(aComponents[1], bComponents[1]);
            }, true);
        }

        //if (openApiSpec.Components?.Schemas != null && openApiSpec.Components.Schemas.Any())
        //{
        //    TocItem schemasTocItem = tocRoot.AddTocItem("Schemas");
        //}

        LogFinishedPublishingTopics(logger);
    }
}
