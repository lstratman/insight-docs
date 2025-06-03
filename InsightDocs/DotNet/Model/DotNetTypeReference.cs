using System.Text;

namespace InsightDocs.DotNet.Model;

public class DotNetTypeReference
{
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