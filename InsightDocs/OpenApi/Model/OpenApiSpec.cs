namespace InsightDocs.OpenApi.Model;

public class OpenApiSpec
{
    public List<OpenApiOperation> Operations
    {
        get;
        set;
    } = new List<OpenApiOperation>();

    public List<OpenApiSchema>? Schemas
    {
        get;
        set;
    }
}
