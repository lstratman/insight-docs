using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetMethodParameter
{
    public DotNetMethodParameter(ParameterInfo parameterInfo)
    {
        Name = parameterInfo.Name!;
        IsOptional = parameterInfo.IsOptional;
        Type = DotNetTypeReference.Resolve(parameterInfo.ParameterType);
    }

    public bool IsOptional
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public DotNetTypeReference Type
    {
        get;
        set;
    }

    public string? Description
    {
        get;
        set;
    }
}