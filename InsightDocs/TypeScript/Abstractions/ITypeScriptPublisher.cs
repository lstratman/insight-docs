using InsightDocs.TypeScript.Model;

namespace InsightDocs.TypeScript.Abstractions;

public interface ITypeScriptPublisher
{
    Task PublishTopics(TocItem tocRoot, string typeScriptApiJsonFilePath, Func<TypeScriptTypeDeclaration, bool>? typeFilter, Func<TypeScriptModule, bool>? moduleFilter);
}
