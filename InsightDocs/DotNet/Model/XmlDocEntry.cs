using System.Xml;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.DotNet.Model;

public class XmlDocEntry
{
    public XmlDocEntry(string key, XmlElement xmlNode, IServiceProvider serviceProvider)
    {
        Key = key;

        foreach (XmlElement childNode in xmlNode.ChildNodes)
        {
            if (childNode.Name == "summary")
            {
                Summary = XmlDocCommentComponent.Process(childNode, serviceProvider);
            }

            else if (childNode.Name == "remarks")
            {
                Remarks = XmlDocCommentComponent.Process(childNode, serviceProvider);
            }

            else if (childNode.Name == "returns")
            {
                Returns = XmlDocCommentComponent.Process(childNode, serviceProvider);
            }

            else if (childNode.Name == "value")
            {
                Value = XmlDocCommentComponent.Process(childNode, serviceProvider);
            }

            else if (childNode.Name == "param")
            {
                Parameters ??= [];
                Parameters[childNode.GetAttribute("name")] = XmlDocCommentComponent.Process(childNode, serviceProvider);
            }

            else if (childNode.Name == "typeparam")
            {
                TypeParameters ??= [];
                TypeParameters[childNode.GetAttribute("name")] = XmlDocCommentComponent.Process(childNode, serviceProvider);
            }

            else if (childNode.Name == "seealso")
            {
                SeeAlso ??= [];
                SeeAlso.Add(new XmlDocSeeTagComponent(childNode, serviceProvider));
            }

            else if (childNode.Name == "example")
            {
                // TODO
            }

            else if (childNode.Name == "history")
            {
                // TODO
            }

            else if (childNode.Name == "para")
            {
                // TODO: possibly fix source
            }

            else if (childNode.Name == "return")
            {
                // TODO: possibly fix source
            }

            else if (childNode.Name == "exception")
            {
                Exceptions ??= [];
                Exceptions.Add(new(childNode, serviceProvider));
            }

            else if (childNode.Name == "inheritdoc")
            {
                // TODO
            }

            else
            {
                throw new Exception("Unsupported XmlDoc node: " + childNode.Name + ".");
            }
        }
    }

    public string Key
    {
        get;
        set;
    }

    public List<XmlDocCommentComponent>? Summary
    {
        get;
        set;
    }

    public List<XmlDocCommentComponent>? Remarks
    {
        get;
        set;
    }

    public List<XmlDocCommentComponent>? Returns
    {
        get;
        set;
    }

    public List<XmlDocCommentComponent>? Value
    {
        get;
        set;
    }

    public Dictionary<string, List<XmlDocCommentComponent>>? Parameters
    {
        get;
        set;
    }

    public Dictionary<string, List<XmlDocCommentComponent>>? TypeParameters
    {
        get;
        set;
    }

    public List<XmlDocSeeTagComponent>? SeeAlso
    {
        get;
        set;
    }

    public List<XmlDocException>? Exceptions
    {
        get;
        set;
    }
}

public abstract class XmlDocCommentComponent
{
    public static List<XmlDocCommentComponent> Process(XmlElement xmlNode, IServiceProvider serviceProvider)
    {
        List<XmlDocCommentComponent> components = [];

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            if (node is XmlText xmlText)
            {
                components.Add(new XmlDocCommentTextComponent(xmlText.InnerText));
            }

            else if (node is XmlElement xmlElement)
            {
                if (xmlElement.Name == "para")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent("p", xmlElement, serviceProvider));
                }

