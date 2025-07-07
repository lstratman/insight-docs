using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class PredicateType : TypeScriptType
{
    [JsonProperty("parameterName")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("type")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType TargetType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        return
        [
            new TypeToStringTextComponent($"{Name} is "),
            new TypeToStringTypeComponent(TargetType)
        ];
    }
}
