using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetProperty
{
    public DotNetProperty(PropertyInfo property)
    {
        Name = property.Name;
        PropertyType = DotNetTypeReference.Resolve(property.PropertyType);

        if (property.DeclaringType != null)
        {
            DeclaringType = DotNetTypeReference.Resolve(property.DeclaringType);
        }
    }

    public string Name
    {
        get;
        set;
    }

    public string? Description
    {
        get;
        set;
    }

    public DotNetTypeReference PropertyType
    {
        get;
        set;
    }

    public DotNetTypeReference? DeclaringType
    {
        get;
        set;
    }
}