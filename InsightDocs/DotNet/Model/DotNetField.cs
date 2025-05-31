using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetField : DotNetMemberInfo
{
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
    }

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

            if (DeclaringType.Type?.Assembly?.XmlDocEntries != null && XmlDocKey != null)
            {
                DeclaringType.Type.Assembly.XmlDocEntries.TryGetValue("F:" + XmlDocKey, out XmlDocEntry? xmlDocEntry);

                if (xmlDocEntry != null)
                {
                    if (xmlDocEntry.Summary != null)
                    {
                        Description = new XmlDocHtml(xmlDocEntry.Summary);
                    }

                    if (xmlDocEntry.Remarks != null)
                    {
                        Remarks = new XmlDocHtml(xmlDocEntry.Remarks);
                    }

                    if (xmlDocEntry.Returns != null)
                    {
                        ReturnsDescription = new XmlDocHtml(xmlDocEntry.Returns);
                    }

                    if (xmlDocEntry.SeeAlso != null)
                    {
                        SeeAlso = [];

                        foreach (XmlDocSeeTagComponent seeAlso in xmlDocEntry.SeeAlso)
                        {
                            SeeAlso.Add(new XmlDocHtml([seeAlso]));
                        }
                    }
                }
            }
        }
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

    public string Name
    {
        get;
        set;
    }

    public XmlDocHtml? ReturnsDescription
    {
        get;
        set;
    }

    public DotNetTypeReference FieldType
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
}