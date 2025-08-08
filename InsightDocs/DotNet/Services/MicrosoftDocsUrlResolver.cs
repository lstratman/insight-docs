using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;

namespace InsightDocs.DotNet.Services;

public class MicrosoftDocsUrlResolver : IMicrosoftDocsUrlResolver
{
    public string GetUrl(DotNetType type)
    {
        return $"https://learn.microsoft.com/dotnet/api/{type.FullName.ToLower()}{(type.TypeParameters != null && type.TypeParameters.Count != 0 ? "-" + type.TypeParameters.Count.ToString() : "")}";
    }

    public string GetUrl(DotNetMethodOverload method)
    {
        return $"https://learn.microsoft.com/dotnet/api/{method.DeclaringType!.Type!.FullName.ToLower()}.{method.Name.ToLower()}{(method.GenericArguments != null && method.GenericArguments.Count != 0 ? "-" + method.GenericArguments.Count.ToString() : "")}";
    }

    public string GetUrl(DotNetProperty property)
    {
        return $"https://learn.microsoft.com/dotnet/api/{property.DeclaringType!.Type!.FullName.ToLower()}.{property.Name.ToLower()}";
    }

    public string GetUrl(DotNetField field)
    {
        return $"https://learn.microsoft.com/dotnet/api/{field.DeclaringType!.Type!.FullName.ToLower()}.{field.Name.ToLower()}";
    }

    public string GetUrl(DotNetNamespace ns)
    {
        return $"https://learn.microsoft.com/dotnet/api/{ns.FullName.ToLower()}";
    }

    public bool IsMicrosoftType(DotNetType type)
    {
        return type.Namespace != null && IsMicrosoftNamespace(type.Namespace.FullName);
    }

    public bool IsMicrosoftNamespace(string ns)
    {
        return ns == "Microsoft" || ns == "System" || ns.StartsWith("Microsoft.") || ns.StartsWith("System.");
    }
}
