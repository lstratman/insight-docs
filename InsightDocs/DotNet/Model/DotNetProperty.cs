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
            return Name;
        }
    }

    public List<DotNetMethodParameter>? IndexParameters
    {
        get;
        set;
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
            code.Append(Name);

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