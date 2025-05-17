using System.Reflection;
using System.Text;

namespace InsightDocs.Model.DotNet;

public class DotNetType : DotNetXmlDocSource
{
    private readonly static Dictionary<string, DotNetType> TypeCache = [];

    public static DotNetType Resolve(Type type)
    {
        string key = type.Name;

        if (!String.IsNullOrEmpty(type.Namespace))
        {
            key = type.Namespace + "." + type.Name;
        }

        if (!TypeCache.TryGetValue(key, out DotNetType? typeMetadata))
        {
            DotNetAssembly assembly = DotNetAssembly.Resolve(type.Assembly);
            DotNetNamespace? ns = String.IsNullOrEmpty(type.Namespace) ? null : DotNetNamespace.Resolve(type.Namespace);
            typeMetadata = new DotNetType(type, assembly, ns);
        }

        return typeMetadata;
    }

    protected DotNetType(Type type, DotNetAssembly assembly, DotNetNamespace? ns)
    {
        string key = type.Name;

        if (!String.IsNullOrEmpty(type.Namespace))
        {
            key = type.Namespace + "." + type.Name;
        }

        if (!TypeCache.ContainsKey(key))
        {
            TypeCache[key] = this;
        }

        Assembly = assembly;
        Namespace = ns;
        Name = type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name;
        FullName = String.IsNullOrEmpty(type.Namespace) ? Name : type.Namespace + "." + Name;
        IsSealed = type.IsSealed;
        IsAbstract = type.IsAbstract;

        Type[] typeParameters = type.GetGenericArguments();

        if (typeParameters != null && typeParameters.Length > 0)
        {
            DisplayName = Name + "<" + String.Join(", ", typeParameters.Select(a => a.Name)) + ">";
            TypeParameters = [.. typeParameters.Select(a => new DotNetTypeParameter(a))];
        }

        else
        {
            DisplayName = Name;
        }

        if (type.IsInterface)
        {
            TypeName = "Interface";
        }

        else if (type.IsEnum)
        {
            TypeName = "Enum";
        }

        else if (type.IsValueType)
        {
            TypeName = "Struct";
        }

        else
        {
            TypeName = "Class";
        }

        if (Assembly.XmlDocEntries != null)
        {
            Assembly.XmlDocEntries.TryGetValue("T:" + XmlDocKey, out XmlDocEntry? xmlDocEntry);

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

                if (TypeParameters != null && xmlDocEntry.TypeParameters != null)
                {
                    foreach (DotNetTypeParameter parameter in TypeParameters)
                    {
                        if (xmlDocEntry.TypeParameters.TryGetValue(parameter.Name, out List<XmlDocCommentComponent>? components))
                        {
                            parameter.Description = new XmlDocHtml(components);
                        }
                    }
                }
            }
        }

        MethodInfo[] methods = [.. type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => !m.Name.StartsWith('<') && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))];

        if (methods != null && methods.Length > 0)
        {
            Methods = [.. methods.Select(m => new DotNetMethod(m))];
        }

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (properties != null && properties.Length > 0)
        {
            Properties = [.. properties.Select(p => new DotNetProperty(p))];
        }

        FieldInfo[] fields = [.. type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(f => !f.Name.StartsWith('<'))];

        if (fields != null && fields.Length > 0)
        {
            Fields = [.. fields.Select(f => new DotNetField(f))];
        }
    }

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

    public string TypeName
    {
        get;
        set;
    }

    public string DisplayTitle
    {
        get
        {
            return DisplayName + " " + TypeName;
        }
    }

    public string DisplayName
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public string FullName
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

    public DotNetAssembly Assembly
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

            return key.ToString();
        }
    }
}