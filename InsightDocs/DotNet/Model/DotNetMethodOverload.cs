using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace InsightDocs.DotNet.Model;

public class DotNetMethodOverload : DotNetMemberInfo
{
    [SetsRequiredMembers]
    public DotNetMethodOverload(DotNetMethodOverload method)
    {
        Name = method.Name;
        IsStatic = method.IsStatic;
        IsInternal = method.IsInternal;
        IsAbstract = method.IsAbstract;
        AccessType = method.AccessType;
        ReturnType = method.ReturnType;
        GenericArguments = method.GenericArguments == null ? null : [.. method.GenericArguments];
        Parameters = method.Parameters == null ? null : [.. method.Parameters];
        Description = method.Description;
        Remarks = method.Remarks;
        ReturnsDescription = method.ReturnsDescription;
        DeclaringType = method.DeclaringType;
    }

    public DotNetMethodOverload()
    {
    }

    public DotNetMethod? MethodCollection
    {
        get;
        set;
    }

    public required string Name
    {
        get;
        set;
    }

    public List<DotNetTypeParameter>? GenericArguments
    {
        get;
        set;
    }

    public List<DotNetMethodParameter>? Parameters
    {
        get;
        set;
    }

    public required DotNetTypeReference ReturnType
    {
        get;
        set;
    }

    public XmlDocHtml? ReturnsDescription
    {
        get;
        set;
    }

    public List<Tuple<XmlDocHtml, XmlDocHtml?>>? Exceptions
    {
        get;
        set;
    }

    public override string MemberDisplayName
    {
        get
        {
            StringBuilder output = new(Name);

            if (GenericArguments != null)
            {
                output.Append('<');
                output.Append(String.Join(", ", GenericArguments.Select(a => a.Name)));
                output.Append('>');
            }

            output.Append('(');

            if (Parameters != null)
            {
                output.Append(String.Join(", ", Parameters.Select(p => p.Type.DisplayName)));
            }

            output.Append(')');
            return output.ToString();
        }
    }

    public override string LinkText
    {
        get
        {
            return MemberDisplayName;
        }
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

            StringBuilder key = new(typeDocKey);

            key.Append('.');
            key.Append(Name);

            if (GenericArguments != null && GenericArguments.Count > 0)
            {
                key.Append('{');
                key.Append(String.Join(',', GenericArguments.Select(a => a.Name)));
                key.Append('}');
            }

            if (Parameters != null)
            {
                key.Append('(');
                key.Append(String.Join(',', Parameters.Select(p => p.Type.XmlDocKey + (p.IsByRef ? "@" : ""))));
                key.Append(')');
            }

            return key.ToString();
        }
    }

    public override string CSharpCode
    {
        get
        {
            StringBuilder output = new(base.CSharpCode);

            output.Append(ReturnType.CSharpCode);
            output.Append(' ');
            output.Append(Name);

            if (GenericArguments != null)
            {
                output.Append('<');
                output.Append(String.Join(", ", GenericArguments.Select(a => a.Name)));
                output.Append('>');
            }

            output.Append('(');

            if (Parameters != null)
            {
                output.Append(String.Join(", ", Parameters.Select(p => p.Type.CSharpCode + " " + p.Name)));
            }

            output.Append(");");
            return output.ToString();
        }
    }
}