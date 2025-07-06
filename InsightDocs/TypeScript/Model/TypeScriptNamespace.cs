using InsightDocs.Abstractions;

namespace InsightDocs.TypeScript.Model;

public class TypeScriptNamespace : ILinkTarget
{
    public required string Name
    {
        get;
        set;
    }

    public string LinkText
    {
        get
        {
            return Name;
        }
    }

    public List<TypeScriptTypeDeclaration> Types
    {
        get;
        set;
    } = [];
}
