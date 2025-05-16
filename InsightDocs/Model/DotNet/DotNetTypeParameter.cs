namespace InsightDocs.Model.DotNet;

public class DotNetTypeParameter(Type type)
{
    public string Name
    {
        get;
        set;
    } = type.Name;

    public string? Description
    {
        get;
        set;
    }
}