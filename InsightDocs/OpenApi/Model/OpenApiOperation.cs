using InsightDocs.Abstractions;

namespace InsightDocs.OpenApi.Model;

public class OpenApiOperation(string method, string url, List<OpenApiSchema> allSchemas, string? description = null) : ILinkTarget
{
    public string Method
    {
        get;
        set;
    } = method;

    public string Url
    {
        get;
        set;
    } = url;

    public string? Description
    {
        get;
        set;
    } = description;

    public List<OpenApiParameter>? Parameters
    {
        get;
        set;
    }

    public List<OpenApiResponse>? Responses
    {
        get;
        set;
    }

    public List<OpenApiSchema> AllSchemas
    {
        get;
        set;
    } = allSchemas;

    public string LinkText
    {
        get
        {
            return $"{Method} {Url}";
        }
    }
}
