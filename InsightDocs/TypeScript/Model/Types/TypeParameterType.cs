using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class TypeParameterType : TypeScriptType
{
    [JsonProperty("name")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("constraint")]
    public TypeScriptType? Constraint
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        List<TypeToStringComponent> components = [new TypeToStringTextComponent(Name)];

        if (Constraint != null)
        {
            components.Add(new TypeToStringTextComponent(" extends "));
            components.Add(new TypeToStringTypeComponent(Constraint));
        }

        return components;
    }
}
