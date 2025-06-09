using InsightDocs.Abstractions;

namespace InsightDocs.Markdown.Model;

public class MarkdownFile(string filePath, string title) : ILinkTarget
{
    public string FilePath
    {
        get;
        set;
    } = filePath;

    public string Title
    {
        get;
        set;
    } = title;

    public string LinkText
    {
        get;
        set;
    } = title;

    public string? Html
    {
        get;
        set;
    }
}
