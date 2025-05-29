using System.Text;

namespace InsightDocs.Model.DotNet;

public class DotNetTypeReference
{
    private readonly static Dictionary<string, DotNetTypeReference> TypeReferenceCache = [];

    public static DotNetTypeReference Resolve(Type type)
    {
        if (type.IsByRef)
        {
            return Resolve(type.GetElementType()!);
        }

        string? key = type.FullName;

        if (key == null)
        {
            key = type.Name;
            
            Type? currentDeclaringType = type.DeclaringType;

            while (currentDeclaringType != null)
            {
                key = (currentDeclaringType.Name.Contains('`') ? currentDeclaringType.Name[..currentDeclaringType.Name.IndexOf('`')] : currentDeclaringType.Name) + "." + key;
                currentDeclaringType = currentDeclaringType.DeclaringType;
            }

            if (type.Namespace != null)
            {
                key = type.Namespace + "." + key;
            }
        }

        if (!TypeReferenceCache.TryGetValue(key, out DotNetTypeReference? typeReference))
        {
            typeReference = new DotNetTypeReference(type);
        }

        return typeReference;
    }

    public DotNetTypeReference(Type type)
    {
        if (type.IsArray)
        {
            IsArray = true;
            type = type.GetElementType()!;
        }

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

        if (type.IsGenericParameter)
        {
            IsGenericParameter = true;
            GenericParameterName = type.Name;
        }

        else
        {
            Type = DotNetType.Resolve(type);
        }
    }

    public DotNetType? Type
    {
        get;
        set;
    }

    public bool IsArray
    {
        get;
        set;
    }

    public bool IsGenericParameter
    {
        get;
        set;
    }

    public string? GenericParameterName
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
            return Type == null ? GenericParameterName! : Type.Name;
        }
    }

    public DotNetNamespace? Namespace
    {
        get
        {
            return Type?.Namespace;
        }
    }

    public DotNetAssembly? Assembly
    {
        get
        {
            return Type?.Assembly;
        }
    }

    public string XmlDocKey
    {
        get
        {
            if (GenericParameterName != null)
            {
                return GenericParameterName;
            }

            else
            {
                StringBuilder key = new();

                if (GenericArguments != null)
                {
                    if (Namespace != null)
                    {
                        key.Append(Namespace.FullName);
                        key.Append('.');
                    }

                    key.Append(Name);

                    key.Append('{');
                    key.Append(String.Join(',', GenericArguments.Select(a => a.XmlDocKey)));
                    key.Append('}');
                }

                else
                {
                    key.Append(Type!.XmlDocKey);
                }

                if (IsArray)
                {
                    key.Append("[]");
                }

                return key.ToString();
            }
        }
    }

    public string DisplayName
    {
        get
        {
            StringBuilder output = new(Name);

            if (GenericArguments != null)
            {
                output.Append('<');
                output.Append(String.Join(", ", GenericArguments.Select(a => a.DisplayName)));
                output.Append('>');
            }

            if (IsArray)
            {
                output.Append("[]");
            }

            return output.ToString();
        }
    }
}