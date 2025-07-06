using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class UnionType : TypeScriptType
{
    [JsonProperty("types")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public List<TypeScriptType> Types
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override string ToString()
    {
        return string.Join(" | ", Types.Select(t => t.ToString()));
    }
}
