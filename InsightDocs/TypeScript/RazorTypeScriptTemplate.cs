using InsightDocs.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace InsightDocs.TypeScript;

public class RazorTypeScriptTemplate<T> : RazorTemplate<T>
{
    public virtual RenderFragment GetLink<TItem>(TItem item, bool simple) where TItem : ILinkTarget
    {
        IUrlProvider<TItem> urlProvider = ServiceProvider.GetRequiredService<IUrlProvider<TItem>>();
        string url = urlProvider.GetUrl(item);
        string linkText = item.LinkText.Replace("<", "&lt;").Replace(">", "&gt;");

        if (simple && linkText.Contains('.'))
        {
            linkText = linkText[(linkText.IndexOf('.') + 1)..];
        }

        UrlChecker.RegisterUrl(url, Url);

        return (builder) =>
        {
            if (String.IsNullOrEmpty(url))
            {
                builder.AddMarkupContent(0, linkText);
            }

            else
            {
                builder.AddMarkupContent(0, $@"<a href=""{url}""{(url.StartsWith("https://") || url.StartsWith("http://") ? " target=\"_blank\"" : "")}>{linkText}</a>");
            }
        };
    }

    public void BuildLinkTag(TypeScriptType item, StringBuilder linkTagBuilder, bool prettyPrint = false)
    {
        bool noBreaks = false;

        foreach (TypeToStringComponent component in item.GetToStringComponents())
        {
            if (component is TypeToStringReferenceTypeComponent referenceTypeComponent)
            {
                if (ReferenceType.AllTypes.TryGetValue(referenceTypeComponent.Id, out TypeScriptTypeDeclaration? typeDeclaration))
                {
                    // If we have a type declaration, we can use it to build the link.
                    IUrlProvider<TypeScriptTypeDeclaration> urlProvider = ServiceProvider.GetRequiredService<IUrlProvider<TypeScriptTypeDeclaration>>();
                    string url = urlProvider.GetUrl(typeDeclaration);
                    string shortName = typeDeclaration.Name;

                    UrlChecker.RegisterUrl(url, Url);

                    if (shortName.Contains('<'))
                    {
                        shortName = shortName[..shortName.IndexOf('<')];
                    }

                    if (String.IsNullOrEmpty(url))
                    {
                        linkTagBuilder.Append(shortName);
                    }

                    else
                    {
                        linkTagBuilder.Append($@"<a href=""{url}""{(url.StartsWith("https://") || url.StartsWith("http://") ? " target=\"_blank\"" : "")}>{shortName}</a>");
                    }
                }

                else
                {
                    throw new Exception($"Type declaration for reference type with ID {referenceTypeComponent.Id} not found.");
                }
            }

            else if (component is TypeToStringIntrinsicTypeComponent intrinsicTypeComponent)
            {
                IUrlProvider<IntrinsicType> urlProvider = ServiceProvider.GetRequiredService<IUrlProvider<IntrinsicType>>();
                string url = urlProvider.GetUrl(intrinsicTypeComponent.Type);

                UrlChecker.RegisterUrl(url, Url);

                if (String.IsNullOrEmpty(url))
                {
                    linkTagBuilder.Append(intrinsicTypeComponent.Type.Name);
                }

                else
                {
                    linkTagBuilder.Append($@"<a href=""{url}""{(url.StartsWith("https://") || url.StartsWith("http://") ? " target=\"_blank\"" : "")}>{intrinsicTypeComponent.Type.Name}</a>");
                }
            }

            else if (component is TypeToStringTypeComponent typeComponent)
            {
                BuildLinkTag(typeComponent.Type, linkTagBuilder);
            }

            else if (component is TypeToStringTextComponent textComponent)
            {
                if (textComponent.Text.EndsWith(": ("))
                {
                    noBreaks = true;
                }

                else if (textComponent.Text == ") => ")
                {
                    noBreaks = false;
                }

                if (prettyPrint && item is ReflectionType && textComponent.Text == " }")
                {
                    linkTagBuilder.Append("</div>");
                }

                linkTagBuilder.Append(textComponent.Text.Replace("<", "&lt;").Replace(">", "&gt;"));

                if (prettyPrint && item is ReflectionType)
                {
                    if (textComponent.Text == "{ ")
                    {
                        linkTagBuilder.Append("<div class=\"propertiesList\">");
                    }

                    else if (textComponent.Text == ", " && !noBreaks)
                    {
                        linkTagBuilder.Append("<br/>");
                    }
                }
            }
        }
    }

    public virtual RenderFragment GetLink(TypeScriptType item, bool prettyPrint = false)
    {
        StringBuilder linkTagBuilder = new();

        BuildLinkTag(item, linkTagBuilder, prettyPrint);
        string linkTag = linkTagBuilder.ToString();

        return (builder) =>
        {
            builder.AddMarkupContent(0, linkTag);
        };
    }
}
