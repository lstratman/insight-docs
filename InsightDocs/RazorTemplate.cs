using InsightDocs.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs;

public class RazorTemplate
{
    public static RenderFragment GetLink<T>(T item, string text, IServiceProvider serviceProvider)
    {
        IUrlProvider<T> urlProvider = serviceProvider.GetService<IUrlProvider<T>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(T).Name + ".");
        string url = urlProvider.GetUrl(item);

        return (builder) =>
        {
            if (String.IsNullOrEmpty(url))
            {
                builder.AddMarkupContent(0, text.Replace("<", "&lt;").Replace(">", "&gt;"));
            }

            else
            {
                builder.AddMarkupContent(0, $@"<a href=""{url}"">{text.Replace("<", "&lt;").Replace(">", "&gt;")}</a>");
            }
        };
    }

    public static RenderFragment Raw(string html, string? fallbackWrapperTag = null)
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