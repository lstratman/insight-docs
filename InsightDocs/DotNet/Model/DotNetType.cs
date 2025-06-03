using System.Text;
using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetType : DotNetXmlDocSource, ILinkTarget
{
    public List<DotNetTypeParameter>? TypeParameters
    {
        get;
        set;
    }

    public DotNetNamespace? Namespace
    {
        get;
        set;
    }

    public required string TypeName
    {
        get;
        set;
    }

    public string Title
    {
        get
        {
            return DisplayName + " " + TypeName;
        }
    }

    public required string DisplayName
    {
        get;
        set;
    }

    public required string Name
    {
        get;
        set;
    }

    public required string FullName
    {
        get;
        set;
    }

    public bool IsSealed
    {
        get;
        set;
    }

    public bool IsAbstract
    {
        get;
        set;
    }

    public DotNetTypeReference? BaseType
    {
        get;
        set;
    }

    public List<DotNetTypeReference>? ImplementedInterfaces
    {
        get;
        set;
    }

    public required DotNetAssembly Assembly
    {
        get;
        set;
    }

    public List<DotNetMethod>? Methods
    {
        get;
        set;
    }

    public List<DotNetProperty>? Properties
    {
        get;
        set;
    }

    public List<DotNetField>? Fields
    {
        get;
        set;
    }

    public string XmlDocKey
    {
        get
        {
            StringBuilder key = new();

            if (Namespace != null)
            {
                key.Append(Namespace.FullName);
                key.Append('.');
            }

            key.Append(Name);

            if (TypeParameters != null && TypeParameters.Count > 0)
            {
                key.Append('{');
                key.Append(String.Join(',', TypeParameters.Select(p => p.Name)));
                key.Append('}');
            }

            return key.ToString();
        }
    }

    public string LinkText
    {
        get
        {
            return DisplayName;
        }
    }
}