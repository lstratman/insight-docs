namespace InsightDocs.Model.DotNet;

public class XmlDocHtml(List<XmlDocCommentComponent> components)
{
    public List<XmlDocCommentComponent> Components
    {
        get;
        set;
    } = components;

    public string ToHtml()
    {
        return String.Join(' ', Components.Select(c => c.ToHtml()));
    }
}