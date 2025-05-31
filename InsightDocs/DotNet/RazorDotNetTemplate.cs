using System.Text;
using InsightDocs.Abstractions;
using InsightDocs.DotNet.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.DotNet;

public class RazorDotNetTemplate<T> : RazorTemplate<T>
{
    protected virtual void BuildLinkTag(DotNetTypeReference typeReference, StringBuilder linkTagBuilder, IUrlProvider<DotNetType> urlProvider)
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

    public virtual RenderFragment GetLink(DotNetTypeReference item)
    {
        StringBuilder linkTagBuilder = new();
        IUrlProvider<DotNetType> urlProvider = ServiceProvider!.GetService<IUrlProvider<DotNetType>>() ?? throw new Exception("No IUrlProvider service registered for DotNetType.");

        BuildLinkTag(item, linkTagBuilder, urlProvider);
        string linkTag = linkTagBuilder.ToString();

        return (builder) =>
        {
            builder.AddMarkupContent(0, linkTag);
        };
    }
}