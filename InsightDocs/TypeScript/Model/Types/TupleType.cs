using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class TupleType : TypeScriptType
{
    [JsonProperty("elements")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public List<TupleTypeElement> Elements
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override string ToString()
    {
        return "[" + string.Join(", ", Elements.Select(e => e.ToString())) + "]";
    }
}

public class TupleTypeElement
{
    [JsonProperty("name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonProperty("type")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType Type
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override string ToString()
    {
        return (string.IsNullOrEmpty(Name) ? Name + ": " : "") + Type.ToString();
    }
}
