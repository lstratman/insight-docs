using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model.Types;

public class FunctionType : TypeScriptType
{
    [JsonProperty("name")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("returnType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType ReturnType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("parameters")]
    public List<TypeScriptParameter>? Parameters
    {
        get;
        set;
    }

    public override string ToString()
    {
        string displayName = "";

        if (Parameters != null && Parameters.Count > 0)
        {
            if (Parameters[0].Type is StringLiteralType)
            {
                displayName += "(" + string.Join(", ", Parameters.Select(p => p.Type is StringLiteralType ? p.Type.ToString() : p.Name)) + ")";
            }

            else
            {
                displayName += "(" + string.Join(", ", Parameters.Select(p => p.Type.ToString())) + ")";
            }
        }

        displayName += " => " + ReturnType.ToString();

        return displayName;
    }
}
