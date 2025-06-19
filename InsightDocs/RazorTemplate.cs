using InsightDocs.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace InsightDocs;

public class RazorTemplate<T> : ComponentBase
{
    [Inject]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public IServiceProvider ServiceProvider
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        get;
        set;
    }

    [Parameter]
    public T? Item 
    { 
        get; 
        set; 
    }

    public virtual string GetUrl<TItem>(TItem item) where TItem : ILinkTarget
    {
        IUrlProvider<TItem> urlProvider = ServiceProvider.GetRequiredService<IUrlProvider<TItem>>();
        return urlProvider.GetUrl(item);
    }

    public virtual RenderFragment GetLink<TItem>(TItem item) where TItem : ILinkTarget
    {
        IUrlProvider<TItem> urlProvider = ServiceProvider.GetRequiredService<IUrlProvider<TItem>>();
        string url = urlProvider.GetUrl(item);

        return (builder) =>
        {
            if (String.IsNullOrEmpty(url))
            {
                builder.AddMarkupContent(0, item.LinkText.Replace("<", "&lt;").Replace(">", "&gt;"));
            }

            else
            {
                builder.AddMarkupContent(0, $@"<a href=""{url}""{(url.StartsWith("https://") || url.StartsWith("http://") ? " target=\"_blank\"" : "")}>{item.LinkText.Replace("<", "&lt;").Replace(">", "&gt;")}</a>");
            }
        };
    }

    public virtual RenderFragment Raw(string html, string? fallbackWrapperTag = null)
    {
        if (!html.StartsWith('<') && !String.IsNullOrEmpty(fallbackWrapperTag))
        {
            html = $"<{fallbackWrapperTag}>{html}</{fallbackWrapperTag}>";
        }

        return (builder) =>
        {
            builder.AddMarkupContent(0, html);
        };
    }

    public virtual RenderFragment AddAdditionalCss()
    {
        IEnumerable<IAdditionalCssProvider<T>> additionalCssProviders = ServiceProvider.GetServices<IAdditionalCssProvider<T>>();
        List<IAsset> additionalCssAssets = new List<IAsset>();

        if (additionalCssProviders != null && additionalCssProviders.Any())
        {
            foreach (IAdditionalCssProvider<T> additionalCssProvider in additionalCssProviders)
            {
                additionalCssAssets.AddRange(additionalCssProvider.GetAdditionalCssAssets(Item!));
            }
        }

        return (builder) =>
        {
            StringBuilder cssLinks = new StringBuilder();

            foreach (IAsset asset in additionalCssAssets)
            {
                string url = GetUrl(asset);
                cssLinks.AppendLine($@"<link rel=""stylesheet"" type=""text/css"" href=""{url}"" />");
            }

            builder.AddMarkupContent(0, cssLinks.ToString());
        };
    }
}