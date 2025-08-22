namespace InsightDocs.Abstractions;

public interface IPublisher
{
    Task Initialize();
    Task Publish(string url, object contents, string mimeType, string? title);
    void RegisterPublishedAnchor(string url, string anchor);
    Task<bool> UrlWasPublished(string url);
}