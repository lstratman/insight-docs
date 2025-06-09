namespace InsightDocs.Markdown.Abstractions;

public interface IMarkdownPublisher
{
    Task PublishTopics(TocItem tocRoot, List<string> markdownFilePaths);
}
