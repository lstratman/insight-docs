using InsightDocs.Abstractions;

namespace InsightDocs.Model.DotNet;

public class DotNetIndex : ILinkTarget
{
    public List<DotNetNamespace> Namespaces
    {
        get;
        set;
    } = [];

    public string LinkText
    {
        get
        {
            return ".NET API Index";
        }
    }
}