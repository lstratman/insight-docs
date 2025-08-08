using Microsoft.OpenApi;

namespace InsightDocs.OpenApi.Model;

public class OpenApiResponseContent(string mimeType, IOpenApiSchema? schema = null)
{
    public string MimeType
    {
        get;
        set;
    } = mimeType;

    public IOpenApiSchema? Schema
    {
        get;
        set;
    } = schema;
}
