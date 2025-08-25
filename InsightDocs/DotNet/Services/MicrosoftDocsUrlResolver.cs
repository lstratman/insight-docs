using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;

namespace InsightDocs.DotNet.Services;

public class MicrosoftDocsUrlResolver : IMicrosoftDocsUrlResolver
{
    public virtual string GetUrl(DotNetType type)
    {
        return $"https://learn.microsoft.com/dotnet/api/{type.FullName.ToLower()}{(type.TypeParameters != null && type.TypeParameters.Count != 0 ? "-" + type.TypeParameters.Count.ToString() : "")}";
    }

    public virtual string GetUrl(DotNetMethodOverload method)
    {
        // TODO: add overload anchors
        return $"https://learn.microsoft.com/dotnet/api/{method.DeclaringType!.Type!.FullName.ToLower()}{(method.DeclaringType.Type.TypeParameters != null && method.DeclaringType.Type.TypeParameters.Count != 0 ? "-" + method.DeclaringType.Type.TypeParameters.Count.ToString() : "")}.{(method.Name.StartsWith("add_") || method.Name.StartsWith("remove_") || method.Name.StartsWith("op_") ? method.Name[(method.Name.IndexOf("_") + 1)..].ToLower() : method.Name.ToLower())}";
    }

    public virtual string GetUrl(DotNetProperty property)
    {
        return $"https://learn.microsoft.com/dotnet/api/{property.DeclaringType!.Type!.FullName.ToLower()}{(property.DeclaringType.Type.TypeParameters != null && property.DeclaringType.Type.TypeParameters.Count != 0 ? "-" + property.DeclaringType.Type.TypeParameters.Count.ToString() : "")}.{property.Name.ToLower()}";
    }

    public virtual string GetUrl(DotNetField field)
    {
        return $"https://learn.microsoft.com/dotnet/api/{field.DeclaringType!.Type!.FullName.ToLower()}{(field.DeclaringType.Type.TypeParameters != null && field.DeclaringType.Type.TypeParameters.Count != 0 ? "-" + field.DeclaringType.Type.TypeParameters.Count.ToString() : "")}.{field.Name.ToLower()}";
    }

    public virtual string GetUrl(DotNetNamespace ns)
    {
        return $"https://learn.microsoft.com/dotnet/api/{ns.FullName.ToLower()}";
    }

    public virtual bool IsMicrosoftType(DotNetType type)
    {
        return type.Namespace != null && IsMicrosoftNamespace(type.Namespace.FullName);
    }

    public virtual bool IsMicrosoftNamespace(string ns)
    {
        return ns == "Microsoft" || ns == "System" || ns.StartsWith("Microsoft.") || ns.StartsWith("System.");
    }
}
