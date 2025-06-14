using InsightDocs.Abstractions;
using System.Text;

namespace InsightDocs.DotNet.Model;

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

    public virtual string VBCode
    {
        get
        {
            StringBuilder code = new StringBuilder();

            if (AccessType == DotNetMemberInfoAccessType.Public)
            {
                code.Append("Public ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Protected)
            {
                code.Append("Protected ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Private)
            {
                code.Append("Private ");
            }

            if (IsInternal)
            {
                code.Append("Friend ");
            }

            if (IsAbstract)
            {
                code.Append("MustInherit ");
            }

            if (IsStatic)
            {
                code.Append("Shared ");
            }

            return code.ToString();
        }
    }

    public virtual string CSharpCode
    {
        get
        {
            StringBuilder code = new StringBuilder();

            if (AccessType == DotNetMemberInfoAccessType.Public)
            {
                code.Append("public ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Protected)
            {
                code.Append("protected ");
            }

            else if (AccessType == DotNetMemberInfoAccessType.Private)
            {
                code.Append("private ");
            }

            if (IsInternal)
            {
                code.Append("internal ");
            }

            if (IsAbstract)
            {
                code.Append("abstract ");
            }

            if (IsStatic)
            {
                code.Append("static ");
            }

            return code.ToString();
        }
    }
}

public enum DotNetMemberInfoAccessType
{
    Public,
    Private,
    Protected
}