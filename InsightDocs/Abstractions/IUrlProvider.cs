namespace InsightDocs.Abstractions;

public interface IUrlProvider<T>
{
    string GetUrl(T item, string? urlPrefix = null);
}