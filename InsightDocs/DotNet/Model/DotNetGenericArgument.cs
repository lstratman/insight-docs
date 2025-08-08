namespace InsightDocs.DotNet.Model;

public class DotNetGenericArgument
{
    public DotNetTypeReference? Type
    {
        get;
        set;
    }

    public string? TypeParameterName
    {
        get;
        set;
    }

    public string DisplayName
    {
        get
        {
            return Type != null ? Type.DisplayName : TypeParameterName!;
        }
    }

    public string XmlDocKey
    {
        get
        {
            return TypeParameterName ?? Type!.XmlDocKey;
        }
    }

    public string CSharpCode
    {
        get
        {
            return Type != null ? Type.CSharpCode : TypeParameterName!;
        }
    }

    public string VBCode
    {
        get
        {
            return Type != null ? Type.VBCode : TypeParameterName!;
        }
    }
}