using InsightDocs.Abstractions;
using InsightDocs.OpenApi.Abstractions;
using Markdig;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace InsightDocs.OpenApi.Services;

public partial class OpenApiPublisher(
    ILoggerFactory loggerFactory,
    IOpenApiLoader openApiLoader,
    IItemTemplateProvider<Model.OpenApiOperation> openApiOperationTemplateProvider,
    IUrlProvider<Model.OpenApiOperation> openApiOperationUrlProvider,
    IPublisher publisher
) : IOpenApiPublisher
{
    private static readonly MarkdownPipeline Pipeline;

    [LoggerMessage(LogLevel.Information, "Publishing topics")]
    public static partial void LogPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics")]
    public static partial void LogFinishedPublishingTopics(ILogger logger); 
    
    [LoggerMessage(LogLevel.Debug, "Publishing topic for endpoint {method} {endpoint}")]
    public static partial void LogPublishingEndpoint(ILogger logger, string method, string endpoint);

    static OpenApiPublisher()
    {
        Pipeline = new MarkdownPipelineBuilder().UseCustomContainers().UsePipeTables().Build();
    }

    public async Task PublishTopics(TocItem tocRoot, string openApiSpecFilePath)
    {
        OpenApiDocument openApiSpec = await openApiLoader.LoadOpenApiSpecFile(openApiSpecFilePath);
        ILogger logger = loggerFactory.CreateLogger<OpenApiPublisher>();

        LogPublishingTopics(logger);

        if (openApiSpec.Paths.Any())
        {
            TocItem endpointsTocItem = tocRoot.AddTocItem("Endpoints");
            List<TocItem> tocStack = [endpointsTocItem];

            foreach (KeyValuePair<string, IOpenApiPathItem> path in openApiSpec.Paths.OrderBy(p => p.Key))
            {
                if (path.Value.Operations == null || path.Value.Operations.Count == 0)
                {
                    continue;
                }

                foreach (KeyValuePair<HttpMethod, OpenApiOperation> operation in path.Value.Operations.OrderBy(o => o.Key.Method))
                {
                    OpenApiOperation operationMetadata = operation.Value;
                    string method = operation.Key.Method.ToUpperInvariant();
                    string endpoint = path.Key;

                    LogPublishingEndpoint(logger, method, endpoint);

                    string[] pathComponents = path.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);

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

                    Model.OpenApiOperation operationModel = new Model.OpenApiOperation(method, path.Key, String.IsNullOrEmpty(operationMetadata.Description) ? null : Markdig.Markdown.ToHtml(operationMetadata.Description, Pipeline));

                    if (operationMetadata.Parameters != null && operationMetadata.Parameters.Any())
                    {
                        operationModel.Parameters = new List<Model.OpenApiParameter>();

                        foreach (OpenApiParameter parameter in operationMetadata.Parameters)
                        {
                            string parameterType = "";

                            if (parameter.Schema != null)
                            {
                                if (parameter.Schema.Type != null)
                                {
                                    parameterType = parameter.Schema.Type.ToString()!;
                                }
                            }

                            Model.OpenApiParameter openApiParameter = new Model.OpenApiParameter(parameter.Name!, String.IsNullOrEmpty(parameter.Description) ? null : Markdig.Markdown.ToHtml(parameter.Description, Pipeline), (Model.OpenApiParameterLocation)Enum.Parse(typeof(Model.OpenApiParameterLocation), parameter.In!.Value.ToString("G")), parameter.Required, parameterType);
                            operationModel.Parameters.Add(openApiParameter);
                        }
                    }

                    string url = openApiOperationUrlProvider.GetUrl(operationModel);
                    TocItem operationTocItem = tocStack.Last().AddTocItem($"{method} {pathComponents.Last()}", url);
                    string operationHtml = await openApiOperationTemplateProvider.GetContent(operationModel, url);
                    
                    await publisher.Publish(url, operationHtml, "text/html", operationModel.LinkText);
                }
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

        if (openApiSpec.Components?.Schemas != null && openApiSpec.Components.Schemas.Any())
        {
            TocItem schemasTocItem = tocRoot.AddTocItem("Schemas");
        }

        LogFinishedPublishingTopics(logger);
    }
}
