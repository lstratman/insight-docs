using InsightDocs.Abstractions;
using Microsoft.OpenApi;

namespace InsightDocs.OpenApi.Model;

public class OpenApiSchema(string name, IOpenApiSchema schemaDefinition, List<OpenApiSchema> allSchemas) : ILinkTarget
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

    public List<OpenApiSchema> AllSchemas
    {
        get;
        set;
    } = allSchemas;

    public string LinkText
    {
        get
        {
            return Name;
        }
    }
}
