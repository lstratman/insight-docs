using System.Text.Json.Serialization;
using InsightDocs.Abstractions;

namespace InsightDocs.Site.Model;

public class SiteToc : ILinkTarget
{
    public SiteToc(TocItem root)
    {
        foreach (TocItem rootChild in root.Children)
        {
            RootItems.Add(PopulateFromItem(rootChild));
        }
    }

    private int PopulateFromItem(TocItem item, int? parentIndex = null)
    {
        SiteTableOfContentsItem siteTocItem = new SiteTableOfContentsItem(item, parentIndex);
        Items.Add(siteTocItem);

        int itemIndex = Items.Count - 1;

        if (!String.IsNullOrEmpty(item.Url))
        {
            UrlLookups[item.Url] = itemIndex;
        }

        if (item.Children != null)
        {
            foreach (TocItem child in item.Children)
            {
                siteTocItem.ChildIndices ??= [];
                siteTocItem.ChildIndices.Add(PopulateFromItem(child, itemIndex));
            }
        }

        if (item.AdditionalData != null)
        {
            siteTocItem.AdditionalData = item.AdditionalData;
        }

        return itemIndex;
    }

    public List<int> RootItems
    {
        get;
        set;
    } = [];

    public List<SiteTableOfContentsItem> Items
    {
        get;
        set;
    } = [];

    public Dictionary<string, int> UrlLookups
    {
        get;
        set;
    } = [];

    public string LinkText
    {
        get
        {
            return "Table of contents";
        }
    }
}

public class SiteTableOfContentsItem(TocItem tocItem, int? parentIndex)
{
    [JsonPropertyName("t")]
    public string Title
    {
        get;
        set;
    } = tocItem.Title;

    [JsonPropertyName("u")]
    public string? Url
    {
        get;
        set;
    } = tocItem.Url;

    [JsonPropertyName("p")]
    public int? ParentIndex
    {
        get;
        set;
    } = parentIndex;

    [JsonPropertyName("c")]
    public List<int>? ChildIndices
    {
        get;
        set;
    }

    [JsonPropertyName("d")]
    public Dictionary<string, string>? AdditionalData
    {
        get;
        set;
    }
}