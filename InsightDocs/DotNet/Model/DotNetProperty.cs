using System.Reflection;
using System.Text;
using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetProperty : DotNetXmlDocSource, ILinkTarget
{
    public DotNetProperty(DotNetProperty property)
    {
        Name = property.Name;
        PropertyType = property.PropertyType;
        DeclaringType = property.DeclaringType;
        GetMethod = property.GetMethod == null ? null : new DotNetMethodOverload(property.GetMethod);
        SetMethod = property.SetMethod == null ? null : new DotNetMethodOverload(property.SetMethod);
        Description = property.Description;
        Remarks = property.Remarks;
        DeclaringType = property.DeclaringType;
    }

    public DotNetProperty(PropertyInfo property, IServiceProvider serviceProvider)
    {
        Name = property.Name;

        if (Name.Contains('.'))
        {
            Name = Name[(Name.LastIndexOf('.') + 1)..];
        }
        
        PropertyType = DotNetTypeReference.Resolve(property.PropertyType, serviceProvider);

        if (property.DeclaringType != null)
        {
            DeclaringType = DotNetTypeReference.Resolve(property.DeclaringType, serviceProvider);

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

        if (property.GetMethod != null)
        {
            GetMethod = new DotNetMethodOverload(property.GetMethod, serviceProvider);
        }

        if (property.SetMethod != null)
        {
            SetMethod = new DotNetMethodOverload(property.SetMethod, serviceProvider);
        }

        ParameterInfo[] indexParameters = property.GetIndexParameters();

        if (indexParameters != null && indexParameters.Length > 0)
        {
            IndexParameters = [.. indexParameters.Select(p => new DotNetMethodParameter(p, serviceProvider))];
        }
    }

    public DotNetMethodOverload? GetMethod
    {
        get;
        set;
    }

    public DotNetMethodOverload? SetMethod
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

            if (typeDocKey == null)
            {
                return null;
            }

            StringBuilder key = new(typeDocKey + "." + Name);

            if (IndexParameters != null && IndexParameters.Count > 0)
            {
                key.Append('[');
                key.Append(String.Join(',', IndexParameters.Select(p => p.Type.XmlDocKey)));
                key.Append(']');
            }

            return key.ToString();
        }
    }

    public string MemberDisplayName
    {
        get
        {
            if (DeclaringType != null)
            {
                return DeclaringType.DisplayName + "." + Name;
            }

            else
            {
                return Name;
            }
        }
    }

    public string Title
    {
        get
        {
            return MemberDisplayName + " Property";
        }
    }

    public string LinkText
    {
        get
        {
            return Name;
        }
    }

    public List<DotNetMethodParameter>? IndexParameters
    {
        get;
        set;
    }
}