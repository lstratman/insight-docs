namespace InsightDocs.Model.DotNet;

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
}