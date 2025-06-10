using InsightDocs.Markdown.Abstractions;
using InsightDocs.Markdown.Model;
using Markdig;
using System.Text;
using MarkdownProcessor = Markdig.Markdown;

namespace InsightDocs.Markdown.Services;

public class MarkdownLoader : IMarkdownLoader
{
    protected MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseCustomContainers().UsePipeTables().UseAutoIdentifiers().Build();

    public virtual async Task<string> GetHtml(MarkdownFile markdownFile)
    {
        string markdownContent = await File.ReadAllTextAsync(markdownFile.FilePath);
        markdownContent = markdownContent.Replace("&nbsp;", " ").Replace(" & ", " &amp; ");

        return MarkdownProcessor.ToHtml(markdownContent, _pipeline);
    }

    public virtual MarkdownFile LoadMarkdownFile(string markdownFilePath)
    {
        return new MarkdownFile(markdownFilePath, FromKebabCase(Path.GetFileNameWithoutExtension(markdownFilePath)));
    }

    protected virtual string FromKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        StringBuilder output = new StringBuilder(input.Length);
        bool toUpper = true;
        
        foreach (char c in input)
        {
            if (c == '-' || c == ' ')
            {
                toUpper = true;
                output.Append(' ');
            }

            else
            {
                output.Append(toUpper ? char.ToUpper(c) : c);
                toUpper = false;
            }
        }

        return output.ToString();
    }
}
