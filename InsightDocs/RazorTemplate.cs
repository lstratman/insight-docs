using InsightDocs.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs;

public class RazorTemplate<T> : ComponentBase
{
    [Parameter]
    public IServiceProvider? ServiceProvider
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
        IUrlProvider<TItem> urlProvider = ServiceProvider!.GetService<IUrlProvider<TItem>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(TItem).Name + ".");
        return urlProvider.GetUrl(item);
    }

    public virtual RenderFragment GetLink<TItem>(TItem item) where TItem : ILinkTarget
    {
        IUrlProvider<TItem> urlProvider = ServiceProvider!.GetService<IUrlProvider<TItem>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(TItem).Name + ".");
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
}