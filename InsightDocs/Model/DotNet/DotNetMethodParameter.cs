using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetMethodParameter(ParameterInfo parameterInfo)
{
    public bool IsOptional
    {
        get;
        set;
    } = parameterInfo.IsOptional;

    public bool IsOut
    {
        get;
        set;
    } = parameterInfo.IsOut;

    public bool IsByRef
    {
        get;
        set;
    } = parameterInfo.ParameterType.IsByRef;

    public string Name
    {
        get;
        set;
    } = parameterInfo.Name!;

    public DotNetTypeReference Type
    {
        get;
        set;
    } = DotNetTypeReference.Resolve(parameterInfo.ParameterType);

    public XmlDocHtml? Description
    {
        get;
        set;
    }
}