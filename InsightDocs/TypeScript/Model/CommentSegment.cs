using InsightDocs.TypeScript.Model.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsightDocs.TypeScript.Model;

public class CommentSegmentConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(CommentSegment);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return ReadJson(JObject.Load(reader), serializer);
    }

    public static object ReadJson(JObject jsonObject, JsonSerializer serializer)
    {
        if (!jsonObject.ContainsKey("kind"))
        {
            throw new Exception("Comment segment JSON object does not contain 'kind' property.");
        }

        CommentSegment commentSegmentObject = jsonObject["kind"]!.Value<string>() switch
        {
            "text" => new TextCommentSegment(),
            "code" => new CodeCommentSegment(),
            "link" => new LinkTagCommentSegment(),
            "parameterName" => new ParameterNameCommentSegment(),
            "type" => new TypeDefinitionCommentSegment(),
            _ => throw new Exception("Unrecognized comment kind for " + jsonObject["kind"]!.Value<string>() + "."),
        };

        serializer.Populate(jsonObject.CreateReader(), commentSegmentObject);
        return commentSegmentObject;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

[JsonConverter(typeof(CommentSegmentConverter))]
public class CommentSegment
{
    [JsonProperty("text")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string Text
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public virtual string GetMarkdown()
    {
        return Text;
    }
}

public class ParameterNameCommentSegment : CommentSegment
{
}

public class TextCommentSegment : CommentSegment
{
}

public class CodeCommentSegment : CommentSegment
{
}

public class TypeDefinitionCommentSegment : CommentSegment
{
}

public class LinkTagCommentSegment : CommentSegment
{
    [JsonProperty("linkText")]
    public string? LinkText
    {
        get;
        set;
    }

    public TypeScriptCodeElement? TargetElement
    {
        get;
        set;
    }

    public string? Url
    {
        get;
        set;
    }

    public override string GetMarkdown()
    {
        if (String.IsNullOrEmpty(Url))
        {
            string lookupIndex = (string.IsNullOrEmpty(LinkText) ? Text : LinkText).Replace("#", ".");

            if (ReferenceType.AllTypesByName.TryGetValue(lookupIndex, out TypeScriptTypeDeclaration? type))
            {
                return $"[{(string.IsNullOrEmpty(LinkText) ? Text : LinkText).Replace("#", ".")}]({type.Url})";
            }

            else
            {
                string memberName = lookupIndex[(lookupIndex.LastIndexOf('.') + 1)..];
                lookupIndex = lookupIndex[..lookupIndex.LastIndexOf('.')];

                if (!ReferenceType.AllTypesByName.TryGetValue(lookupIndex, out TypeScriptTypeDeclaration? targetTypeDeclaration))
                {
                    throw new Exception("Unable to resolve the JSDoc comment link to " + (string.IsNullOrEmpty(LinkText) ? Text : LinkText));
                }

                if (targetTypeDeclaration is TypeScriptInterface targetType)
                {
#pragma warning disable IDE0045 // Convert to conditional expression
                    if (targetType.Methods != null && targetType.Methods.TryGetValue(memberName, out TypeScriptMethod? value))
                    {
                        TargetElement = value.Signatures[0];
                    }

                    else if (targetType.Properties != null && targetType.Properties.Any(p => p.Name == memberName))
                    {
                        TargetElement = targetType.Properties.First(p => p.Name == memberName);
                    }

                    else
                    {
                        throw new Exception("Unable to resolve the JSDoc comment link to " + (string.IsNullOrEmpty(LinkText) ? Text : LinkText));
                    }
#pragma warning restore IDE0045 // Convert to conditional expression
                }

                else
                {
                    throw new Exception("Unable to resolve the JSDoc comment link to " + (string.IsNullOrEmpty(LinkText) ? Text : LinkText));
                }
            }

            Url = TargetElement.Url;
        }

        string text = (string.IsNullOrEmpty(LinkText) ? Text : LinkText) + (TargetElement is not null and TypeScriptMethodSignature ? "()" : "");
        return $"[{text.Replace("#", ".")}]({Url})";
    }
}
