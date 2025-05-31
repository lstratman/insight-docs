namespace InsightDocs.DotNet.Model;

public abstract class DotNetXmlDocSource
{
    public XmlDocHtml? Description
    {
        get;
        set;
    }

    public XmlDocHtml? Remarks
    {
        get;
        set;
    }

    public List<XmlDocHtml>? SeeAlso
    {
        get;
        set;
    }
}