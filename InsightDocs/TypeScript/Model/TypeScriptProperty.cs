using InsightDocs.TypeScript.Model.Types;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptProperty: TypeScriptTypeMember
{
    public override string Title
    {
        get
        {
            return Name + " Property";
        }
    }

    public override string MemberDisplayName
    {
        get
        {
            if (SourceTypeId != 0)
            {
                TypeScriptInterface sourceType = (TypeScriptInterface)ReferenceType.AllTypes[SourceTypeId];
                return sourceType.Name + "." + Name;
            }

            else
            {
                return Name;
            }
        }
    }

    public override string ToString()
    {
        return $"{Name}: {Type}";
    }
}
