using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetNamespace : ILinkTarget
{
    public DotNetNamespace(string ns, DotNetIndex index)
    {
        Name = ns.Contains('.', StringComparison.CurrentCulture) ? ns[(ns.LastIndexOf('.') + 1)..] : ns;
        FullName = ns;
        Index = index;
    }

    public DotNetIndex Index
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public string FullName
    {
        get;
        set;
    }

    public bool IsExternal
    {
        get;
        set;
    }

    public List<DotNetType> Types
    {
        get;
        private set;
    } = [];

    public string LinkText
    {
        get
        {
            return FullName;
        }
    }
}