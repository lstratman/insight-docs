using System.Reflection;
using System.Text;
using InsightDocs.Abstractions;

namespace InsightDocs.DotNet.Model;

public class DotNetType : DotNetXmlDocSource, ILinkTarget
{
    private readonly static Dictionary<string, DotNetType> TypeCache = [];

    private static string GetCacheKey(Type type)
    {
        string key = type.Name;
        Type? currentDeclaringType = type.DeclaringType;

        while (currentDeclaringType != null)
        {
            key = (currentDeclaringType.Name.Contains('`') ? currentDeclaringType.Name[..currentDeclaringType.Name.IndexOf('`')] : currentDeclaringType.Name) + "." + key;
            currentDeclaringType = currentDeclaringType.DeclaringType;
        }

        if (!String.IsNullOrEmpty(type.Namespace))
        {
            key = type.Namespace + "." + key;
        }

        return key;
    }

    public static DotNetType Resolve(Type type)
    {
        if (type.IsByRef)
        {
            return Resolve(type.GetElementType()!);
        }

        string key = GetCacheKey(type);

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
        string key = GetCacheKey(type);

        if (!TypeCache.ContainsKey(key))
        {
            TypeCache[key] = this;
        }

        Assembly = assembly;
        Namespace = ns;
        Name = type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name;
        IsSealed = type.IsSealed;
        IsAbstract = type.IsAbstract;

        Type? currentDeclaringType = type.DeclaringType;

        while (currentDeclaringType != null)
        {
            Name = (currentDeclaringType.Name.Contains('`') ? currentDeclaringType.Name[..currentDeclaringType.Name.IndexOf('`')] : currentDeclaringType.Name) + "." + Name;
            currentDeclaringType = currentDeclaringType.DeclaringType;
        }

        FullName = String.IsNullOrEmpty(type.Namespace) ? Name : type.Namespace + "." + Name;

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

        Type? baseType = type.BaseType;

        if (type.IsInterface)
        {
            baseType = type.GetInterfaces().FirstOrDefault();
        }

        if (baseType != null)
        {
            BaseType = DotNetTypeReference.Resolve(baseType);
        }

        Type[] baseTypeInterfaces = baseType == null ? [] : baseType.GetInterfaces();
        Type[] implementedInterfaces = [.. type.GetInterfaces().Except(baseTypeInterfaces).Where(i => i != baseType)];

        if (implementedInterfaces != null && implementedInterfaces.Length > 0)
        {
            ImplementedInterfaces = [.. implementedInterfaces.Select(i => new DotNetTypeReference(i))];
        }

        MethodInfo[] methods = [.. type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(m => !m.Name.StartsWith('<') && !m.Name.StartsWith("op_") && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_") && !m.Name.Contains(".op_") && !m.Name.Contains(".get_") && !m.Name.Contains(".set_"))];

        if (methods != null && methods.Length > 0)
        {
            Methods = [];

            foreach (MethodInfo method in methods)
            {
                DotNetMethod? methodCollection = Methods.FirstOrDefault(m => m.Name == method.Name);

                if (methodCollection == null)
                {
                    methodCollection = new DotNetMethod(method.Name, DotNetTypeReference.Resolve(type));
                    Methods.Add(methodCollection);
                }

                DotNetMethodOverload overload = new(method, methodCollection);
                methodCollection.Overloads.Add(overload);

                if (!method.Name.Contains('.') && overload.DeclaringType != null && overload.DeclaringType.Type == this && overload.XmlDocKey != null)
                {
                    XmlDocUrlResolver.RegisterLookup(overload, "M:" + overload.XmlDocKey);
                }
            }
        }

        // TODO: explicitly implemented interface properties
        PropertyInfo[] properties = [.. type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(p => !p.Name.Contains('.'))];

        if (properties != null && properties.Length > 0)
        {
            Properties = [.. properties.Select(p => new DotNetProperty(p))];

            foreach (DotNetProperty property in Properties.Where(p => p.DeclaringType?.Type == this && p.XmlDocKey != null))
            {
                XmlDocUrlResolver.RegisterLookup(property, "P:" + property.XmlDocKey!);
            }
        }

        FieldInfo[] fields = [.. type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(f => !f.Name.StartsWith('<'))];

        if (fields != null && fields.Length > 0)
        {
            Fields = [.. fields.Select(f => new DotNetField(f))];

            foreach (DotNetField field in Fields.Where(f => f.DeclaringType?.Type == this && f.XmlDocKey != null))
            {
                XmlDocUrlResolver.RegisterLookup(field, "F:" + field.XmlDocKey!);
            }
        }

        if (type.IsInterface && BaseType?.Type != null)
        {
            if (BaseType.Type.Properties != null)
            {
                Properties ??= [];

                Properties.AddRange(BaseType.Type.Properties.Select(p =>
                {
                    DotNetProperty clonedProperty = new(p);
                    clonedProperty.DeclaringType ??= BaseType;

                    return clonedProperty;
                }));
            }

            if (BaseType.Type.Methods != null)
            {
                Methods ??= [];

                Methods.AddRange(BaseType.Type.Methods.Select(m =>
                {
                    DotNetMethod clonedMethod = new(m);
                    clonedMethod.DeclaringType ??= BaseType;

                    return clonedMethod;
                }));
            }

            if (BaseType.Type.Fields != null)
            {
                Fields ??= [];

                Fields.AddRange(BaseType.Type.Fields.Select(f =>
                {
                    DotNetField clonedField = new(f);
                    clonedField.DeclaringType ??= BaseType;

                    return clonedField;
                }));
            }
        }

        if (XmlDocKey != null)
        {
            XmlDocUrlResolver.RegisterLookup(this, "T:" + XmlDocKey);
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

    public string Title
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