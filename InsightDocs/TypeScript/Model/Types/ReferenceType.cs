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

    public override string ToString()
    {
        string output = AllTypes[Target].Name;

        if (Arguments != null)
        {
            output += "<" + string.Join(", ", Arguments.Select(a => a.ToString())) + ">";
        }

        return output;
    }
}
