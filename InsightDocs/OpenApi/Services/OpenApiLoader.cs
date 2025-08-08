using InsightDocs.OpenApi.Abstractions;
using InsightDocs.OpenApi.Model;
using Markdig;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using InsightDocsOpenApiOperation = InsightDocs.OpenApi.Model.OpenApiOperation;
using InsightDocsOpenApiParameter = InsightDocs.OpenApi.Model.OpenApiParameter;
using MicrosoftOpenApiOperation = Microsoft.OpenApi.OpenApiOperation;
using OpenApiResponse = InsightDocs.OpenApi.Model.OpenApiResponse;

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

        if (openApiDocument.Components != null && openApiDocument.Components.Schemas != null)
        {
            foreach (KeyValuePair<string, IOpenApiSchema> schema in openApiDocument.Components.Schemas)
            {
                openApiSpec.Schemas.Add(new Model.OpenApiSchema(schema.Key, schema.Value, openApiSpec.Schemas));
            }
        }

        List<IOpenApiSchema> referencedSchemas = [];

        foreach (KeyValuePair<string, IOpenApiPathItem> path in openApiDocument.Paths)
        {
            if (path.Value.Operations != null)
            {
                foreach (KeyValuePair<HttpMethod, MicrosoftOpenApiOperation> operation in path.Value.Operations)
                {
                    InsightDocsOpenApiOperation operationMetadata = new InsightDocsOpenApiOperation(operation.Key.Method.ToUpperInvariant(), path.Key, openApiSpec.Schemas, String.IsNullOrEmpty(operation.Value.Description) ? null : Markdig.Markdown.ToHtml(operation.Value.Description, Pipeline));

                    if (operation.Value.Parameters != null && operation.Value.Parameters.Any())
                    {
                        operationMetadata.Parameters = [];

                        foreach (IOpenApiParameter parameter in operation.Value.Parameters)
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
                            operationMetadata.Parameters ??= [];

                            foreach (KeyValuePair<string, IOpenApiSchema> formField in schema.Properties)
                            {
                                InsightDocsOpenApiParameter formFieldParameter = new InsightDocsOpenApiParameter(formField.Key, String.IsNullOrEmpty(formField.Value.Description) ? null : Markdig.Markdown.ToHtml(formField.Value.Description, Pipeline), OpenApiParameterLocation.Form, schema.Required != null && schema.Required.Contains(formField.Key), formField.Value);
                                operationMetadata.Parameters.Add(formFieldParameter);
                            }
                        }
                    }

                    if (operation.Value.Responses != null && operation.Value.Responses.Any())
                    {
                        operationMetadata.Responses = [];

                        foreach (KeyValuePair<string, IOpenApiResponse> response in operation.Value.Responses)
                        {
                            OpenApiResponse responseMetadata = new OpenApiResponse(String.IsNullOrEmpty(response.Value.Description) ? null : Markdig.Markdown.ToHtml(response.Value.Description, Pipeline), response.Key);

                            if (response.Value.Content != null && response.Value.Content.Any())
                            {
                                responseMetadata.Content = [];

                                foreach (KeyValuePair<string, OpenApiMediaType> content in response.Value.Content)
                                {
                                    OpenApiResponseContent responseContent = new OpenApiResponseContent(content.Key, content.Value.Schema);
                                    responseMetadata.Content.Add(responseContent);

                                    if (content.Value.Schema != null)
                                    {
                                        ApplyMimeTypeToSchema(content.Key, content.Value.Schema, openApiSpec.Schemas, referencedSchemas, []);
                                    }
                                }
                            }

                            operationMetadata.Responses.Add(responseMetadata);
                        }
                    }

                    openApiSpec.Operations.Add(operationMetadata);
                }
            }
        }

        openApiSpec.Schemas.RemoveAll(s => !referencedSchemas.Contains(s.SchemaDefinition));

        return openApiSpec;
    }

    protected virtual void ApplyMimeTypeToSchema(string mimeType, IOpenApiSchema schema, List<Model.OpenApiSchema> insightDocsSchemas, List<IOpenApiSchema> referencedSchemas, Stack<IOpenApiSchema> stack)
    {
        if (schema is OpenApiSchemaReference schemaReference)
        {
            if (schemaReference.Reference != null && !String.IsNullOrEmpty(schemaReference.Reference.Id))
            {
                Model.OpenApiSchema? insightDocsSchema = insightDocsSchemas.FirstOrDefault(s => s.Name == schemaReference.Reference.Id);

                if (insightDocsSchema != null)
                {
                    insightDocsSchema.MimeTypes.Add(mimeType);

                    if (insightDocsSchema.SchemaDefinition != null)
                    {
                        schema = insightDocsSchema.SchemaDefinition;
                    }
                }
            }
        }

        if (stack.Contains(schema))
        {
            return;
        }

        stack.Push(schema);

        if (!referencedSchemas.Contains(schema))
        {
            referencedSchemas.Add(schema);
        }

        if (schema.AllOf != null)
        {
            foreach (IOpenApiSchema? subSchema in schema.AllOf)
            {
                if (subSchema != null)
                {
                    ApplyMimeTypeToSchema(mimeType, subSchema, insightDocsSchemas, referencedSchemas, stack);
                }
            }
        }

        if (schema.AnyOf != null)
        {
            foreach (IOpenApiSchema? subSchema in schema.AnyOf)
            {
                if (subSchema != null)
                {
                    ApplyMimeTypeToSchema(mimeType, subSchema, insightDocsSchemas, referencedSchemas, stack);
                }
            }
        }

        if (schema.Properties != null)
        {
            foreach (KeyValuePair<string, IOpenApiSchema> property in schema.Properties)
            {
                ApplyMimeTypeToSchema(mimeType, property.Value, insightDocsSchemas, referencedSchemas, stack);
            }
        }

        if (schema.Items != null)
        {
            ApplyMimeTypeToSchema(mimeType, schema.Items, insightDocsSchemas, referencedSchemas, stack);
        }

        stack.Pop();
    }
}
