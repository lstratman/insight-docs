using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

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