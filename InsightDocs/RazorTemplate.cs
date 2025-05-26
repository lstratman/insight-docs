using System.Text;
using InsightDocs.Abstractions;
using InsightDocs.Model.DotNet;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs;

public class RazorTemplate
{
    public static RenderFragment GetLink<T>(T item, IServiceProvider serviceProvider) where T : ILinkTarget
    {
        IUrlProvider<T> urlProvider = serviceProvider.GetService<IUrlProvider<T>>() ?? throw new Exception("No IUrlProvider service registered for " + typeof(T).Name + ".");
        string url = urlProvider.GetUrl(item);

        return (builder) =>
        {
            if (String.IsNullOrEmpty(url))
            {
                builder.AddMarkupContent(0, item.LinkText.Replace("<", "&lt;").Replace(">", "&gt;"));
            }

            else
            {
                builder.AddMarkupContent(0, $@"<a href=""{url}"">{item.LinkText.Replace("<", "&lt;").Replace(">", "&gt;")}</a>");
            }
        };
    }

    private static void BuildLinkTag(DotNetTypeReference typeReference, StringBuilder linkTagBuilder, IUrlProvider<DotNetType> urlProvider)
    {
        if (typeReference.Type != null)
        {
            string url = urlProvider.GetUrl(typeReference.Type);

            if (String.IsNullOrEmpty(url))
            {
                linkTagBuilder.Append(typeReference.Type.LinkText.Replace("<", "&lt;").Replace(">", "&gt;"));
            }

            else
            {
                linkTagBuilder.Append($@"<a href=""{url}"">{typeReference.Type.Name}</a>");
            }

            if (typeReference.GenericArguments != null && typeReference.GenericArguments.Count > 0)
            {
                linkTagBuilder.Append("&lt;");

                bool first = true;

                foreach (DotNetGenericArgument genericArgument in typeReference.GenericArguments)
                {
                    if (first)
                    {
                        first = false;
                    }

                    else
                    {
                        linkTagBuilder.Append(", ");
                    }

                    if (genericArgument.Type != null)
                    {
                        BuildLinkTag(genericArgument.Type, linkTagBuilder, urlProvider);
                    }

                    else
                    {
                        linkTagBuilder.Append(genericArgument.TypeParameterName!);
                    }
                }

                linkTagBuilder.Append("&gt;");
            }
        }

        else
        {
            linkTagBuilder.Append(typeReference.GenericParameterName!);
        }
    }

    public static RenderFragment GetLink(DotNetTypeReference item, IServiceProvider serviceProvider)
    {
        StringBuilder linkTagBuilder = new();
        IUrlProvider<DotNetType> urlProvider = serviceProvider.GetService<IUrlProvider<DotNetType>>() ?? throw new Exception("No IUrlProvider service registered for DotNetType.");

        BuildLinkTag(item, linkTagBuilder, urlProvider);
        string linkTag = linkTagBuilder.ToString();

        return (builder) =>
        {
            builder.AddMarkupContent(0, linkTag);
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