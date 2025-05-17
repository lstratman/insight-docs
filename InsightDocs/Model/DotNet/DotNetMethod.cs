using System.Reflection;
using System.Text;

namespace InsightDocs.Model.DotNet;

public class DotNetMethod : DotNetMemberInfo
{
    public DotNetMethod(MethodInfo method)
    {
        Name = method.Name;
        IsStatic = method.IsStatic;
        IsInternal = method.IsAssembly;
        IsAbstract = method.IsAbstract;

        if (method.IsPublic)
        {
            AccessType = DotNetMemberInfoAccessType.Public;
        }

        else if (method.IsPrivate)
        {
            AccessType = DotNetMemberInfoAccessType.Private;
        }

        else if (method.IsFamily)
        {
            AccessType = DotNetMemberInfoAccessType.Protected;
        }

        if (Name.Contains('.'))
        {
            Name = Name[(Name.LastIndexOf('.') + 1)..];
        }

        Type[] genericArguments = method.GetGenericArguments();

        if (genericArguments != null && genericArguments.Length > 0)
        {
            GenericArguments = [.. genericArguments.Select(a => new DotNetTypeParameter(a))];
        }

        ParameterInfo[] parameters = method.GetParameters();

        if (parameters != null && parameters.Length > 0)
        {
            Parameters = [..parameters.Select(p => new DotNetMethodParameter(p))];
        }

        ReturnType = DotNetTypeReference.Resolve(method.ReturnType);

        if (method.DeclaringType != null)
        {
            DeclaringType = DotNetTypeReference.Resolve(method.DeclaringType);

            if (DeclaringType.Type?.Assembly?.XmlDocEntries != null && XmlDocKey != null)
            {
                DeclaringType.Type.Assembly.XmlDocEntries.TryGetValue("M:" + XmlDocKey, out XmlDocEntry? xmlDocEntry);

                if (xmlDocEntry != null)
                {
                    if (xmlDocEntry.Summary != null)
                    {
                        Description = new XmlDocHtml(xmlDocEntry.Summary);
                    }

                    if (xmlDocEntry.Remarks != null)
                    {
                        Remarks = new XmlDocHtml(xmlDocEntry.Remarks);
                    }

                    if (xmlDocEntry.Returns != null)
                    {
                        ReturnsDescription = new XmlDocHtml(xmlDocEntry.Returns);
                    }

                    if (Parameters != null && xmlDocEntry.Parameters != null)
                    {
                        foreach (DotNetMethodParameter parameter in Parameters)
                        {
                            if (xmlDocEntry.Parameters.TryGetValue(parameter.Name, out List<XmlDocCommentComponent>? components))
                            {
                                parameter.Description = new XmlDocHtml(components);
                            }
                        }
                    }
                }
            }
        }
    }

    public string LinkTitle
    {
        get
        {
            return DisplayName;
        }
    }

    public string Name
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

    public DotNetTypeReference ReturnType
    {
        get;
        set;
    }

    public XmlDocHtml? ReturnsDescription
    {
        get;
        set;
    }

    public DotNetTypeReference? DeclaringType
    {
        get;
        set;
    }

    public string DisplayName
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
            key.Append('(');

            if (Parameters != null)
            {
                key.Append(String.Join(',', Parameters.Select(p => p.Type.XmlDocKey)));
            }

            key.Append(')');

            return key.ToString();
        }
    }
}