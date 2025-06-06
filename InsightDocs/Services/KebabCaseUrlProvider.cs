using System.Text;
using InsightDocs.Abstractions;
using InsightDocs.DotNet;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;
using InsightDocs.Site.Model;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Services;

public class KebabCaseUrlProvider(KebabCaseUrlProviderOptions options, IUrlPrefixProvider urlPrefixProvider, IMicrosoftDocsUrlResolver microsoftDocsUrlResolver, DotNetOptions dotNetOptions) 
    : IUrlProvider<DotNetType>,
      IUrlProvider<DotNetIndex>,
      IUrlProvider<DotNetNamespace>,
      IUrlProvider<DotNetMethod>,
      IUrlProvider<DotNetMethodOverload>,
      IUrlProvider<DotNetProperty>,
      IUrlProvider<DotNetField>,
      IUrlProvider<SiteToc>,
      IUrlProvider<SiteIndex>,
      IUrlProvider<ITemplateAsset>,
      IUrlProvider<DotNetIndexer>
{
    protected KebabCaseUrlProviderOptions Options
    {
        get;
        set;
    } = options;

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

            // TODO: parameters

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

    public string GetUrl(ITemplateAsset item)
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
}

public class KebabCaseUrlProviderOptions
{
    public bool IncludeFileExtensions
    {
        get;
        set;
    } = false;
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
        builder.Services.AddScoped<IUrlProvider<ITemplateAsset>, KebabCaseUrlProvider>();
        builder.Services.AddScoped<IUrlProvider<DotNetIndexer>, KebabCaseUrlProvider>();

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