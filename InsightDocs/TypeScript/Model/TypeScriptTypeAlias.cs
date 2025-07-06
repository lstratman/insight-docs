using InsightDocs.TypeScript.Model.Types;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptTypeAlias : TypeScriptNamespacedTypeDeclaration
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TypeScriptType Type
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    public override string Title
    {
        get
        {
            if (Type is ReflectionType reflectionType)
            {
                if (reflectionType.IndexSignature != null && reflectionType.Members == null)
                {
                    return Name + " Function";
                }
            }

            return Name + " Type";
        }
    }
}