                // TODO: move to custom
                else if (xmlElement.Name == "b" || xmlElement.Name == "i" || xmlElement.Name == "u" || xmlElement.Name == "strike" || xmlElement.Name == "p" || xmlElement.Name == "br")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent(xmlElement.Name, xmlElement, serviceProvider));
                }

                else if (xmlElement.Name == "list")
                {
                    // TODO
                }

                else if (xmlElement.Name == "code" || xmlElement.Name == "c")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent("code", xmlElement, serviceProvider));
                }

                else if (xmlElement.Name == "see" || xmlElement.Name == "seealso")
                {
                    components.Add(new XmlDocSeeTagComponent(xmlElement, serviceProvider));
                }

                else if (xmlElement.Name == "example")
                {
                    // TODO
                }

                else if (xmlElement.Name == "paramref" || xmlElement.Name == "typeparamref")
                {
                    XmlDocCommentHtmlTagComponent codeTag = new("code", null, serviceProvider);
                    codeTag.ChildComponents.Add(new XmlDocCommentTextComponent(xmlElement.GetAttribute("name")));

                    components.Add(codeTag);
                }

                else
                {
                    throw new Exception("Unsupported XmlDoc node: " + xmlElement.Name + ".");
                }
            }

            else
            {
                throw new Exception("Unsupported XmlDoc node type: " + node.NodeType.ToString("G") + ".");
            }
        }

        return components;
    }

    public abstract string ToHtml();
}

public class XmlDocCommentHtmlTagComponent : XmlDocCommentComponent
{
    public XmlDocCommentHtmlTagComponent(string tagName, XmlElement? xmlNode, IServiceProvider serviceProvider)
    {
        TagName = tagName;

        if (xmlNode != null)
        {
            ChildComponents = Process(xmlNode, serviceProvider);
        }

        else
        {
            ChildComponents = [];
        }
    }

    public string TagName
    {
        get;
        set;
    }

    public List<XmlDocCommentComponent> ChildComponents
    {
        get;
        set;
    }

    public override string ToHtml()
    {
        return $"<{TagName}>{String.Join(" ", ChildComponents.Select(c => c.ToHtml()))}</{TagName}>";
    }
}

public class XmlDocSeeTagComponent(XmlElement xmlElement, IServiceProvider serviceProvider) : XmlDocCommentComponent
{
    protected IServiceProvider _serviceProvider = serviceProvider;

    public string CRef
    {
        get;
        set;
    } = xmlElement.GetAttribute("cref");

    public string Url
    {
        get;
        set;
    } = xmlElement.GetAttribute("url");

    public string Href
    {
        get;
        set;
    } = xmlElement.GetAttribute("href");

    public string LangWord
    {
        get;
        set;
    } = xmlElement.GetAttribute("langword");

    public string LinkText
    {
        get;
        set;
    } = xmlElement.InnerText;

    public override string ToHtml()
    {
        if (!String.IsNullOrEmpty(LangWord))
        {
            return $@"<code>{LangWord}</code>";
        }

        else if (!String.IsNullOrEmpty(CRef))
        {
            IXmlDocUrlResolver xmlDocUrlResolver = _serviceProvider.GetRequiredService<IXmlDocUrlResolver>();

            string url = CRef.StartsWith('!') ? "about:blank" : xmlDocUrlResolver.GetUrl(CRef);
            string linkText = String.IsNullOrEmpty(LinkText) ? xmlDocUrlResolver.GetLinkText(CRef) : LinkText;

            return $@"<a href=""{url}"">{linkText}</a>";
        }

        else if (!String.IsNullOrEmpty(Url))
        {
            string linkText = String.IsNullOrEmpty(LinkText) ? Url : LinkText;
            return $@"<a href=""{Url}"">{linkText}</a>";
        }

        else if (!String.IsNullOrEmpty(Href))
        {
            string linkText = String.IsNullOrEmpty(LinkText) ? Href : LinkText;
            return $@"<a href=""{Href}"">{linkText}</a>";
        }

        return "";
    }
}

public class XmlDocCommentTextComponent(string text) : XmlDocCommentComponent
{
    public string Text
    {
        get;
        set;
    } = text.Trim();

    public override string ToHtml()
    {
        return Text;
    }
}

public class XmlDocException(XmlElement childNode, IServiceProvider serviceProvider)
{
    public XmlDocSeeTagComponent Exception
    {
        get;
        set;
    } = new XmlDocSeeTagComponent(childNode, serviceProvider) { LinkText = "" };

    public List<XmlDocCommentComponent> Text
    {
        get;
        set;
    } = XmlDocCommentComponent.Process(childNode, serviceProvider);
}