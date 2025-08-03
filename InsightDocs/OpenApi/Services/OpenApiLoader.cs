using InsightDocs.OpenApi.Abstractions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace InsightDocs.OpenApi.Services;

public class OpenApiLoader : IOpenApiLoader
{
    public async Task<OpenApiDocument> LoadOpenApiSpecFile(string openApiSpecFilePath)
    {
        (OpenApiDocument? openApiDocument, OpenApiDiagnostic? _) = await OpenApiDocument.LoadAsync(openApiSpecFilePath);
        
        if (openApiDocument == null)
        {
            throw new Exception($"Unable to load OpenAPI specification file: {openApiSpecFilePath}");
        }

        return openApiDocument;
    }
}
