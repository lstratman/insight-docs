using InsightDocs.Abstractions;
using InsightDocs.TypeScript.Model.Types;
using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptModule : TypeScriptDocumentedElement, ILinkTarget
{
    [JsonProperty("exportStyle")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string ExportStyle
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [JsonProperty("exportType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType ExportType
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public TypeScriptTypeDeclaration? Exports
    {
        get;
        set;
    }

    public string ShortName
    {
        get
        {
            if (Name.Count(c => c == '/') == 1 && Name.StartsWith('@'))
            {
                return Name;
            }

            return Name.Split('/').Last();
        }
    }

    public override string Title
    {
        get
        {
            return Name + " Module";
        }
    }

    public string LinkText
    {
        get
        {
            return Exports == null ? Name.Split('/').Last() : Exports.Name;
        }
    }
}
