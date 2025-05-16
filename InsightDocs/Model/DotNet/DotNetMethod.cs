using System.Reflection;
using System.Text;

namespace InsightDocs.Model.DotNet;

public class DotNetMethod
{
    public DotNetMethod(MethodInfo method)
    {
        Name = method.Name;

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

    public string? Description
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
}