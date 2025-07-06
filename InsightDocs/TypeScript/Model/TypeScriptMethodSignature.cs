using InsightDocs.Abstractions;
using InsightDocs.TypeScript.Model.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptMethodSignature : TypeScriptTypeMember
{
    [JsonProperty("parameters")]
    public List<TypeScriptParameter>? Parameters
    {
        get;
        set;
    }

    [JsonProperty("returnType")]
    public TypeScriptType ReturnType
    {
        get
        {
            return Type;
        }

        set
        {
            Type = value;
        }
    }

    protected List<TypeParameter>? _typeParameters;

    [JsonProperty("typeParameters")]
    public List<TypeParameter>? TypeParameters
    {
        get
        {
            return _typeParameters;
        }

        set
        {
            _typeParameters = value?.Distinct(new TypeParameter.Comparer()).ToList();
        }
    }

    public override string Title
    {
        get
        {
            return ToString() + " Method";
        }
    }

    public override string MemberDisplayName
    {
        get
        {
            TypeScriptInterface? sourceType = SourceTypeId == 0 ? null : (TypeScriptInterface)ReferenceType.AllTypes[SourceTypeId];
            StringBuilder output = new(Name == "constructor" ? sourceType!.Name : Name);

            if (TypeParameters != null)
            {
                output.Append('<');
                output.Append(String.Join(", ", TypeParameters.Select(a => a.Name)));
                output.Append('>');
            }

            output.Append('(');

            if (Parameters != null)
            {
                output.Append(String.Join(", ", Parameters.Select(p => p.Type.ToString())));
            }

            output.Append(')');
            return output.ToString();
        }
    }

    public override string ToString()
    {
        string displayName = "";

        if (TypeParameters != null && TypeParameters.Count > 0)
        {
            displayName += "&lt;";

            for (int i = 0; i < TypeParameters.Count; i++)
            {
                if (i > 0)
                {
                    displayName += ", ";
                }

                displayName += TypeParameters[i].Name;

                if (TypeParameters[i].Constraint != null)
                {
                    displayName += " extends " + TypeParameters[i].Constraint!.ToString();
                }
            }

            displayName += "&gt;";
        }
            
        if (Parameters != null && Parameters.Count > 0)
        {
            if (Parameters[0].Type is StringLiteralType)
            {
                displayName += "(" + string.Join(", ", Parameters.Select(p => p.Type is StringLiteralType ? p.Type.ToString() : p.Name)) + ")";
            }

            else
            {
                displayName += "(" + string.Join(", ", Parameters.Select(p => p.Type.ToString())) + ")";
            }
        }

        return displayName;
    }
}
