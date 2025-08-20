using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;

namespace InsightDocs.TypeScript.Abstractions
{
    public interface IMDNUrlResolver
    {
        string GetUrl(TypeScriptTypeDeclaration item);
        string GetUrl(TypeScriptInterface item);
        string GetUrl(TypeScriptMethod item);
        string GetUrl(TypeScriptProperty item);
        string GetUrl(TypeScriptMethodSignature item);
        string GetUrl(IntrinsicType item);
    }
}
