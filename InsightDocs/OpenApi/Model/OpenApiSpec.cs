namespace InsightDocs.OpenApi.Model;

public class OpenApiSpec
{
    public List<OpenApiOperation> Operations
    {
        get;
        set;
    } = [];

    public List<OpenApiSchema> Schemas
    {
        get;
        set;
    } = [];
}
