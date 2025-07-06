using InsightDocs.TypeScript.Model;

namespace InsightDocs.TypeScript.Abstractions;

public interface ITypeScriptLoader
{
    Task<TypeScriptProject> LoadApiJson(string filename, Func<TypeScriptTypeDeclaration, bool>? typeFilter, Func<TypeScriptModule, bool>? moduleFilter);
}
