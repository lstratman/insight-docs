namespace InsightDocs.DotNet.Model;

public class DotNetMethodParameter
{
    public bool IsOptional
    {
        get;
        set;
    }

    public bool IsOut
    {
        get;
        set;
    }

    public bool IsByRef
    {
        get;
        set;
    }

    public required string Name
    {
        get;
        set;
    }

    public required DotNetTypeReference Type
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