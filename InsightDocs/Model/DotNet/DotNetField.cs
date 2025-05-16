using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetField
{
    public DotNetField(FieldInfo field)
    {
        Name = field.Name;
        FieldType = DotNetTypeReference.Resolve(field.FieldType);

        if (field.DeclaringType != null)
        {
            DeclaringType = DotNetTypeReference.Resolve(field.DeclaringType);
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

    public DotNetTypeReference FieldType
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