using InsightDocs.Abstractions;
using InsightDocs.Markdown.Abstractions;
using InsightDocs.Markdown.Model;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.RegularExpressions;
using MarkdownProcessor = Markdig.Markdown;

namespace InsightDocs.Markdown.Services;

public class MarkdownLoader(IServiceProvider serviceProvider) : IMarkdownLoader
{
    protected MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseCustomContainers().UsePipeTables().UseAutoIdentifiers().Build();
    protected static readonly Regex LinkMatcher = new Regex(@"<a href\s*=\s*(['""])(?<url>.*?)\1");

    public virtual async Task<string> GetHtml(MarkdownFile markdownFile)
    {
        string markdownContent = await File.ReadAllTextAsync(markdownFile.FilePath);
        markdownContent = markdownContent.Replace("&nbsp;", " ").Replace(" & ", " &amp; ");

        string markdownHtml = MarkdownProcessor.ToHtml(markdownContent, _pipeline);
        IUrlRemapper? linkRemapper = serviceProvider.GetService<IUrlRemapper>();

        if (linkRemapper != null)
        {
            markdownHtml = LinkMatcher.Replace(markdownHtml, evaluator =>
            {
                string? newUrl = linkRemapper.RemapUrl(evaluator.Groups["url"].Value);
                return String.IsNullOrEmpty(newUrl) ? evaluator.Value : $"<a href=\"{newUrl}\"";
            });
        }

        return markdownHtml;
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
