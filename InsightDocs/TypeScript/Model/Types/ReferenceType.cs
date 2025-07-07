using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class ReferenceType : TypeScriptType
{
    public static Dictionary<int, TypeScriptTypeDeclaration> AllTypes = [];
    public static Dictionary<string, TypeScriptTypeDeclaration> AllTypesByName = [];

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
