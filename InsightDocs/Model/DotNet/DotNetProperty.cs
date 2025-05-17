using System.Reflection;

namespace InsightDocs.Model.DotNet;

public class DotNetProperty : DotNetXmlDocSource
{
    public DotNetProperty(PropertyInfo property)
    {
        Name = property.Name;
        PropertyType = DotNetTypeReference.Resolve(property.PropertyType);

        if (property.DeclaringType != null)
        {
            DeclaringType = DotNetTypeReference.Resolve(property.DeclaringType);

            if (DeclaringType.Type?.Assembly?.XmlDocEntries != null && XmlDocKey != null)
            {
                DeclaringType.Type.Assembly.XmlDocEntries.TryGetValue("P:" + XmlDocKey, out XmlDocEntry? xmlDocEntry);

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

                    if (xmlDocEntry.Value != null)
                    {
                        ValueDescription = new XmlDocHtml(xmlDocEntry.Value);
                    }
                }
            }
        }

        if (property.GetMethod != null)
        {
            GetMethod = new DotNetMethod(property.GetMethod);
        }

        if (property.SetMethod != null)
        {
            SetMethod = new DotNetMethod(property.SetMethod);
        }
    }

    public DotNetMethod? GetMethod
    {
        get;
        set;
    }

    public DotNetMethod? SetMethod
    {
        get;
        set;
    }

    public string Name
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

    public XmlDocHtml? ValueDescription
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