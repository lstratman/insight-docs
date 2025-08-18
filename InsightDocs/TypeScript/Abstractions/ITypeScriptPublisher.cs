using InsightDocs.TypeScript.Model;

namespace InsightDocs.TypeScript.Abstractions;

public interface ITypeScriptPublisher
{
    Task PublishTopics(TocItem tocRoot, List<string> definitionFilePaths, Func<TypeScriptTypeDeclaration, bool>? typeFilter, Func<TypeScriptModule, bool>? moduleFilter, bool excludePackageRoot);
}
