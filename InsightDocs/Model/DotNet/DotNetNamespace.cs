namespace InsightDocs.Model.DotNet;

public class DotNetNamespace(string ns)
{
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

    public List<DotNetType> Types
    {
        get;
        private set;
    } = [];
}