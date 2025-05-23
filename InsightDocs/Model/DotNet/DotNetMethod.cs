namespace InsightDocs.Model.DotNet;

public class DotNetMethod
{
    public DotNetMethod(DotNetMethod method)
    {
        Name = method.Name;
        DeclaringType = method.DeclaringType;
        Overloads = [.. Overloads.Select(o => new DotNetMethodOverload(o))];
    }

    public DotNetMethod(string name, DotNetTypeReference declaringType)
    {
        Name = name;

        if (Name.Contains('.'))
        {
            Name = Name[(Name.LastIndexOf('.') + 1)..];
        }

        DeclaringType = declaringType;
    }

    public string Name
    {
        get;
        set;
    }

    public DotNetTypeReference DeclaringType
    {
        get;
        set;
    }

    public List<DotNetMethodOverload> Overloads
    {
        get;
        set;
    } = [];
}