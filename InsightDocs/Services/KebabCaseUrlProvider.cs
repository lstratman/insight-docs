using System.Text;
using InsightDocs.Abstractions;
using InsightDocs.Model.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Services;

public class KebabCaseUrlProvider(KebabCaseUrlProviderOptions options) : IUrlProvider<DotNetType>, IUrlProvider<DotNetIndex>, IUrlProvider<DotNetNamespace>
{
    protected KebabCaseUrlProviderOptions Options
    {
        get;
        set;
    } = options;

    public string GetUrl(DotNetType item, string? urlPrefix = null)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefix))
        {
            url.Append(urlPrefix);
            url.Append('/');
        }

        if (item.Namespace != null)
        {
            url.Append(item.Namespace.FullName.ToLower().Replace(".", "-"));
            url.Append('-');
        }

        url.Append(item.Name.ToLower());

        if (item.TypeParameters != null)
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

    public string GetUrl(DotNetIndex item, string? urlPrefix = null)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefix))
        {
            url.Append(urlPrefix);
            url.Append('/');
        }

        url.Append("index");

        if (Options.IncludeFileExtensions)
        {
            url.Append(".html");
        }

        return url.ToString();
    }

    public string GetUrl(DotNetNamespace item, string? urlPrefix = null)
    {
        StringBuilder url = new();

        if (!String.IsNullOrEmpty(urlPrefix))
        {
            url.Append(urlPrefix);
            url.Append('/');
        }

        url.Append(item.FullName.ToLower().Replace(".", "-"));

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
    public static Builder UseKebabCaseUrls(this Builder builder)
    {
        return UseKebabCaseUrls(builder, null);
    }

    public static Builder UseKebabCaseUrls(this Builder builder, Action<KebabCaseUrlProviderOptions>? optionsFactory)
    {
        builder.Services.AddSingleton<IUrlProvider<DotNetIndex>, KebabCaseUrlProvider>();
        builder.Services.AddSingleton<IUrlProvider<DotNetNamespace>, KebabCaseUrlProvider>();
        builder.Services.AddSingleton<IUrlProvider<DotNetType>, KebabCaseUrlProvider>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) => {
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