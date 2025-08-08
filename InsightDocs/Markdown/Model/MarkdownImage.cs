using InsightDocs.Abstractions;

namespace InsightDocs.Markdown.Model;

public class MarkdownImage(string filePath) : ILinkTarget
{
    public string FilePath
    {
        get;
        set;
    } = filePath;

    public string LinkText
    {
        get
        {
            return "";
        }
    }
}
