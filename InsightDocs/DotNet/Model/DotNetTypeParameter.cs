namespace InsightDocs.DotNet.Model;

public class DotNetTypeParameter
{
    public required string Name
    {
        get;
        set;
    }

    public XmlDocHtml? Description
    {
        get;
        set;
    }
}