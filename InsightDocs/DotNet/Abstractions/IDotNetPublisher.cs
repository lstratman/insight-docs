namespace InsightDocs.DotNet.Abstractions;

public interface IDotNetPublisher
{
    Task PublishTopics(TocItem tocRoot, List<string> assemblyPaths, List<string> runtimeAssemblyPaths, Func<Type, bool>? typeFilter);
}
