namespace InsightDocs.Model.DotNet;

public abstract class DotNetMemberInfo
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
}

public enum DotNetMemberInfoAccessType
{
    Public,
    Private,
    Protected
}