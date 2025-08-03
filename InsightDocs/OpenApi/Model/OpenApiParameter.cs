using Microsoft.OpenApi;

namespace InsightDocs.OpenApi.Model;

public class OpenApiParameter(string name, string? description, OpenApiParameterLocation location, bool isRequired, IOpenApiSchema? schema)
{
    public string Name
    {
        get;
        set;
    } = name;

    public string? Description
    {
        get;
        set;
    } = description;

    public OpenApiParameterLocation Location
    {
        get;
        set;
    } = location;

    public bool IsRequired
    {
        get;
        set;
    } = isRequired;

    public IOpenApiSchema? Schema
    {
        get;
        set;
    } = schema;
}

public enum OpenApiParameterLocation
{
    Query,
    Header,
    Path,
    Cookie,
    Form
}
