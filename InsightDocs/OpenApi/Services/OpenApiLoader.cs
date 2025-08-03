using InsightDocs.OpenApi.Abstractions;
using InsightDocs.OpenApi.Model;
using Markdig;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using InsightDocsOpenApiOperation = InsightDocs.OpenApi.Model.OpenApiOperation;
using MicrosoftOpenApiParameter = Microsoft.OpenApi.OpenApiParameter;
using InsightDocsOpenApiParameter = InsightDocs.OpenApi.Model.OpenApiParameter;
using MicrosoftOpenApiOperation = Microsoft.OpenApi.OpenApiOperation;

namespace InsightDocs.OpenApi.Services;

public class OpenApiLoader : IOpenApiLoader
{
    private static readonly MarkdownPipeline Pipeline;

    static OpenApiLoader()
    {
        Pipeline = new MarkdownPipelineBuilder().UseCustomContainers().UsePipeTables().Build();
    }

    public async Task<OpenApiSpec> LoadOpenApiSpecFile(string openApiSpecFilePath)
    {
        (OpenApiDocument? openApiDocument, OpenApiDiagnostic? _) = await OpenApiDocument.LoadAsync(openApiSpecFilePath);
        
        if (openApiDocument == null)
        {
            throw new Exception($"Unable to load OpenAPI specification file: {openApiSpecFilePath}");
        }

        OpenApiSpec openApiSpec = new OpenApiSpec();

        foreach (KeyValuePair<string, IOpenApiPathItem> path in openApiDocument.Paths)
        {
            if (path.Value.Operations != null)
            {
                foreach (KeyValuePair<HttpMethod, MicrosoftOpenApiOperation> operation in path.Value.Operations)
                {
                    InsightDocsOpenApiOperation operationMetadata = new InsightDocsOpenApiOperation(operation.Key.Method.ToUpperInvariant(), path.Key, String.IsNullOrEmpty(operation.Value.Description) ? null : Markdig.Markdown.ToHtml(operation.Value.Description, Pipeline));

                    if (operation.Value.Parameters != null && operation.Value.Parameters.Any())
                    {
                        operationMetadata.Parameters = new List<InsightDocsOpenApiParameter>();

                        foreach (MicrosoftOpenApiParameter parameter in operation.Value.Parameters)
                        {
                            string parameterType = "";

                            if (parameter.Schema != null)
                            {
                                if (parameter.Schema.Type != null)
                                {
                                    parameterType = parameter.Schema.Type.ToString()!;
                                }
                            }

                            InsightDocsOpenApiParameter openApiParameter = new InsightDocsOpenApiParameter(parameter.Name!, String.IsNullOrEmpty(parameter.Description) ? null : Markdig.Markdown.ToHtml(parameter.Description, Pipeline), (OpenApiParameterLocation)Enum.Parse(typeof(OpenApiParameterLocation), parameter.In!.Value.ToString("G")), parameter.Required, parameterType);
                            operationMetadata.Parameters.Add(openApiParameter);
                        }
                    }

                    openApiSpec.Operations.Add(operationMetadata);
                }
            }
        }

        return openApiSpec;
    }
}
