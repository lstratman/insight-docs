using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model;

public class Flags
{
    [JsonProperty("isOptional")]
    public bool IsOptional
    {
        get;
        set;
    }

    [JsonProperty("isPrivate")]
    public bool IsPrivate
    {
        get;
        set;
    }

    [JsonProperty("isProtected")]
    public bool IsProtected
    {
        get;
        set;
    }
}
