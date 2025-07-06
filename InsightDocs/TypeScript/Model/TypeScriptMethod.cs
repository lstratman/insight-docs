using InsightDocs.TypeScript.Model.Types;
using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptMethod: TypeScriptTypeMember
{
    [JsonProperty("signatures")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public List<TypeScriptMethodSignature> Signatures
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override string Title
    {
        get
        {
            string displayName = Name;

            if (Signatures.Count == 1 && Signatures[0].TypeParameters != null && Signatures[0].TypeParameters!.Count > 0)
            {
                displayName += "&lt;";

                for (int i = 0; i < Signatures[0].TypeParameters!.Count; i++)
                {
                    if (i > 0)
                    {
                        displayName += ", ";
                    }

                    displayName += Signatures[0].TypeParameters![i].Name;
                }

                displayName += "&gt;";
            }

            return displayName + (Name == "constructor" ? " Constructor" : " Method");
        }
    }

    public override string MemberDisplayName
    {
        get
        {
            if (SourceTypeId != 0)
            {
                TypeScriptInterface sourceType = (TypeScriptInterface)ReferenceType.AllTypes[SourceTypeId];
                return Name == "constructor" ? sourceType.Name : sourceType.Name + "." + (Signatures.Count == 1 ? Signatures[0].MemberDisplayName : Name);
            }

            else
            {
                return Signatures.Count == 1 ? Signatures[0].MemberDisplayName : Name;
            }
        }
    }
}
