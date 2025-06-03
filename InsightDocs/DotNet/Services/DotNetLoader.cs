using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Xml;

namespace InsightDocs.DotNet.Services;

public partial class DotNetLoader(IXmlDocUrlResolver xmlDocUrlResolver, IXmlDocProcessor xmlDocProcessor, ILoggerFactory loggerFactory) : IDotNetLoader
{
    protected readonly Dictionary<string, DotNetType> TypeCache = [];
    protected readonly Dictionary<string, DotNetTypeReference> TypeReferenceCache = [];
    protected readonly Dictionary<string, DotNetAssembly> AssemblyCache = [];
    protected readonly Dictionary<string, DotNetNamespace> NamespaceCache = [];

    [LoggerMessage(LogLevel.Information, "Loading {assemblyPath}")]
    public static partial void LogAssemblyLoad(ILogger logger, string assemblyPath);

    [LoggerMessage(LogLevel.Information, "Finished loading {assemblyPath}")]
    public static partial void LogFinishedAssemblyLoad(ILogger logger, string assemblyPath);

    [LoggerMessage(LogLevel.Debug, "Skipping type {type}")]
    public static partial void LogSkippingLoadingType(ILogger logger, string type);

    [LoggerMessage(LogLevel.Debug, "Loading type {type}")]
    public static partial void LogLoadingType(ILogger logger, string type);

    private static string GetTypeCacheKey(Type type)
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

    private static string GetTypeReferenceCacheKey(Type type)
    {
        string? key = type.FullName;

        if (key == null)
        {
            key = type.Name;

            Type? currentDeclaringType = type.DeclaringType;

            while (currentDeclaringType != null)
            {
                key = (currentDeclaringType.Name.Contains('`') ? currentDeclaringType.Name[..currentDeclaringType.Name.IndexOf('`')] : currentDeclaringType.Name) + "." + key;
                currentDeclaringType = currentDeclaringType.DeclaringType;
            }

            if (type.Namespace != null)
            {
                key = type.Namespace + "." + key;
            }

            if (type.GenericTypeArguments != null && type.GenericTypeArguments.Length > 0)
            {
                key += "{" + String.Join(",", type.GetGenericArguments().Select(GetTypeReferenceCacheKey)) + "}";
            }
        }

        return key;
    }

    protected virtual void CacheType(DotNetType type, Type actualType)
    {
        string key = GetTypeCacheKey(actualType);

        if (!TypeCache.ContainsKey(key))
        {
            TypeCache[key] = type;
        }
    }

    public virtual DotNetIndex LoadAssemblies(List<string> assemblyPaths, List<string> runtimeAssemblyPaths, Func<Type, bool>? typeFilter)
    {
        ILogger logger = loggerFactory.CreateLogger("InsightDocs.DotNet");
        PathAssemblyResolver pathAssemblyResolver = new(runtimeAssemblyPaths.Concat(assemblyPaths));
        Dictionary<string, DotNetNamespace> namespaces = [];

        foreach (string assemblyPath in assemblyPaths)
        {
            LogAssemblyLoad(logger, assemblyPath);

            MetadataLoadContext metadataLoadContext = new(pathAssemblyResolver);
            Assembly assembly = metadataLoadContext.LoadFromAssemblyPath(assemblyPath);
            
            LogFinishedAssemblyLoad(logger, assemblyPath);

            foreach (Type type in assembly.GetTypes())
            {
                if (type.Name.StartsWith('<') || type.Name.StartsWith("_Closure$") || type.Name.StartsWith("VB$StateMachine_"))
                {
                    LogSkippingLoadingType(logger, type.FullName!);
                    continue;
                }

                if (typeFilter != null && !typeFilter(type))
                {
                    LogSkippingLoadingType(logger, type.FullName!);
                    continue;
                }

                LogLoadingType(logger, type.FullName!);

                DotNetType typeMetadata = LoadType(type);

                if (typeMetadata.Namespace != null && !namespaces.ContainsKey(typeMetadata.Namespace.FullName))
                {
                    namespaces[typeMetadata.Namespace.FullName] = typeMetadata.Namespace;
                }
            }
        }

        return new DotNetIndex
        {
            Namespaces = [.. namespaces.Values]
        };
    }

