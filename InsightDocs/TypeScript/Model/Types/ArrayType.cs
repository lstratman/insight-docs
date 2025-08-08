using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class ArrayType : TypeScriptType
{
    [JsonProperty("elementType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType ElementType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        return
        [
            new TypeToStringTypeComponent(ElementType),
            new TypeToStringTextComponent("[]")
        ];
    }
}
