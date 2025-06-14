using System.Diagnostics.CodeAnalysis;
using System.Text;
using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetProperty : DotNetXmlDocSource, ILinkTarget
{
    [SetsRequiredMembers]
    public DotNetProperty(DotNetProperty property)
    {
        Name = property.Name;
        PropertyType = property.PropertyType;
        DeclaringType = property.DeclaringType;
        GetMethod = property.GetMethod == null ? null : new DotNetMethodOverload(property.GetMethod);
        SetMethod = property.SetMethod == null ? null : new DotNetMethodOverload(property.SetMethod);
        Description = property.Description;
        Remarks = property.Remarks;
        DeclaringType = property.DeclaringType;
    }

    public DotNetProperty()
    {
    }

    public DotNetMethodOverload? GetMethod
    {
        get;
        set;
    }

    public DotNetMethodOverload? SetMethod
    {
        get;
        set;
    }

    public required string Name
    {
        get;
        set;
    }

    public required DotNetTypeReference PropertyType
    {
        get;
        set;
    }

    public DotNetTypeReference? DeclaringType
    {
        get;
        set;
    }

    public XmlDocHtml? ValueDescription
    {
        get;
        set;
    }

    public bool IsIndexer
    {
        get;
        set;
    }

    public string? XmlDocKey
    {
        get
        {
            string? typeDocKey = DeclaringType?.XmlDocKey;

            if (typeDocKey == null)
            {
                return null;
            }

            StringBuilder key = new(typeDocKey + "." + Name);

            if (IndexParameters != null && IndexParameters.Count > 0)
            {
                key.Append('[');
                key.Append(String.Join(',', IndexParameters.Select(p => p.Type.XmlDocKey)));
                key.Append(']');
            }

            return key.ToString();
        }
    }

    public string MemberDisplayName
    {
        get
        {
            if (IsIndexer)
            {
                if (IndexParameters != null && IndexParameters.Count > 0)
                {
                    return "this[" + String.Join(", ", IndexParameters.Select(p => p.Type.DisplayName + " " + p.Name)) + "]";
                }

                else
                {
                    return "this[]";
                }
            }

            else
            {
                if (DeclaringType != null)
                {
                    return DeclaringType.DisplayName + "." + Name;
                }

                else
                {
                    return Name;
                }
            }
        }
    }

    public string Title
    {
        get
        {
            return MemberDisplayName + " Property";
        }
    }

    public string LinkText
    {
        get
        {
            if (IsIndexer)
            {
                if (IndexParameters != null && IndexParameters.Count > 0)
                {
                    return "this[" + String.Join(", ", IndexParameters.Select(p => p.Type.DisplayName + " " + p.Name)) + "]";
                }

                else
                {
                    return "this[]";
                }
            }

            else
            {
                return Name;
            }
        }
    }

    public List<DotNetMethodParameter>? IndexParameters
    {
        get;
        set;
    }

    public string VBCode
    {
        get
        {
            StringBuilder code = new StringBuilder();
            DotNetMethodOverload method = GetMethod ?? SetMethod!;

            if (method.AccessType == DotNetMemberInfoAccessType.Public)
            {
                code.Append("Public ");
            }

            else if (method.AccessType == DotNetMemberInfoAccessType.Protected)
            {
                code.Append("Protected ");
            }

            else if (method.AccessType == DotNetMemberInfoAccessType.Private)
            {
                code.Append("Private ");
            }

            if (method.IsInternal)
            {
                code.Append("Family ");
            }

            if (method.IsAbstract)
            {
                code.Append("MustOverride ");
            }

            if (method.IsStatic)
            {
                code.Append("Shared ");
            }

            if (GetMethod != null && SetMethod == null)
            {
                code.Append("ReadOnly ");
            }

            if (SetMethod != null && GetMethod == null)
            {
                code.Append("WriteOnly ");
            }

            code.Append("Property ");

            if (IsIndexer)
            {
                code.Append("Item(");

                if (IndexParameters != null && IndexParameters.Count > 0)
                {
                    code.Append(String.Join(", ", IndexParameters.Select(p => p.Name + " As " + p.Type.VBCode)));
                }

                code.Append(')');
            }

            else
            {
                code.Append(Name);
            }

            code.Append(" As ");
            code.Append(PropertyType.VBCode);

            return code.ToString();
        }
    }

    public string CSharpCode
    {
        get
        {
            StringBuilder code = new StringBuilder();
            DotNetMethodOverload method = GetMethod ?? SetMethod!;

            if (method.AccessType == DotNetMemberInfoAccessType.Public)
            {
                code.Append("public ");
            }

            else if (method.AccessType == DotNetMemberInfoAccessType.Protected)
            {
                code.Append("protected ");
            }

            else if (method.AccessType == DotNetMemberInfoAccessType.Private)
            {
                code.Append("private ");
            }

            if (method.IsInternal)
            {
                code.Append("internal ");
            }

            if (method.IsAbstract)
            {
                code.Append("abstract ");
            }

            if (method.IsStatic)
            {
                code.Append("static ");
            }

            code.Append(PropertyType.CSharpCode);
            code.Append(' ');

            if (IsIndexer)
            {
                code.Append("this[");

                if (IndexParameters != null && IndexParameters.Count > 0)
                {
                    code.Append(String.Join(", ", IndexParameters.Select(p => p.Type.CSharpCode + " " + p.Name)));
                }

                code.Append(']');
            }

            else
            {
                code.Append(Name);
            }

            code.Append(" {");

            if (GetMethod != null)
            {
                code.Append(" get;");
            }

            if (SetMethod != null)
            {
                if (GetMethod != null && SetMethod.AccessType != GetMethod.AccessType)
                {
                    if (SetMethod.AccessType == DotNetMemberInfoAccessType.Public)
                    {
                        code.Append(" public");
                    }

                    else if (SetMethod.AccessType == DotNetMemberInfoAccessType.Protected)
                    {
                        code.Append(" protected");
                    }

                    else if (SetMethod.AccessType == DotNetMemberInfoAccessType.Private)
                    {
                        code.Append(" private");
                    }
                }

                code.Append(" set;");
            }

            code.Append(" }");

            return code.ToString();
        }
    }
}