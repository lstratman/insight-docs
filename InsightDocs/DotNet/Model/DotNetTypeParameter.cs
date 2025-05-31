namespace InsightDocs.DotNet.Model;

public class DotNetTypeParameter(Type type)
{
    public string Name
    {
        get;
        set;
    } = type.Name;

    public XmlDocHtml? Description
    {
        get;
        set;
    }
}