namespace InsightDocs.Abstractions;

public interface IUrlChecker
{
    void RegisterUrl(string url, string sourceUrl);
    Task CheckUrls();
}
