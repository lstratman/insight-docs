using InsightDocs.Abstractions;

namespace InsightDocs.TypeScript.Model;

public abstract class TypeScriptTypeMember : TypeScriptCodeElement, ILinkTarget
{
    public abstract string MemberDisplayName
    {
        get;
    }

    public virtual string LinkText
    {
        get
        {
            return MemberDisplayName;
        }
    }
}