    public virtual DotNetAssembly LoadAssembly(Assembly assembly)
    {
        string key = assembly.Location;

        if (!AssemblyCache.TryGetValue(key, out DotNetAssembly? assemblyMetadata))
        {
            AssemblyName assemblyName = assembly.GetName();
            string? assemblyDirectory = Path.GetDirectoryName(assembly.Location);

            assemblyMetadata = new DotNetAssembly
            {
                Name = assemblyName.Name,
                FullName = assemblyName.FullName
            };

            AssemblyCache[key] = assemblyMetadata;

            if (!String.IsNullOrEmpty(assemblyDirectory) && !String.IsNullOrEmpty(assemblyName.Name) && File.Exists(Path.Combine(assemblyDirectory, assemblyName.Name + ".xml")))
            {
                XmlDocument xmlDocDocument = new();
                // TODO: move >> replacement to custom
                string xmlDocText = File.ReadAllText(Path.Combine(assemblyDirectory, assemblyName.Name + ".xml")).Replace(">>", ">");

                xmlDocDocument.LoadXml(xmlDocText);

                XmlNodeList? memberNodes = xmlDocDocument.SelectNodes("/doc/members/member");

                if (memberNodes != null)
                {
                    assemblyMetadata.XmlDocEntries = [];

                    foreach (XmlElement memberNode in memberNodes)
                    {
                        XmlDocEntry xmlDocEntry = xmlDocProcessor.ProcessMemberNode(memberNode);
                        assemblyMetadata.XmlDocEntries[xmlDocEntry.Key] = xmlDocEntry;
                    }
                }
            }
        }

        return assemblyMetadata;
    }

    protected virtual DotNetNamespace LoadNamespace(string ns)
    {
        if (!NamespaceCache.TryGetValue(ns, out DotNetNamespace? namespaceMetadata))
        {
            namespaceMetadata = new DotNetNamespace(ns);
            NamespaceCache[ns] = namespaceMetadata;
        }

        return namespaceMetadata;
    }

