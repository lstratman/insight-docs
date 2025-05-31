namespace InsightDocs.DotNet.Model;

public class DotNetGenericArgument
{
    public DotNetGenericArgument(Type type)
    {
        if (type.IsGenericParameter)
        {
            TypeParameterName = type.Name;
        }

        else
        {
            Type = DotNetTypeReference.Resolve(type);
        }
    }

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
}