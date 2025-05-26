namespace InsightDocs.Abstractions;

public interface IUrlProvider<T> where T : ILinkTarget
{
    string GetUrl(T item, string? urlPrefix = null);
}