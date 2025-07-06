using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptEnum : TypeScriptNamespacedTypeDeclaration
{
    public override string Title
    {
        get
        {
            return Name + " Enumeration";
        }
    }

    [JsonProperty("values")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public List<EnumValue> EnumValues
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }
}

public class EnumValue
{
    [JsonProperty("name")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }
}
