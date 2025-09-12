namespace InsightDocs.Abstractions;

public interface IPublisher
{
    Task Initialize();
    Task Publish(string url, object contents, string mimeType, string? title);
    Task<byte[]> GetUrlContents(string url);
    Task<string> GetUrlMimeType(string url);
    void RegisterPublishedAnchor(string url, string anchor);
    Task<bool> UrlWasPublished(string url);

    bool SupportsDynamicToc
    {
        get;
    }
}