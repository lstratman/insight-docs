using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetNamespace : ILinkTarget
{
    private static readonly Dictionary<string, DotNetNamespace> NamespaceCache = [];

    public static DotNetNamespace Resolve(string ns)
    {
        if (!NamespaceCache.TryGetValue(ns, out DotNetNamespace? namespaceMetadata))
        {
            namespaceMetadata = new DotNetNamespace(ns);
            NamespaceCache[ns] = namespaceMetadata;
        }

        return namespaceMetadata;
    }

    protected DotNetNamespace(string ns)
    {
        Name = ns.Contains('.', StringComparison.CurrentCulture) ? ns[(ns.LastIndexOf('.') + 1)..] : ns;
        FullName = ns;
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