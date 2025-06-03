namespace InsightDocs.DotNet.Model;

public class DotNetAssembly
{
    public string? Name
    {
        get;
        set;
    }

    public required string FullName
    {
        get;
        set;
    }

    public Dictionary<string, XmlDocEntry>? XmlDocEntries
    {
        get;
        set;
    }
}