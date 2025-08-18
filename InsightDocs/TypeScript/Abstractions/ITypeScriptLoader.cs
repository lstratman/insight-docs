using InsightDocs.TypeScript.Model;

namespace InsightDocs.TypeScript.Abstractions;

public interface ITypeScriptLoader
{
    Task<TypeScriptProject> LoadDefinitionFiles(List<string> definitionFilePaths, bool excludePackageRoot);
}
