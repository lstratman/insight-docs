namespace InsightDocs.OpenApi.Model;

public class OpenApiResponse(string? description, string httpStatusCode)
{
    public string? Description
    {
        get;
        set;
    } = description;

    public string HttpStatusCode
    {
        get;
        set;
    } = httpStatusCode;

    public List<OpenApiResponseContent>? Content
    {
        get;
        set;
    }
}