    public virtual DotNetType LoadType(Type type)
    {
        if (type.IsByRef)
        {
            return LoadType(type.GetElementType()!);
        }

        string key = GetTypeCacheKey(type);

        if (!TypeCache.TryGetValue(key, out DotNetType? typeMetadata))
        {
            DotNetAssembly assembly = LoadAssembly(type.Assembly);
            DotNetNamespace? ns = String.IsNullOrEmpty(type.Namespace) ? null : LoadNamespace(type.Namespace);
            string name = type.Name.Contains('`') ? type.Name[..type.Name.IndexOf('`')] : type.Name;
            Type[] typeParameters = type.GetGenericArguments();
            Type? currentDeclaringType = type.DeclaringType;
            string typeName;

            if (type.IsInterface)
            {
                typeName = "Interface";
            }

            else if (type.IsEnum)
            {
                typeName = "Enum";
            }

            else if (type.IsValueType)
            {
                typeName = "Struct";
            }

            else
            {
                typeName = "Class";
            }

            while (currentDeclaringType != null)
            {
                name = (currentDeclaringType.Name.Contains('`') ? currentDeclaringType.Name[..currentDeclaringType.Name.IndexOf('`')] : currentDeclaringType.Name) + "." + name;
                currentDeclaringType = currentDeclaringType.DeclaringType;
            }

            typeMetadata = new DotNetType
            {
                Assembly = assembly,
                Namespace = ns,
                Name = name,
                FullName = String.IsNullOrEmpty(type.Namespace) ? name : type.Namespace + "." + name,
                IsSealed = type.IsSealed,
                IsAbstract = type.IsAbstract,
                TypeName = typeName,
                DisplayName = typeParameters != null && typeParameters.Length > 0 ? name + "<" + String.Join(", ", typeParameters.Select(a => a.Name)) + ">" : name
            };

            if (ns != null)
            {
                ns.Types.Add(typeMetadata);
            }

            CacheType(typeMetadata, type);

            if (typeParameters != null && typeParameters.Length > 0)
            {
                typeMetadata.TypeParameters = [.. typeParameters.Select(LoadTypeParameter)];
            }

            if (assembly.XmlDocEntries != null)
            {
                assembly.XmlDocEntries.TryGetValue("T:" + typeMetadata.XmlDocKey, out XmlDocEntry? xmlDocEntry);

                if (xmlDocEntry != null)
                {
                    if (xmlDocEntry.Summary != null)
                    {
                        typeMetadata.Description = new XmlDocHtml(xmlDocEntry.Summary);
                    }

                    if (xmlDocEntry.Remarks != null)
                    {
                        typeMetadata.Remarks = new XmlDocHtml(xmlDocEntry.Remarks);
                    }

                    if (typeMetadata.TypeParameters != null && xmlDocEntry.TypeParameters != null)
                    {
                        foreach (DotNetTypeParameter parameter in typeMetadata.TypeParameters)
                        {
                            if (xmlDocEntry.TypeParameters.TryGetValue(parameter.Name, out List<XmlDocCommentComponent>? components))
                            {
                                parameter.Description = new XmlDocHtml(components);
                            }
                        }
                    }

                    if (xmlDocEntry.SeeAlso != null)
                    {
                        typeMetadata.SeeAlso = [];

                        foreach (XmlDocSeeTagComponent seeAlso in xmlDocEntry.SeeAlso)
                        {
                            typeMetadata.SeeAlso.Add(new XmlDocHtml([seeAlso]));
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
                typeMetadata.BaseType = LoadTypeReference(baseType);
            }

            Type[] baseTypeInterfaces = baseType == null ? [] : baseType.GetInterfaces();
            Type[] implementedInterfaces = [.. type.GetInterfaces().Except(baseTypeInterfaces).Where(i => i != baseType)];

            if (implementedInterfaces != null && implementedInterfaces.Length > 0)
            {
                typeMetadata.ImplementedInterfaces = [.. implementedInterfaces.Select(LoadTypeReference)];
            }

            FieldInfo[] fields = [.. type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(f => !f.Name.StartsWith('<'))];

            if (fields != null && fields.Length > 0)
            {
                typeMetadata.Fields = [.. fields.Select(LoadField)];

                foreach (DotNetField field in typeMetadata.Fields.Where(f => f.DeclaringType?.Type == typeMetadata && f.XmlDocKey != null))
                {
                    xmlDocUrlResolver.RegisterLookup(field, "F:" + field.XmlDocKey!);
                }
            }

            MethodInfo[] methods = [.. type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(m => !m.Name.StartsWith('<') && !m.Name.StartsWith("op_") && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_") && !m.Name.Contains(".op_") && !m.Name.Contains(".get_") && !m.Name.Contains(".set_"))];

            if (methods != null && methods.Length > 0)
            {
                typeMetadata.Methods = [];

                foreach (MethodInfo method in methods)
                {
                    if (method.Name.Contains('.'))
                    {
                        // TODO
                        continue;
                    }

                    DotNetMethod? methodCollection = typeMetadata.Methods.FirstOrDefault(m => m.Name == method.Name);

                    if (methodCollection == null)
                    {
                        methodCollection = LoadMethod(method, type);
                        typeMetadata.Methods.Add(methodCollection);
                    }

                    DotNetMethodOverload overload = LoadMethodOverload(method, methodCollection);
                    methodCollection.Overloads.Add(overload);
                }

                foreach (DotNetMethod method in typeMetadata.Methods.Where(m => !m.Name.Contains('.')))
                {
                    foreach (DotNetMethodOverload overload in method.Overloads.Where(o => o.DeclaringType != null && o.DeclaringType.Type == typeMetadata && o.XmlDocKey != null))
                    {
                        xmlDocUrlResolver.RegisterLookup(overload, "M:" + overload.XmlDocKey);
                    }
                }
            }

            // TODO: explicitly implemented interface properties
            PropertyInfo[] properties = [.. type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(p => !p.Name.Contains('.'))];

            if (properties != null && properties.Length > 0)
            {
                typeMetadata.Properties = [.. properties.Select(LoadProperty)];

                foreach (DotNetProperty property in typeMetadata.Properties.Where(p => p.DeclaringType?.Type == typeMetadata && p.XmlDocKey != null))
                {
                    xmlDocUrlResolver.RegisterLookup(property, "P:" + property.XmlDocKey!);
                }
            }

            if (type.IsInterface && typeMetadata.BaseType?.Type != null)
            {
                if (typeMetadata.BaseType.Type.Properties != null)
                {
                    typeMetadata.Properties ??= [];

                    typeMetadata.Properties.AddRange(typeMetadata.BaseType.Type.Properties.Select(p =>
                    {
                        DotNetProperty clonedProperty = new(p);
                        clonedProperty.DeclaringType ??= typeMetadata.BaseType;

                        return clonedProperty;
                    }));
                }

                if (typeMetadata.BaseType.Type.Methods != null)
                {
                    typeMetadata.Methods ??= [];

                    typeMetadata.Methods.AddRange(typeMetadata.BaseType.Type.Methods.Select(m =>
                    {
                        DotNetMethod clonedMethod = new(m);
                        clonedMethod.DeclaringType ??= typeMetadata.BaseType;

                        return clonedMethod;
                    }));
                }

                if (typeMetadata.BaseType.Type.Fields != null)
                {
                    typeMetadata.Fields ??= [];

                    typeMetadata.Fields.AddRange(typeMetadata.BaseType.Type.Fields.Select(f =>
                    {
                        DotNetField clonedField = new(f);
                        clonedField.DeclaringType ??= typeMetadata.BaseType;

                        return clonedField;
                    }));
                }
            }

            if (typeMetadata.XmlDocKey != null)
            {
                xmlDocUrlResolver.RegisterLookup(typeMetadata, "T:" + typeMetadata.XmlDocKey);
            }

            CacheType(typeMetadata, type);
        }

        return typeMetadata;
    }

    protected virtual DotNetGenericArgument LoadGenericArgument(Type type)
    {
        DotNetGenericArgument genericArgument = new DotNetGenericArgument();

        if (type.IsGenericParameter)
        {
            genericArgument.TypeParameterName = type.Name;
        }

        else
        {
            genericArgument.Type = LoadTypeReference(type);
        }

        return genericArgument;
    }

    protected virtual DotNetTypeReference LoadTypeReference(Type type)
    {
        if (type.IsByRef)
        {
            return LoadTypeReference(type.GetElementType()!);
        }

        string key = GetTypeReferenceCacheKey(type);

        if (!TypeReferenceCache.TryGetValue(key, out DotNetTypeReference? typeReference))
        {
            typeReference = new DotNetTypeReference();

            if (type.IsArray)
            {
                typeReference.IsArray = true;
                type = type.GetElementType()!;
            }

            if (type.GenericTypeArguments != null && type.GenericTypeArguments.Length > 0)
            {
                typeReference.GenericArguments = [];

                foreach (Type genericArgument in type.GetGenericArguments())
                {
                    typeReference.GenericArguments.Add(LoadGenericArgument(genericArgument));
                }

                if (type.IsGenericType)
                {
                    type = type.GetGenericTypeDefinition();
                }
            }

            if (type.IsGenericParameter)
            {
                typeReference.IsGenericParameter = true;
                typeReference.GenericParameterName = type.Name;
            }

            else
            {
                typeReference.Type = LoadType(type);
            }

            TypeReferenceCache[key] = typeReference;
        }

        return typeReference;
    }

    protected virtual DotNetMethod LoadMethod(MethodInfo method, Type declaringType)
    {
        DotNetMethod methodMetadata = new DotNetMethod
        {
            Name = method.Name.Contains('.') ? method.Name[(method.Name.LastIndexOf('.') + 1)..] : method.Name,
            DeclaringType = LoadTypeReference(declaringType)
        };

        return methodMetadata;
    }

    protected virtual DotNetMethodParameter LoadMethodParameter(ParameterInfo parameter)
    {
        return new DotNetMethodParameter
        {
            IsOptional = parameter.IsOptional,
            IsOut = parameter.IsOut,
            IsByRef = parameter.ParameterType.IsByRef,
            Name = parameter.Name!,
            Type = LoadTypeReference(parameter.ParameterType)
        };
    }

    protected virtual DotNetProperty LoadProperty(PropertyInfo property)
    {
        DotNetProperty propertyMetadata = new DotNetProperty
        {
            Name = property.Name.Contains('.') ? property.Name[(property.Name.LastIndexOf('.') + 1)..] : property.Name,
            PropertyType = LoadTypeReference(property.PropertyType)
        };

        if (property.DeclaringType != null)
        {
            propertyMetadata.DeclaringType = LoadTypeReference(property.DeclaringType);

            if (propertyMetadata.DeclaringType.Type?.Assembly?.XmlDocEntries != null && propertyMetadata.XmlDocKey != null)
            {
                propertyMetadata.DeclaringType.Type.Assembly.XmlDocEntries.TryGetValue("P:" + propertyMetadata.XmlDocKey, out XmlDocEntry? xmlDocEntry);

                if (xmlDocEntry != null)
                {
                    if (xmlDocEntry.Summary != null)
                    {
                        propertyMetadata.Description = new XmlDocHtml(xmlDocEntry.Summary);
                    }

                    if (xmlDocEntry.Remarks != null)
                    {
                        propertyMetadata.Remarks = new XmlDocHtml(xmlDocEntry.Remarks);
                    }

                    if (xmlDocEntry.Value != null)
                    {
                        propertyMetadata.ValueDescription = new XmlDocHtml(xmlDocEntry.Value);
                    }

                    if (xmlDocEntry.SeeAlso != null)
                    {
                        propertyMetadata.SeeAlso = [];

                        foreach (XmlDocSeeTagComponent seeAlso in xmlDocEntry.SeeAlso)
                        {
                            propertyMetadata.SeeAlso.Add(new XmlDocHtml([seeAlso]));
                        }
                    }
                }
            }
        }

        if (property.GetMethod != null)
        {
            propertyMetadata.GetMethod = LoadMethodOverload(property.GetMethod);
        }

        if (property.SetMethod != null)
        {
            propertyMetadata.SetMethod = LoadMethodOverload(property.SetMethod);
        }

        ParameterInfo[] indexParameters = property.GetIndexParameters();

        if (indexParameters != null && indexParameters.Length > 0)
        {
            propertyMetadata.IndexParameters = [.. indexParameters.Select(LoadMethodParameter)];
        }

        return propertyMetadata;
    }

    protected virtual DotNetMethodOverload LoadMethodOverload(MethodInfo method, DotNetMethod? methodCollection = null)
    {
        DotNetMethodOverload overloadMetadata = new DotNetMethodOverload
        {
            Name = method.Name.Contains('.') ? method.Name[(method.Name.LastIndexOf('.') + 1)..] : method.Name,
            IsStatic = method.IsStatic,
            IsInternal = method.IsAssembly,
            IsAbstract = method.IsAbstract,
            MethodCollection = methodCollection,
            ReturnType = LoadTypeReference(method.ReturnType)
        };

        if (method.IsPublic)
        {
            overloadMetadata.AccessType = DotNetMemberInfoAccessType.Public;
        }

        else if (method.IsPrivate)
        {
            overloadMetadata.AccessType = DotNetMemberInfoAccessType.Private;
        }

        else if (method.IsFamily)
        {
            overloadMetadata.AccessType = DotNetMemberInfoAccessType.Protected;
        }

        Type[] genericArguments = method.GetGenericArguments();

        if (genericArguments != null && genericArguments.Length > 0)
        {
            overloadMetadata.GenericArguments = [.. genericArguments.Select(LoadTypeParameter)];
        }

        ParameterInfo[] parameters = method.GetParameters();

        if (parameters != null && parameters.Length > 0)
        {
            overloadMetadata.Parameters = [.. parameters.Select(LoadMethodParameter)];
        }

        if (method.DeclaringType != null)
        {
            overloadMetadata.DeclaringType = LoadTypeReference(method.DeclaringType);

            if (overloadMetadata.DeclaringType.Type?.Assembly?.XmlDocEntries != null && overloadMetadata.XmlDocKey != null)
            {
                overloadMetadata.DeclaringType.Type.Assembly.XmlDocEntries.TryGetValue("M:" + overloadMetadata.XmlDocKey, out XmlDocEntry? xmlDocEntry);

                if (xmlDocEntry != null)
                {
                    if (xmlDocEntry.Summary != null)
                    {
                        overloadMetadata.Description = new XmlDocHtml(xmlDocEntry.Summary);
                    }

                    if (xmlDocEntry.Remarks != null)
                    {
                        overloadMetadata.Remarks = new XmlDocHtml(xmlDocEntry.Remarks);
                    }

                    if (xmlDocEntry.Returns != null)
                    {
                        overloadMetadata.ReturnsDescription = new XmlDocHtml(xmlDocEntry.Returns);
                    }

                    if (overloadMetadata.Parameters != null && xmlDocEntry.Parameters != null)
                    {
                        foreach (DotNetMethodParameter parameter in overloadMetadata.Parameters)
                        {
                            if (xmlDocEntry.Parameters.TryGetValue(parameter.Name, out List<XmlDocCommentComponent>? components))
                            {
                                parameter.Description = new XmlDocHtml(components);
                            }
                        }
                    }

                    if (xmlDocEntry.Exceptions != null)
                    {
                        overloadMetadata.Exceptions = [];

                        foreach (XmlDocException exception in xmlDocEntry.Exceptions)
                        {
                            overloadMetadata.Exceptions.Add(new(new XmlDocHtml([exception.Exception]), exception.Text == null || exception.Text.Count == 0 ? null : new XmlDocHtml(exception.Text)));
                        }
                    }

                    if (xmlDocEntry.SeeAlso != null)
                    {
                        overloadMetadata.SeeAlso = [];

                        foreach (XmlDocSeeTagComponent seeAlso in xmlDocEntry.SeeAlso)
                        {
                            overloadMetadata.SeeAlso.Add(new XmlDocHtml([seeAlso]));
                        }
                    }
                }
            }
        }

        return overloadMetadata;
    }

    protected virtual DotNetTypeParameter LoadTypeParameter(Type type)
    {
        return new DotNetTypeParameter
        {
            Name = type.Name
        };
    }

    protected virtual DotNetField LoadField(FieldInfo field)
    {
        DotNetField fieldMetadata = new DotNetField
        {
            Name = field.Name,
            FieldType = LoadTypeReference(field.FieldType),
            IsStatic = field.IsStatic,
            IsInternal = field.IsAssembly,
            IsAbstract = false
        };

        if (field.IsPublic)
        {
            fieldMetadata.AccessType = DotNetMemberInfoAccessType.Public;
        }

        else if (field.IsPrivate)
        {
            fieldMetadata.AccessType = DotNetMemberInfoAccessType.Private;
        }

        else if (field.IsFamily)
        {
            fieldMetadata.AccessType = DotNetMemberInfoAccessType.Protected;
        }

        if (field.DeclaringType != null)
        {
            fieldMetadata.DeclaringType = LoadTypeReference(field.DeclaringType);

            if (fieldMetadata.DeclaringType.Type?.Assembly?.XmlDocEntries != null && fieldMetadata.XmlDocKey != null)
            {
                fieldMetadata.DeclaringType.Type.Assembly.XmlDocEntries.TryGetValue("F:" + fieldMetadata.XmlDocKey, out XmlDocEntry? xmlDocEntry);

                if (xmlDocEntry != null)
                {
                    if (xmlDocEntry.Summary != null)
                    {
                        fieldMetadata.Description = new XmlDocHtml(xmlDocEntry.Summary);
                    }

                    if (xmlDocEntry.Remarks != null)
                    {
                        fieldMetadata.Remarks = new XmlDocHtml(xmlDocEntry.Remarks);
                    }

                    if (xmlDocEntry.Returns != null)
                    {
                        fieldMetadata.ReturnsDescription = new XmlDocHtml(xmlDocEntry.Returns);
                    }

                    if (xmlDocEntry.SeeAlso != null)
                    {
                        fieldMetadata.SeeAlso = [];

                        foreach (XmlDocSeeTagComponent seeAlso in xmlDocEntry.SeeAlso)
                        {
                            fieldMetadata.SeeAlso.Add(new XmlDocHtml([seeAlso]));
                        }
                    }
                }
            }
        }

        return fieldMetadata;
    }
}
