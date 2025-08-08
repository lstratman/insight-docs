using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetNamespace(string ns, DotNetIndex index) : ILinkTarget
{
    public DotNetIndex Index
    {
        get;
        set;
    } = index;

    public string Name
    {
        get;
        set;
    } = ns.Contains('.', StringComparison.CurrentCulture) ? ns[(ns.LastIndexOf('.') + 1)..] : ns;

    public string FullName
    {
        get;
        set;
    } = ns;

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

    public string Title
    {
        get
        {
            return FullName + " Namespace";
        }
    }
}