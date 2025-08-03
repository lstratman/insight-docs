using InsightDocs.OpenApi.Model;

namespace InsightDocs.OpenApi.Abstractions;

public interface IOpenApiLoader
{
    Task<OpenApiSpec> LoadOpenApiSpecFile(string openApiSpecFilePath);
}
