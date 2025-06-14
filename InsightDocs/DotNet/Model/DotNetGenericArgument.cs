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
            if (Type != null)
            {
                return Type.DisplayName;
            }

            else
            {
                return TypeParameterName!;
            }
        }
    }

    public string XmlDocKey
    {
        get
        {
            if (TypeParameterName != null)
            {
                return TypeParameterName;
            }

            else
            {
                return Type!.XmlDocKey;
            }
        }
    }

    public string CSharpCode
    {
        get
        {
            if (Type != null)
            {
                return Type.CSharpCode;
            }

            else
            {
                return TypeParameterName!;
            }
        }
    }

    public string VBCode
    {
        get
        {
            if (Type != null)
            {
                return Type.VBCode;
            }

            else
            {
                return TypeParameterName!;
            }
        }
    }
}