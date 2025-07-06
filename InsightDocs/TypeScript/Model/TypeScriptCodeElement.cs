using InsightDocs.TypeScript.Model.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptCodeElementConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TypeScriptCodeElement);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return ReadJson(JObject.Load(reader), serializer);
    }

    public static object ReadJson(JObject jsonObject, JsonSerializer serializer)
    {
        if (!jsonObject.ContainsKey("kind"))
        {
            throw new Exception("TypeScript type declaration JSON object does not contain 'kind' property.");
        }

        TypeScriptCodeElement memberObject = jsonObject["kind"]!.Value<string>() switch
        {
            "method" => new TypeScriptMethod(),
            "signature" => new TypeScriptMethodSignature(),
            "property" => new TypeScriptProperty(),
            "parameter" => new TypeScriptParameter(),
            _ => throw new Exception("Unrecognized member for " + jsonObject["name"]!.Value<string>() + ", kind " + jsonObject["kind"]!.Value<string>() + "."),
        };

        serializer.Populate(jsonObject.CreateReader(), memberObject);
        return memberObject;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

[JsonConverter(typeof(TypeScriptCodeElementConverter))]
public abstract class TypeScriptCodeElement : TypeScriptDocumentedElement
{
    [JsonProperty("type")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType Type
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public int SourceTypeId
    {
        get;
        set;
    }

    public string? Url
    {
        get;
        set;
    }

    [JsonProperty("comment")]
    public Comment? Comment
    {
        get;
        set;
    }

    [JsonProperty("flags")]
    public Flags? Flags
    {
        get;
        set;
    }
}
