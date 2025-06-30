namespace InsightDocs.Abstractions;

public interface ISearchService
{
    string SearchUrl
    {
        get;
    }

    Task IndexContentForSearch(string url, string content, string title);
}
