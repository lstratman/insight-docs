namespace InsightDocs.Markdown.Abstractions;

public interface IMarkdownPublisher
{
    Task PublishTopic(TocItem tocItem, string markdownFilePath);
    Task PublishTopics(TocItem tocRoot, List<string> markdownFilePaths);
}
