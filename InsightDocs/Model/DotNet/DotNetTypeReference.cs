using System.Text;

namespace InsightDocs.Model.DotNet;

public class DotNetTypeReference
{
    private readonly static Dictionary<string, DotNetTypeReference> TypeReferenceCache = [];

    public static DotNetTypeReference Resolve(Type type)
    {
        string key = type.Name;

        if (!String.IsNullOrEmpty(type.Namespace))
        {
            key = type.Namespace + "." + type.Name;
        }

        if (!TypeReferenceCache.TryGetValue(key, out DotNetTypeReference? typeReference))
        {
            typeReference = new DotNetTypeReference(type);
        }

        return typeReference;
    }

    public DotNetTypeReference(Type type)
    {
        if (type.GenericTypeArguments != null && type.GenericTypeArguments.Length > 0)
        {
            GenericArguments = [];

            foreach (Type genericArgument in type.GetGenericArguments())
            {
                GenericArguments.Add(new DotNetGenericArgument(genericArgument));
            }

            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
        }

        Type = DotNetType.Resolve(type);
    }

    public DotNetType Type
    {
        get;
        set;
    }

    public List<DotNetGenericArgument>? GenericArguments
    {
        get;
        set;
    }

    public string Name
    {
        get
        {
            return Type.Name;
        }
    }

    public DotNetNamespace? Namespace
    {
        get
        {
            return Type.Namespace;
        }
    }

    public string DisplayName
    {
        get
        {
            StringBuilder output = new(Type.Name);

            if (GenericArguments != null)
            {
                output.Append('<');
                output.Append(String.Join(", ", GenericArguments.Select(a => a.DisplayName)));
                output.Append('>');
            }

            return output.ToString();
        }
    }
}