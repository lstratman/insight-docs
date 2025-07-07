using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class ConditionalType : TypeScriptType
{
    [JsonProperty("checkType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType CheckType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("extendsType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType ExtendsType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("trueType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType TrueType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("falseType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType FalseType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override List<TypeToStringComponent> GetToStringComponents()
    {
        return
        [
            new TypeToStringTypeComponent(CheckType),
            new TypeToStringTextComponent(" extends "),
            new TypeToStringTypeComponent(ExtendsType),
            new TypeToStringTextComponent(" ? "),
            new TypeToStringTypeComponent(TrueType),
            new TypeToStringTextComponent(" : "),
            new TypeToStringTypeComponent(FalseType)
        ];
    }
}