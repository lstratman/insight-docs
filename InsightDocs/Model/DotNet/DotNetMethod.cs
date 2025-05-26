using InsightDocs.Abstractions;

namespace InsightDocs.Model.DotNet;

public class DotNetMethod : ILinkTarget
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

    public string Title
    {
        get
        {
            return MemberDisplayName + " Method";
        }
    }

    public string MemberDisplayName
    {
        get
        {
            if (DeclaringType != null)
            {
                return DeclaringType.DisplayName + "." + (Overloads.Count == 1 ? Overloads[0].MemberDisplayName : Name);
            }

            else
            {
                return Overloads.Count == 1 ? Overloads[0].MemberDisplayName : Name;
            }
        }
    }

    public string LinkText
    {
        get
        {
            return MemberDisplayName;
        }
    }
}