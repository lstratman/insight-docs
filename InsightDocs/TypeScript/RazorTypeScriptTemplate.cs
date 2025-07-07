using InsightDocs.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace InsightDocs.TypeScript;

public class RazorTypeScriptTemplate<T> : RazorTemplate<T>
{
    public void BuildLinkTag(TypeScriptType item, StringBuilder linkTagBuilder)
    {
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
                linkTagBuilder.Append(textComponent.Text.Replace("<", "&lt;").Replace(">", "&gt;"));
            }
        }
    }

    public virtual RenderFragment GetLink(TypeScriptType item)
    {
        StringBuilder linkTagBuilder = new();

        BuildLinkTag(item, linkTagBuilder);
        string linkTag = linkTagBuilder.ToString();

        return (builder) =>
        {
            builder.AddMarkupContent(0, linkTag);
        };
    }
}
