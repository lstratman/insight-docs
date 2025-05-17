using System.Xml;

namespace InsightDocs.Model.DotNet;

public class XmlDocEntry
{
    public XmlDocEntry(string key, XmlElement xmlNode)
    {
        Key = key;

        foreach (XmlElement childNode in xmlNode.ChildNodes)
        {
            if (childNode.Name == "summary")
            {
                Summary = XmlDocCommentComponent.Process(childNode);
            }

            else if (childNode.Name == "remarks")
            {
                Remarks = XmlDocCommentComponent.Process(childNode);
            }

            else if (childNode.Name == "returns")
            {
                Returns = XmlDocCommentComponent.Process(childNode);
            }

            else if (childNode.Name == "value")
            {
                Value = XmlDocCommentComponent.Process(childNode);
            }

            else if (childNode.Name == "param")
            {
                Parameters ??= [];
                Parameters[childNode.GetAttribute("name")] = XmlDocCommentComponent.Process(childNode);
            }

            else if (childNode.Name == "typeparam")
            {
                TypeParameters ??= [];
                TypeParameters[childNode.GetAttribute("name")] = XmlDocCommentComponent.Process(childNode);
            }

            else if (childNode.Name == "seealso")
            {
                // TODO
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
                // TODO
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
}

public abstract class XmlDocCommentComponent
{
    public static List<XmlDocCommentComponent> Process(XmlElement xmlNode)
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
                    components.Add(new XmlDocCommentHtmlTagComponent("p", xmlElement));
                }

                else if (xmlElement.Name == "b" || xmlElement.Name == "i" || xmlElement.Name == "u" || xmlElement.Name == "strike" || xmlElement.Name == "p" || xmlElement.Name == "br")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent(xmlElement.Name, xmlElement));
                }

                else if (xmlElement.Name == "list")
                {
                    // TODO
                }

                else if (xmlElement.Name == "code" || xmlElement.Name == "c")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent("code", xmlElement));
                }

                else if (xmlElement.Name == "see")
                {
                    // TODO
                }

                else if (xmlElement.Name == "seealso")
                {
                    // TODO
                }

                else if (xmlElement.Name == "example")
                {
                    // TODO
                }

                else if (xmlElement.Name == "paramref" || xmlElement.Name == "typeparamref")
                {
                    XmlDocCommentHtmlTagComponent codeTag = new XmlDocCommentHtmlTagComponent("code", null);
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
    public XmlDocCommentHtmlTagComponent(string tagName, XmlElement? xmlNode)
    {
        TagName = tagName;

        if (xmlNode != null)
        {
            ChildComponents = Process(xmlNode);
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