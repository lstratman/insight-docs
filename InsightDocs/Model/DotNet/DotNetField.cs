using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetField : DotNetMemberInfo
{
    public DotNetField(FieldInfo field)
    {
        Name = field.Name;
        FieldType = DotNetTypeReference.Resolve(field.FieldType);
        IsStatic = field.IsStatic;
        IsInternal = field.IsAssembly;
        IsAbstract = false;

        if (field.IsPublic)
        {
            AccessType = DotNetMemberInfoAccessType.Public;
        }

        else if (field.IsPrivate)
        {
            AccessType = DotNetMemberInfoAccessType.Private;
        }

        else if (field.IsFamily)
        {
            AccessType = DotNetMemberInfoAccessType.Protected;
        }

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