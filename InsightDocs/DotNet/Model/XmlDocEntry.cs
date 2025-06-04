using System.Xml;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.DotNet.Model;

public class XmlDocEntry(string key)
{
    public string Key
    {
        get;
        set;
    } = key;

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

    public List<Tuple<string, List<XmlDocCommentComponent>>>? Examples
    {
        get;
        set;
    }
}

public abstract class XmlDocCommentComponent
{
    public abstract string ToHtml();
}

public class XmlDocCommentHtmlTagComponent : XmlDocCommentComponent
{
    public XmlDocCommentHtmlTagComponent(string tagName, XmlElement? xmlNode, IServiceProvider serviceProvider)
    {
        TagName = tagName;

        if (xmlNode != null)
        {
            IXmlDocProcessor xmlDocProcessor = serviceProvider.GetRequiredService<IXmlDocProcessor>();
            ChildComponents = xmlDocProcessor.ProcessCommentNode(xmlNode);
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

    public Dictionary<string, string>? Attributes
    {
        get;
        set;
    }

    public override string ToHtml()
    {
        return $"<{TagName}{(Attributes != null && Attributes.Any() ? (" " + String.Join(' ', Attributes.Select(a => a.Key + "=\"" + a.Value + "\""))) : "")}>{String.Join(" ", ChildComponents.Select(c => c.ToHtml()))}</{TagName}>";
    }
}

public class XmlDocSeeTagComponent(XmlElement xmlElement, IServiceProvider serviceProvider) : XmlDocCommentComponent
{
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
            IXmlDocUrlResolver xmlDocUrlResolver = serviceProvider.GetRequiredService<IXmlDocUrlResolver>();

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

public class XmlDocException
{
    public XmlDocException(XmlElement childNode, IServiceProvider serviceProvider)
    {
        Exception = new XmlDocSeeTagComponent(childNode, serviceProvider)
        {
            LinkText = ""
        };

        IXmlDocProcessor xmlDocProcessor = serviceProvider.GetRequiredService<IXmlDocProcessor>();
        Text = xmlDocProcessor.ProcessCommentNode(childNode);
    }

    public XmlDocSeeTagComponent Exception
    {
        get;
        set;
    }

    public List<XmlDocCommentComponent> Text
    {
        get;
        set;
    }
}