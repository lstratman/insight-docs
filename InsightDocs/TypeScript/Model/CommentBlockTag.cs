using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model;

public class CommentBlockTag
{
    [JsonProperty("tag")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Tag
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("content")]
    public List<CommentSegment>? Content
    {
        get;
        set;
    }

    public string? ContentText
    {
        get
        {
            return Content == null ? null : Comment.GetCombinedText(Content);
        }
    }

    public string? ContentHTML
    {
        get
        {
            return Comment.MarkdownToHTML(Content);
        }
    }
}
