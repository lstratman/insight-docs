namespace InsightDocs.Abstractions;

public class JsonSearchResult
{
    public required string Url
    {
        get;
        set;
    }

    public required string ResultSummaryHtml
    {
        get;
        set;
    }
}

public interface ILocalSearchService
{
    Task<List<JsonSearchResult>> ExecuteSearch(string query);
}
