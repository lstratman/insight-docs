using InsightDocs.Markdown.Model;

namespace InsightDocs.Markdown.Abstractions;

public interface IMarkdownLoader
{
    MarkdownFile LoadMarkdownFile(string markdownFilePath);
    Task<string> GetHtml(MarkdownFile markdownFile);
}
