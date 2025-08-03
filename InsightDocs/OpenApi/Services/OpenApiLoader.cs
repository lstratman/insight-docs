using InsightDocs.OpenApi.Abstractions;
using InsightDocs.OpenApi.Model;
using Markdig;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using InsightDocsOpenApiOperation = InsightDocs.OpenApi.Model.OpenApiOperation;
using InsightDocsOpenApiParameter = InsightDocs.OpenApi.Model.OpenApiParameter;
using MicrosoftOpenApiOperation = Microsoft.OpenApi.OpenApiOperation;
using MicrosoftOpenApiParameter = Microsoft.OpenApi.OpenApiParameter;

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
                            InsightDocsOpenApiParameter openApiParameter = new InsightDocsOpenApiParameter(parameter.Name!, String.IsNullOrEmpty(parameter.Description) ? null : Markdig.Markdown.ToHtml(parameter.Description, Pipeline), (OpenApiParameterLocation)Enum.Parse(typeof(OpenApiParameterLocation), parameter.In!.Value.ToString("G")), parameter.Required, parameter.Schema);
                            operationMetadata.Parameters.Add(openApiParameter);
                        }
                    }

                    if (operation.Value.RequestBody != null && operation.Value.RequestBody.Content != null && operation.Value.RequestBody.Content.ContainsKey("application/x-www-form-urlencoded"))
                    {
                        IOpenApiSchema? schema = operation.Value.RequestBody.Content["application/x-www-form-urlencoded"].Schema;

                        if (schema != null && schema.Type == JsonSchemaType.Object && schema.Properties != null && schema.Properties.Count > 0)
                        {
                            operationMetadata.Parameters ??= new List<InsightDocsOpenApiParameter>();

                            foreach (KeyValuePair<string, IOpenApiSchema> formField in schema.Properties)
                            {
                                InsightDocsOpenApiParameter formFieldParameter = new InsightDocsOpenApiParameter(formField.Key, String.IsNullOrEmpty(formField.Value.Description) ? null : Markdig.Markdown.ToHtml(formField.Value.Description, Pipeline), OpenApiParameterLocation.Form, schema.Required != null && schema.Required.Contains(formField.Key), formField.Value);
                                operationMetadata.Parameters.Add(formFieldParameter);
                            }
                        }
                    }

                    openApiSpec.Operations.Add(operationMetadata);
                }
            }
        }

        return openApiSpec;
    }
}
