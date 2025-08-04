using InsightDocs.Abstractions;
using Microsoft.OpenApi;

namespace InsightDocs.OpenApi.Model;

public class OpenApiSchema(string name, IOpenApiSchema schemaDefinition) : ILinkTarget
{
    public string Name
    {
        get;
        set;
    } = name;

    public IOpenApiSchema SchemaDefinition
    {
        get;
        set;
    } = schemaDefinition;

    public string LinkText
    {
        get
        {
            return Name;
        }
    }
}
