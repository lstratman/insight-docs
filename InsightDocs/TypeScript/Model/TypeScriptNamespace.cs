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

    public string Title
    {
        get
        {
            return Name + " Namespace";
        }
    }

    public List<TypeScriptTypeDeclaration> Types
    {
        get;
        set;
    } = [];
}
