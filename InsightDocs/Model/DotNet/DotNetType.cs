using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetType
{
    private readonly static Dictionary<string, DotNetType> TypeCache = [];

    public static DotNetType Resolve(Type type)
    {
        string key = type.Name;

        if (!String.IsNullOrEmpty(type.Namespace))
        {
            key = type.Namespace + "." + type.Name;
        }

        if (!TypeCache.TryGetValue(key, out DotNetType? typeMetadata))
        {
            DotNetNamespace? ns = String.IsNullOrEmpty(type.Namespace) ? null : DotNetNamespace.Resolve(type.Namespace);
            typeMetadata = new DotNetType(type, ns);
        }

        return typeMetadata;
    }

    protected DotNetType(Type type, DotNetNamespace? ns)
    {
        string key = type.Name;

        if (!String.IsNullOrEmpty(type.Namespace))
        {
            key = type.Namespace + "." + type.Name;
        }

        if (!TypeCache.ContainsKey(key))
        {
            TypeCache[key] = this;
        }

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

        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (methods != null && methods.Length > 0)
        {
            Methods = [..methods.Select(m => new DotNetMethod(m))];
        }
    }

    public List<DotNetTypeParameter>? TypeParameters
    {
        get;
        set;
    }

    public DotNetNamespace? Namespace
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

    public List<DotNetMethod>? Methods
    {
        get;
        set;
    }
}