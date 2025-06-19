using System.Text;
using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetType : DotNetXmlDocSource, ILinkTarget
{
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

    public required string TypeName
    {
        get;
        set;
    }

    public string Title
    {
        get
        {
            return DisplayName + " " + TypeName;
        }
    }

    public required string DisplayName
    {
        get;
        set;
    }

    public required string Name
    {
        get;
        set;
    }

    public required string FullName
    {
        get;
        set;
    }

    public bool IsSealed
    {
        get;
        set;
    }

    public bool IsAbstract
    {
        get;
        set;
    }

    public bool IsStatic
    {
        get;
        set;
    }

    public bool IsInternal
    {
        get;
        set;
    }

    public DotNetMemberInfoAccessType AccessType
    {
        get;
        set;
    }

    public DotNetTypeReference? BaseType
    {
        get;
        set;
    }

    public List<DotNetTypeReference>? ImplementedInterfaces
    {
        get;
        set;
    }

    public required DotNetAssembly Assembly
    {
        get;
        set;
    }

    public List<DotNetMethod>? Methods
    {
        get;
        set;
    }

    public List<DotNetProperty>? Properties
    {
        get;
        set;
    }

    public DotNetIndexer? Indexer
    {
        get;
        set;
    }

    public List<DotNetField>? Fields
    {
        get;
        set;
    }

    public DotNetMethod? Constructor
    {
        get;
        set;
    }

    public bool IsExternal
    {
        get;
        set;
    }

    public string XmlDocKey
    {
        get
        {
            StringBuilder key = new();

            if (Namespace != null)
            {
                key.Append(Namespace.FullName);
                key.Append('.');
            }

            key.Append(Name);

            if (TypeParameters != null && TypeParameters.Count > 0)
            {
                key.Append('`');
                key.Append(TypeParameters.Count);
            }

            return key.ToString();
        }
    }

    public string LinkText
    {
        get
        {
            return DisplayName;
        }
    }

    public required DotNetIndex Index
    {
        get;
        set;
    }

    public string VBCode
    {
        get
        {
            StringBuilder code = new StringBuilder();

            if (AccessType == DotNetMemberInfoAccessType.Public)
            {
                code.Append("Public ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Protected)
            {
                code.Append("Protected ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Private)
            {
                code.Append("Private ");
            }

            if (IsInternal)
            {
                code.Append("Family ");
            }

            if (TypeName == "Enum")
            {
                code.Append("Enum ");
                code.Append(Name);

                return code.ToString();
            }

            if (IsAbstract)
            {
                code.Append("MustInherit ");
            }

            if (IsStatic)
            {
                code.Append("Shared ");
            }

            if (IsSealed)
            {
                code.Append("NotInheritable ");
            }

            code.Append(TypeName);
            code.Append(' ');
            code.Append(Name);

            if (TypeParameters != null && TypeParameters.Count > 0)
            {
                code.Append("(Of ");
                code.Append(String.Join(", ", TypeParameters.Select(p => p.Name + (p.TypeConstraint != null ? " As " + p.TypeConstraint.VBCode : ""))));
                code.Append(')');
            }

            if (BaseType != null && BaseType.CSharpCode != "object")
            {
                code.Append("\n    Inherits ");
                code.Append(BaseType.VBCode);
            }

            if (ImplementedInterfaces != null && ImplementedInterfaces.Count > 0)
            {
                code.Append("\n    Implements ");
                code.Append(String.Join(", ", ImplementedInterfaces.Select(i => i.VBCode)));
            }

            if ((BaseType != null && BaseType.CSharpCode != "object") || ImplementedInterfaces != null && ImplementedInterfaces.Count > 0)
            {
                code.Append("\nEnd " + TypeName);
            }

            return code.ToString();
        }
    }

    public string CSharpCode
    {
        get
        {
            StringBuilder code = new StringBuilder();

            if (AccessType == DotNetMemberInfoAccessType.Public)
            {
                code.Append("public ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Protected)
            {
                code.Append("protected ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Private)
            {
                code.Append("private ");
            }

            if (IsInternal)
            {
                code.Append("internal ");
            }

            if (TypeName == "Enum")
            {
                code.Append("enum ");
                code.Append(Name);

                return code.ToString();
            }

            if (IsAbstract)
            {
                code.Append("abstract ");
            }

            if (IsStatic)
            {
                code.Append("static ");
            }

            if (IsSealed)
            {
                code.Append("sealed ");
            }

            code.Append(TypeName.ToLower());
            code.Append(' ');
            code.Append(Name);

            if (TypeParameters != null && TypeParameters.Count > 0)
            {
                code.Append('<');
                code.Append(String.Join(',', TypeParameters.Select(p => p.Name)));
                code.Append('>');
            }

            if (BaseType != null && BaseType.CSharpCode != "object")
            {
                code.Append(" : ");
                code.Append(BaseType.CSharpCode);
            }

            if (ImplementedInterfaces != null && ImplementedInterfaces.Count > 0)
            {
                if (BaseType != null && BaseType.CSharpCode != "object")
                {
                    code.Append(", ");
                }

                else
                {
                    code.Append(" : ");
                }

                code.Append(String.Join(", ", ImplementedInterfaces.Select(i => i.CSharpCode)));
            }

            if (TypeParameters != null && TypeParameters.Count(p => p.TypeConstraint != null) > 0)
            {
                code.Append(" where ");
                code.Append(String.Join(", ", TypeParameters.Where(p => p.TypeConstraint != null).Select(p => p.Name + " : " + p.TypeConstraint!.CSharpCode)));
            }

            return code.ToString();
        }
    }
}