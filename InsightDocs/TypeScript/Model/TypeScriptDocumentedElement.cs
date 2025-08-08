using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptDocumentedElementConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TypeScriptDocumentedElement);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        if (!jsonObject.ContainsKey("kind"))
        {
            throw new Exception("TypeScript type declaration JSON object does not contain 'kind' property.");
        }

        TypeScriptDocumentedElement memberObject;

        switch (jsonObject["kind"]!.Value<string>())
        {
            case "module":
                memberObject = new TypeScriptModule();
                break;

            default:
                return TypeScriptCodeElementConverter.ReadJson(jsonObject, serializer);
        }

        serializer.Populate(jsonObject.CreateReader(), memberObject);
        return memberObject;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

[JsonConverter(typeof(TypeScriptDocumentedElementConverter))]
public class TypeScriptDocumentedElement
{
    [JsonProperty("id")]
    public virtual int Id
    {
        get;
        set;
    }

    [JsonProperty("kind")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public virtual string Kind
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("name")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public virtual string Name
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public virtual string Title
    {
        get
        {
            return Name;
        }
    }

    public virtual string FullName
    {
        get
        {
            return Name;
        }
    }
}
