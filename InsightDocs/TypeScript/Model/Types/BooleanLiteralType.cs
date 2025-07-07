using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class BooleanLiteralType : TypeScriptType
{
    [JsonProperty("literalValue")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Value
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        return
        [
            new TypeToStringTextComponent(Value.ToString().ToLower())
        ];
    }
}