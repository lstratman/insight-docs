using InsightDocs.Abstractions;

namespace InsightDocs.Model.DotNet;

public abstract class DotNetMemberInfo : DotNetXmlDocSource, ILinkTarget
{
    public DotNetMemberInfoAccessType AccessType
    {
        get;
        set;
    }

    public bool IsStatic
    {
        get;
        set;
    }

    public bool IsInternal
    {
        get;
        set;
    }

    public bool IsAbstract
    {
        get;
        set;
    }

    public abstract string LinkText
    {
        get;
    }

    public abstract string MemberDisplayName
    {
        get;
    }

    public DotNetTypeReference? DeclaringType
    {
        get;
        set;
    }
}

public enum DotNetMemberInfoAccessType
{
    Public,
    Private,
    Protected
}