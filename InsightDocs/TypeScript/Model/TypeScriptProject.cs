using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptProject
{
    [JsonProperty("modules")]
    public Dictionary<string, TypeScriptModule>? Modules
    {
        get;
        set;
    }

    [JsonProperty("types")]
    public Dictionary<string, TypeScriptTypeDeclaration>? Types
    {
        get;
        set;
    }

    public Dictionary<string, TypeScriptNamespace>? Namespaces
    {
        get;
        set;
    }
}
