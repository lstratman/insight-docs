using InsightDocs.TypeScript.Model;

namespace InsightDocs.TypeScript.Abstractions;

public interface ITypeScriptLoader
{
    Task<TypeScriptProject> LoadApiJson(string filename);
}
