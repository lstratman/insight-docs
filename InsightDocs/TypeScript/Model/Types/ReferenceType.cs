using InsightDocs.Abstractions;
using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class ReferenceType : TypeScriptType
{
    public static Dictionary<int, TypeScriptTypeDeclaration> AllTypes = [];
    public static Dictionary<string, TypeScriptTypeDeclaration> AllTypesByName = [];
    public static IUrlProvider<TypeScriptMethodSignature>? MethodSignatureUrlProvider;
    public static IUrlProvider<TypeScriptProperty>? PropertyUrlProvider;

    public static string GetUrl(TypeScriptCodeElement element)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (element is TypeScriptMethodSignature methodSignature)
        {
            return MethodSignatureUrlProvider == null
                ? throw new Exception("No URL provider registered for TypeScriptMethodSignature")
                : MethodSignatureUrlProvider.GetUrl(methodSignature);
        }

        else if (element is TypeScriptProperty property)
        {
            return PropertyUrlProvider == null
                ? throw new Exception("No URL provider registered for TypeScriptProperty")
                : PropertyUrlProvider.GetUrl(property);
        }

        else
        {
            throw new Exception("Unsupported TypeScript code element type: " + element.GetType().FullName);
        }
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    [JsonProperty("id")]
    public int Target
    {
        get;
        set;
    }

    [JsonProperty("typeArguments")]
    public List<TypeScriptType>? Arguments
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        List<TypeToStringComponent> components = [new TypeToStringReferenceTypeComponent(Target)];

        if (Arguments != null && Arguments.Count > 0)
        {
            components.Add(new TypeToStringTextComponent("<"));

            bool first = true;

            foreach (TypeScriptType arg in Arguments)
            {
                if (!first)
                {
                    components.Add(new TypeToStringTextComponent(", "));
                }

                first = false;
                components.Add(new TypeToStringTypeComponent(arg));
            }

            components.Add(new TypeToStringTextComponent(">"));
        }

        return components;
    }
}
