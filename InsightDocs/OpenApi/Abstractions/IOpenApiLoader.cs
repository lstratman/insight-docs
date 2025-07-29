using Microsoft.OpenApi;

namespace InsightDocs.OpenApi.Abstractions;

public interface IOpenApiLoader
{
    Task<OpenApiDocument> LoadOpenApiSpecFile(string openApiSpecFilePath);
}
