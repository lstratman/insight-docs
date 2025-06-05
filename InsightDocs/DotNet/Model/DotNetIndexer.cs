using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetIndexer : ILinkTarget
{
    public DotNetTypeReference? DeclaringType
    {
        get;
        set;
    }

    public List<DotNetProperty> Overloads
    {
        get;
        set;
    } = [];

    public string Title
    {
        get
        {
            return DeclaringType != null ? DeclaringType.DisplayName + " Indexer" : "Indexer";
        }
    }

    public string LinkText
    {
        get
        {
            return "Indexer";
        }
    }
}
