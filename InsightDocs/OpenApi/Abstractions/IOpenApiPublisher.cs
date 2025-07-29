namespace InsightDocs.OpenApi.Abstractions;

public interface IOpenApiPublisher
{
    Task PublishTopics(TocItem tocRoot, string openApiSpecFilePath);
}
