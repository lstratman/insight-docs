using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetIndex : ILinkTarget
{
    public List<DotNetNamespace> Namespaces
    {
        get;
        set;
    } = [];

    public string Title
    {
        get
        {
            return ".NET API Reference";
        }
    }

    public string LinkText
    {
        get
        {
            return Title;
        }
    }
}