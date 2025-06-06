using InsightDocs.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace InsightDocs.DotNet.Model;

public class DotNetMethod : ILinkTarget
{
    [SetsRequiredMembers]
    public DotNetMethod(DotNetMethod method)
    {
        Name = method.Name;
        DeclaringType = method.DeclaringType;
        Overloads = [.. Overloads.Select(o => new DotNetMethodOverload(o))];
    }

    public DotNetMethod()
    {
    }

    public required string Name
    {
        get;
        set;
    }

    public required DotNetTypeReference DeclaringType
    {
        get;
        set;
    }

    public bool IsConstructor
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
            return MemberDisplayName + (IsConstructor ? " Constructors" : " Method");
        }
    }

    public string MemberDisplayName
    {
        get
        {
            if (DeclaringType != null && DeclaringType.Type != null)
            {
                return IsConstructor ? DeclaringType.Type.DisplayName : DeclaringType.Type.DisplayName + "." + (Overloads.Count == 1 ? Overloads[0].MemberDisplayName : Name);
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