using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.TypeScript.Model.Types;

public class ReflectionType : TypeScriptType
{
    [JsonProperty("members")]
    public List<TypeScriptCodeElement>? Members
    {
        get;
        set;
    }

    [JsonProperty("indexSignature")]
    public TypeScriptMethodSignature? IndexSignature
    {
        get;
        set;
    }

    public override string ToString()
    {
        List<string> members = [];

        if (IndexSignature != null)
        {
            members.Add("[" + (IndexSignature.Parameters == null ? "" : String.Join(", ", IndexSignature.Parameters.Select(p => p.Type.ToString()))) + "]: " + (IndexSignature.Type == null ? "any" : IndexSignature.Type.ToString()));
        }

        if (Members != null)
        {
            members.AddRange(Members.Select(m => m.Type == null ? "any" : m.Type.ToString()).Cast<string>());
        }

        return "{ " + string.Join(", ", members) + " }";
    }
}
