using System.Text;
using System.Text.RegularExpressions;
using InsightDocs.Abstractions;
using InsightDocs.DotNet;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;
using InsightDocs.Markdown.Model;
using InsightDocs.Site.Model;
using InsightDocs.TypeScript;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Services;

public class KebabCaseUrlProvider(KebabCaseUrlProviderOptions options, IUrlPrefixProvider urlPrefixProvider, IMicrosoftDocsUrlResolver microsoftDocsUrlResolver, DotNetOptions dotNetOptions, TypeScriptOptions typeScriptOptions) 
    : IUrlProvider<DotNetType>,
      IUrlProvider<DotNetIndex>,
      IUrlProvider<DotNetNamespace>,
      IUrlProvider<DotNetMethod>,
      IUrlProvider<DotNetMethodOverload>,
      IUrlProvider<DotNetProperty>,
      IUrlProvider<DotNetField>,
      IUrlProvider<SiteToc>,
      IUrlProvider<SiteIndex>,
      IUrlProvider<IAsset>,
      IUrlProvider<DotNetIndexer>,
      IUrlProvider<MarkdownFile>,
      IUrlProvider<MarkdownImage>,
      IUrlProvider<TypeScriptModule>,
      IUrlProvider<TypeScriptTypeDeclaration>,
      IUrlProvider<TypeScriptInterface>,
      IUrlProvider<TypeScriptMethod>,
      IUrlProvider<TypeScriptProperty>,
      IUrlProvider<TypeScriptMethodSignature>,
      IUrlProvider<TypeScriptNamespace>,
      IUrlProvider<IntrinsicType>
{
    protected KebabCaseUrlProviderOptions Options
    {
        get;
        set;
    } = options;

    protected static Regex NonAlphanumericCharacters = new Regex(@"[^a-zA-Z0-9\-]");

    public string GetUrl(DotNetType item)
    {
        if (item.IsExternal)
        {
            return dotNetOptions.ResolveMicrosoftDocsUrls && microsoftDocsUrlResolver.IsMicrosoftType(item) ? microsoftDocsUrlResolver.GetUrl(item) : "";
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.Namespace != null)
        {
            url.Append(item.Namespace.FullName);
            url.Append('.');
        }

        url.Append(item.Name);

        if (item.TypeParameters != null && item.TypeParameters.Count > 0)
        {
            url.Append('-');
            url.Append(item.TypeParameters.Count);
        }

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(DotNetIndex item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append("index");

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(DotNetNamespace item)
    {
        if (item.IsExternal)
        {
            return dotNetOptions.ResolveMicrosoftDocsUrls && microsoftDocsUrlResolver.IsMicrosoftNamespace(item.FullName) ? microsoftDocsUrlResolver.GetUrl(item) : "";
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append(item.FullName);

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(DotNetMethod item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.DeclaringType != null)
        {
            if (item.DeclaringType.Namespace != null)
            {
                url.Append(item.DeclaringType.Namespace.FullName);
                url.Append('.');
            }

            url.Append(item.DeclaringType.Name);

            if (item.DeclaringType.Type != null && item.DeclaringType.Type.TypeParameters != null && item.DeclaringType.Type.TypeParameters.Count > 0)
            {
                url.Append('-');
                url.Append(item.DeclaringType.Type.TypeParameters.Count);
            }

            url.Append('.');
        }
        
        url.Append(item.IsConstructor ? "ctor" : item.Name);

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(DotNetProperty item)
    {
        if (item.DeclaringType != null && item.DeclaringType.Type != null && item.DeclaringType.Type.IsExternal)
        {
            return dotNetOptions.ResolveMicrosoftDocsUrls && microsoftDocsUrlResolver.IsMicrosoftType(item.DeclaringType.Type) ? microsoftDocsUrlResolver.GetUrl(item) : "";
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.DeclaringType != null)
        {
            if (item.DeclaringType.Namespace != null)
            {
                url.Append(item.DeclaringType.Namespace.FullName);
                url.Append('.');
            }

            url.Append(item.DeclaringType.Name);

            if (item.DeclaringType.Type != null && item.DeclaringType.Type.TypeParameters != null && item.DeclaringType.Type.TypeParameters.Count > 0)
            {
                url.Append('-');
                url.Append(item.DeclaringType.Type.TypeParameters.Count);
            }

            url.Append('.');
        }
        
        url.Append(item.Name);

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(DotNetField item)
    {
        if (item.DeclaringType != null && item.DeclaringType.Type != null && item.DeclaringType.Type.IsExternal)
        {
            return dotNetOptions.ResolveMicrosoftDocsUrls && microsoftDocsUrlResolver.IsMicrosoftType(item.DeclaringType.Type) ? microsoftDocsUrlResolver.GetUrl(item) : "";
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.DeclaringType != null)
        {
            if (item.DeclaringType.Namespace != null)
            {
                url.Append(item.DeclaringType.Namespace.FullName);
                url.Append('.');
            }

            url.Append(item.DeclaringType.Name);

            if (item.DeclaringType.Type != null && item.DeclaringType.Type.TypeParameters != null && item.DeclaringType.Type.TypeParameters.Count > 0)
            {
                url.Append('-');
                url.Append(item.DeclaringType.Type.TypeParameters.Count);
            }

            url.Append('.');
        }
        
        url.Append(item.Name);

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(DotNetMethodOverload item)
    {
        if (item.DeclaringType != null && item.DeclaringType.Type != null && item.DeclaringType.Type.IsExternal)
        {
            return dotNetOptions.ResolveMicrosoftDocsUrls && microsoftDocsUrlResolver.IsMicrosoftType(item.DeclaringType.Type) ? microsoftDocsUrlResolver.GetUrl(item) : "";
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.DeclaringType != null)
        {
            if (item.DeclaringType.Namespace != null)
            {
                url.Append(item.DeclaringType.Namespace.FullName);
                url.Append('.');
            }

            url.Append(item.DeclaringType.Name);

            if (item.DeclaringType.Type != null && item.DeclaringType.Type.TypeParameters != null && item.DeclaringType.Type.TypeParameters.Count > 0)
            {
                url.Append('-');
                url.Append(item.DeclaringType.Type.TypeParameters.Count);
            }

            url.Append('.');
        }
        
        url.Append(item.IsConstructor ? "ctor" : item.Name);

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        if (item.MethodCollection != null && item.MethodCollection.Overloads.Count > 1)
        {
            url.Append('#');

            if (item.DeclaringType != null)
            {
                if (item.DeclaringType.Namespace != null)
                {
                    url.Append(item.DeclaringType.Namespace.FullName.ToLower().Replace(".", "-"));
                    url.Append('-');
                }

                url.Append(item.DeclaringType.Name.ToLower().Replace(".", "-"));
                url.Append('-');
            }

            url.Append(item.Name.ToLower().Replace(".", "-"));
            url.Append('(');

            if (item.Parameters != null && item.Parameters.Count > 0)
            {
                url.Append(String.Join('-', item.Parameters.Select(p => p.Type.XmlDocKey.ToLower().Replace(".", "-") + (p.IsByRef ? "@" : ""))));
            }

            url.Append(')');
        }

        return url.ToString();
    }

    public string GetUrl(SiteToc item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append("table-of-contents");

        if (Options.IncludeFileExtensions)
        {
            url.Append(".json");
        }

        return url.ToString();
    }

    public string GetUrl(SiteIndex item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append("index");

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(IAsset item)
    {
        return $"/_assets/{item.FilePath.Replace("\\", "/")}";
    }

    public string GetUrl(DotNetIndexer item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.DeclaringType != null)
        {
            if (item.DeclaringType.Namespace != null)
            {
                url.Append(item.DeclaringType.Namespace.FullName);
                url.Append('.');
            }

            url.Append(item.DeclaringType.Name);

            if (item.DeclaringType.Type != null && item.DeclaringType.Type.TypeParameters != null && item.DeclaringType.Type.TypeParameters.Count > 0)
            {
                url.Append('-');
                url.Append(item.DeclaringType.Type.TypeParameters.Count);
            }

            url.Append('.');
        }

        url.Append("Item");

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(MarkdownFile item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append(NonAlphanumericCharacters.Replace(item.Title, "-").ToLower());

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(MarkdownImage item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append("_images/");
        url.Append(Guid.NewGuid().ToString("N").ToLowerInvariant());
        url.Append(Path.GetExtension(item.FilePath).ToLowerInvariant());

        return url.ToString();
    }

    public string GetUrl(TypeScriptModule item)
    {
        if (item.Exports != null)
        {
            return GetUrl(item.Exports);
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append(item.Name);

        return url.ToString();
    }

    public string GetUrl(TypeScriptTypeDeclaration item)
    {
        if (item.BuiltIn)
        {
            if (!typeScriptOptions.ResolveMDNUrls)
            {
                return "";
            }

            string typeName = item.Name;

            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }

            if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}";
            }

            else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            // TODO: throw error
            return "";
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append(item.FullName);

        return url.ToString();
    }

    public string GetUrl(TypeScriptInterface item)
    {
        if (item.BuiltIn)
        {
            if (!typeScriptOptions.ResolveMDNUrls)
            {
                return "";
            }

            string typeName = item.Name;
            
            if (typeName.Contains('<'))
            {
                typeName = typeName[..typeName.IndexOf('<')];
            }
            
            if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}";
            }
            
            else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
            {
                return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}";
            }

            // TODO: throw error
            return "";
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append(item.FullName);

        return url.ToString();
    }

    public string GetUrl(TypeScriptMethod item)
    {
        if (item.SourceTypeId != 0)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];

            if (sourceType.BuiltIn)
            {
                if (!typeScriptOptions.ResolveMDNUrls)
                {
                    return "";
                }

                string typeName = sourceType.Name;

                if (typeName.Contains('<'))
                {
                    typeName = typeName[..typeName.IndexOf('<')];
                }

                if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
                {
                    return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
                }

                else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
                {
                    return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
                }

                // TODO: throw error
                return "";
            }
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.SourceTypeId != 0)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];

            url.Append(sourceType.FullName);
            url.Append('.');
        }

        url.Append(item.Name);

        return url.ToString();
    }

    public string GetUrl(TypeScriptProperty item)
    {
        if (item.SourceTypeId != 0)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];

            if (sourceType.BuiltIn)
            {
                if (!typeScriptOptions.ResolveMDNUrls)
                {
                    return "";
                }

                string typeName = sourceType.Name;

                if (typeName.Contains('<'))
                {
                    typeName = typeName[..typeName.IndexOf('<')];
                }

                if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
                {
                    return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
                }

                else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
                {
                    return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
                }

                // TODO: throw error
                return "";
            }
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.SourceTypeId != 0)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];

            url.Append(sourceType.FullName);
            url.Append('.');
        }

        url.Append(item.Name);

        return url.ToString();
    }

    public string GetUrl(TypeScriptMethodSignature item)
    {
        if (item.SourceTypeId != 0)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];

            if (sourceType.BuiltIn)
            {
                if (!typeScriptOptions.ResolveMDNUrls)
                {
                    return "";
                }

                string typeName = sourceType.Name;

                if (typeName.Contains('<'))
                {
                    typeName = typeName[..typeName.IndexOf('<')];
                }

                if (TypeScriptTypeDeclaration.JavaScriptGlobalObjects.Contains(typeName))
                {
                    return $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{typeName}/{item.Name}";
                }

                else if (TypeScriptTypeDeclaration.JavaScriptBuiltinTypes.Contains(typeName))
                {
                    return $"https://developer.mozilla.org/en-US/docs/Web/API/{typeName}/{item.Name}";
                }

                // TODO: throw error
                return "";
            }
        }

        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        if (item.SourceTypeId != 0)
        {
            TypeScriptTypeDeclaration sourceType = ReferenceType.AllTypes[item.SourceTypeId];

            url.Append(sourceType.FullName);
            url.Append('.');
        }

        url.Append(item.Name);

        if (item.MethodCollection != null && item.MethodCollection.Signatures.Count > 1)
        {
            url.Append('#');

            url.Append(item.Name.ToLower().Replace(".", "-"));
            url.Append('(');

            if (item.Parameters != null && item.Parameters.Count > 0)
            {
                url.Append(String.Join('-', item.Parameters.Select(p => Regex.Replace(p.Type.ToString().ToLower(), "[^a-zA-Z0-9_]", "-"))));
            }

            url.Append(')');
        }

        return url.ToString();
    }

    public string GetUrl(TypeScriptNamespace item)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefixProvider.UrlPrefix))
        {
            url.Append(urlPrefixProvider.UrlPrefix);
            url.Append('/');
        }

        url.Append(item.Name);

        return url.ToString();
    }

    public string GetUrl(IntrinsicType item)
    {
        return typeScriptOptions.ResolveMDNUrls ? item.Name == "void" ? "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/void" : $"https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/{item.Name}" : "";
    }
}

public class KebabCaseUrlProviderOptions
{
    public bool IncludeFileExtensions
    {
        get;
        set;
    } = true;
}

public static class KebabCaseUrlProviderExtensions
{
    public static InsightDocsBuilder UseKebabCaseUrls(this InsightDocsBuilder builder)
    {
        return UseKebabCaseUrls(builder, null);
    }

    public static InsightDocsBuilder UseKebabCaseUrls(this InsightDocsBuilder builder, Action<KebabCaseUrlProviderOptions>? optionsFactory)
    {
        builder.Services.AddScoped<IUrlProvider<DotNetIndex>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetNamespace>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetType>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetMethod>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetMethodOverload>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetProperty>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetField>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<SiteToc>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<SiteIndex>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<IAsset>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetIndexer>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<MarkdownFile>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<MarkdownImage>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<TypeScriptModule>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<TypeScriptTypeDeclaration>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<TypeScriptInterface>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<TypeScriptMethod>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<TypeScriptProperty>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<TypeScriptMethodSignature>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<TypeScriptNamespace>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<IntrinsicType>, KebabCaseUrlProvider>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                KebabCaseUrlProviderOptions options = new();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new KebabCaseUrlProviderOptions());
        }

        return builder;
    }
}