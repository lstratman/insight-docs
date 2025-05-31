using System.Reflection;

namespace InsightDocs.DotNet.Model;

public class DotNetMethodParameter(ParameterInfo parameterInfo, IServiceProvider serviceProvider)
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
    } = DotNetTypeReference.Resolve(parameterInfo.ParameterType, serviceProvider);

    public XmlDocHtml? Description
    {
        get;
        set;
    }
}