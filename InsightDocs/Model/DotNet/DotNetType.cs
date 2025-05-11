namespace InsightDocs.Model.DotNet;

public class DotNetType
{
    public DotNetType(Type type, DotNetNamespace ns)
    {
        Namespace = ns;
        Name = type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name;
        FullName = String.IsNullOrEmpty(type.Namespace) ? Name : type.Namespace + "." + Name;

        Type[] typeParameters = type.GetGenericArguments();

        if (typeParameters != null && typeParameters.Length > 0)
        {
            DisplayName = Name + "<" + String.Join(", ", typeParameters.Select(a => a.Name)) + ">";
            TypeParameters = [.. typeParameters.Select(a => new DotNetTypeParameter(a))];
        }

        else
        {
            DisplayName = Name;
        }

        if (type.IsInterface)
        {
            TypeName = "Interface";
        }

        else if (type.IsEnum)
        {
            TypeName = "Enum";
        }

        else if (type.IsValueType)
        {
            TypeName = "Struct";
        }

        else
        {
            TypeName = "Class";
        }
    }

    public List<DotNetTypeParameter>? TypeParameters
    {
        get;
        set;
    }

    public DotNetNamespace Namespace
    {
        get;
        set;
    }

    public string TypeName
    {
        get;
        set;
    }

    public string DisplayTitle
    {
        get
        {
            return DisplayName + " " + TypeName;
        }
    }

    public string DisplayName
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public string FullName
    {
        get;
        set;
    }

    public string? Description
    {
        get;
        set;
    }
}