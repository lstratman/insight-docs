using Newtonsoft.Json;
using System.Text;
using Markdig;
using System.Text.RegularExpressions;

namespace InsightDocs.TypeScript.Model;

public class Comment
{
    private static readonly MarkdownPipeline Pipeline;
    private static readonly Regex CodeBlocks = new Regex(@"`(?<codeBlock>[^`]+)`");
    private static readonly Regex LinkTags = new Regex(@"\<a\s+href=""(?<url>[^""]+)""");

    static Comment()
    {
        Pipeline = new MarkdownPipelineBuilder().UseCustomContainers().UsePipeTables().Build();
    }

    [JsonProperty("summary")]
    public List<CommentSegment>? SummaryMarkdown
    {
        get;
        set;
    }

    [JsonProperty("returns")]
    public List<CommentSegment>? ReturnsMarkdown
    {
        get;
        set;
    }

    [JsonProperty("blockTags")]
    public List<CommentBlockTag>? BlockTags
    {
        get;
        set;
    }

    public static string? GetCombinedText(List<CommentSegment> segments)
    {
        if (segments == null || segments.Count == 0)
        {
            return null;
        }

        StringBuilder combinedText = new StringBuilder();

        foreach (CommentSegment segment in segments.Where(s => s is not TypeDefinitionCommentSegment))
        {
            combinedText.Append(segment.Text);
        }

        return combinedText.ToString();
    }

    public static string? GetCombinedMarkdown(List<CommentSegment>? segments)
    {
        if (segments == null || segments.Count == 0)
        {
            return null;
        }

        StringBuilder combinedMarkdown = new StringBuilder();

        foreach (CommentSegment segment in segments.Where(s => s is not TypeDefinitionCommentSegment))
        {
            combinedMarkdown.Append(segment.GetMarkdown());
        }

        return combinedMarkdown.ToString();
    }

    public string? SummaryHtml
    {
        get
        {
            return MarkdownToHTML(SummaryMarkdown);
        }
    }

    public string? ReturnsHtml
    {
        get
        {
            return MarkdownToHTML(ReturnsMarkdown);
        }
    }

    public static string? MarkdownToHTML(List<CommentSegment>? markdown)
    {
        string? combinedMarkdown = GetCombinedMarkdown(markdown);

        return String.IsNullOrEmpty(combinedMarkdown) ? null : MarkdownToHTML(combinedMarkdown);
    }

    public static string? MarkdownToHTML(string combinedMarkdown)
    {
        return LinkTags.Replace(Markdig.Markdown.ToHtml(CodeBlocks.Replace(combinedMarkdown.Replace("<", "&lt;").Replace(">", "&gt;"), MaintainTagsInCodeBlocks), Pipeline), m =>
        {
            string returnText = m.Value;

            if (m.Groups["url"].Value.StartsWith("http://") || m.Groups["url"].Value.StartsWith("https://"))
            {
                returnText += " target=\"_new\"";
            }

            return returnText;
        });
    }

    protected static string MaintainTagsInCodeBlocks(Match match)
    {
        return "`" + match.Groups["codeBlock"].Value.Replace("&lt;", "<").Replace("&gt;", ">") + "`";
    }
}
