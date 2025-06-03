using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace InsightDocs.DotNet.Model;

public class DotNetField : DotNetMemberInfo
{
    [SetsRequiredMembers]
    public DotNetField(DotNetField field)
    {
        Name = field.Name;
        IsStatic = field.IsStatic;
        IsInternal = field.IsInternal;
        IsAbstract = field.IsAbstract;
        AccessType = field.AccessType;
        FieldType = field.FieldType;
        DeclaringType = field.DeclaringType;
        Description = field.Description;
        Remarks = field.Remarks;
        ReturnsDescription = field.ReturnsDescription;
        DeclaringType = field.DeclaringType;
        IsReadOnly = field.IsReadOnly;
    }

    public DotNetField()
    {
    }

    public bool IsReadOnly
    {
        get;
        set;
    }

    public override string MemberDisplayName
    {
        get
        {
            return Name;
        }
    }

    public string Title
    {
        get
        {
            if (DeclaringType != null)
            {
                return DeclaringType.DisplayName + "." + Name + " Field";
            }

            else
            {
                return Name + " Field";
            }
        }
    }

    public override string LinkText
    {
        get
        {
            return MemberDisplayName;
        }
    }

    public required string Name
    {
        get;
        set;
    }

    public XmlDocHtml? ReturnsDescription
    {
        get;
        set;
    }

    public required DotNetTypeReference FieldType
    {
        get;
        set;
    }

    public string? XmlDocKey
    {
        get
        {
            string? typeDocKey = DeclaringType?.XmlDocKey;
            return typeDocKey == null ? null : typeDocKey + "." + Name;
        }
    }

    public override string CSharpCode
    {
        get
        {
            StringBuilder code = new StringBuilder(base.CSharpCode);

            if (IsReadOnly)
            {
                code.Append("readonly ");
            }

            code.Append(FieldType.CSharpCode);
            code.Append(' ');
            code.Append(Name);
            code.Append(';');

            return code.ToString();
        }
    }
}