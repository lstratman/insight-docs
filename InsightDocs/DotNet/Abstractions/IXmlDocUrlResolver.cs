using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Abstractions;

public interface IXmlDocUrlResolver
{
    string GetUrl(string key);
    string GetLinkText(string key);
    void RegisterLookup<T>(T target, string key) where T : ILinkTarget;
}