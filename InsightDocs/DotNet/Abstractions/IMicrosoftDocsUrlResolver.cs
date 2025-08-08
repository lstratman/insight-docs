using InsightDocs.DotNet.Model;

namespace InsightDocs.DotNet.Abstractions;

public interface IMicrosoftDocsUrlResolver
{
    string GetUrl(DotNetType type);
    string GetUrl(DotNetMethodOverload method);
    string GetUrl(DotNetProperty property);
    string GetUrl(DotNetField field);
    string GetUrl(DotNetNamespace ns);
    bool IsMicrosoftType(DotNetType type);
    bool IsMicrosoftNamespace(string ns);
}
