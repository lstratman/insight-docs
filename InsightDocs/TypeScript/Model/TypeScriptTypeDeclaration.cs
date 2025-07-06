using InsightDocs.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptTypeDeclarationConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TypeScriptTypeDeclaration);
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

        TypeScriptTypeDeclaration typeDeclarationObject = jsonObject["kind"]!.Value<string>() switch
        {
            "interface" => new TypeScriptInterface(),
            "alias" => new TypeScriptTypeAlias(),
            "enum" => new TypeScriptEnum(),
            "variable" => new TypeScriptVariable(),
            _ => throw new Exception("Unrecognized TypeScript type declaration type: " + jsonObject["kind"]!.Value<string>() + "."),
        };

        serializer.Populate(jsonObject.CreateReader(), typeDeclarationObject);
        return typeDeclarationObject;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public abstract class TypeScriptNamespacedTypeDeclaration : TypeScriptTypeDeclaration
{
    public TypeScriptNamespace? Namespace
    {
        get;
        set;
    }
}

[JsonConverter(typeof(TypeScriptTypeDeclarationConverter))]
public abstract class TypeScriptTypeDeclaration : ILinkTarget
{
    public static readonly Dictionary<string, string> MDNUrlMappings = new Dictionary<string, string>
    {
        { "https://developer.mozilla.org/docs/Web/API/HTMLElement/cancel_event", "https://developer.mozilla.org/docs/Web/API/HTMLElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLDialogElement/cancel_event", "https://developer.mozilla.org/docs/Web/API/HTMLElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLVideoElement/resize_event", "https://developer.mozilla.org/docs/Web/API/VisualViewport/resize_event" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement/cells", "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement/sectionRowIndex", "https://developer.mozilla.org/docs/Web/API/HTMLTableRowElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/colSpan", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/cellIndex", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/abbr", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/rowSpan", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/headers", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" },
        { "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement/scope", "https://developer.mozilla.org/docs/Web/API/HTMLTableCellElement" }
    };

    public virtual int Id
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

    [JsonProperty("fullName")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public virtual string FullName
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
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

    [JsonProperty("builtIn")]
    public bool BuiltIn
    {
        get;
        set;
    }

    public string? Url
    {
        get;
        set;
    }

    public abstract string Title
    {
        get;
    }

    public virtual string LinkText
    {
        get
        {
            return Name;
        }
    }

    public virtual TypeScriptProject? Project
    {
        get;
        set;
    }

    public virtual void Normalize()
    {
    }
}
