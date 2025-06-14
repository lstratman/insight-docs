using System.Text;

namespace InsightDocs.DotNet.Model;

public class DotNetTypeReference
{
    protected static Dictionary<string, string> CSharpTypeNames = new()
    {
        { "System.Int32", "int" },
        { "System.String", "string" },
        { "System.Boolean", "bool" },
        { "System.Double", "double" },
        { "System.Single", "float" },
        { "System.Decimal", "decimal" },
        { "System.Object", "object" },
        { "System.Void", "void" },
        { "System.Char", "char" },
        { "System.Byte", "byte" },
        { "System.SByte", "sbyte" },
        { "System.Int16", "short" },
        { "System.UInt16", "ushort" },
        { "System.Int64", "long" },
        { "System.UInt64", "ulong" },
        { "System.IntPtr", "nint" },
        { "System.UIntPtr", "nuint" }
    };

    protected static Dictionary<string, string> VBTypeNames = new()
    {
        { "System.Int32", "Integer" }
    };

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

    public string VBCode
    {
        get
        {
            if (!VBTypeNames.TryGetValue(Namespace?.FullName + "." + Name, out string? name))
            {
                name = Name;
            }

            StringBuilder code = new StringBuilder(name);

            if (GenericArguments != null && GenericArguments.Count > 0)
            {
                code.Append("(Of ");
                code.Append(String.Join(", ", GenericArguments.Select(a => a.VBCode)));
                code.Append(')');
            }

            if (IsArray)
            {
                code.Append("()");
            }

            return code.ToString();
        }
    }

    public string CSharpCode
    {
        get
        {
            if (!CSharpTypeNames.TryGetValue(Namespace?.FullName + "." + Name, out string? name))
            {
                name = Name;
            }

            StringBuilder code = new StringBuilder(name);

            if (GenericArguments != null && GenericArguments.Count > 0)
            {
                code.Append('<');
                code.Append(String.Join(", ", GenericArguments.Select(a => a.CSharpCode)));
                code.Append('>');
            }

            if (IsArray)
            {
                code.Append("[]");
            }

            return code.ToString();
        }
    }
}