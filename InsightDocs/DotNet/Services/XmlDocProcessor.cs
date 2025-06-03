using System.Xml;
using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;

namespace InsightDocs.DotNet.Services;

public class XmlDocProcessor(IServiceProvider serviceProvider) : IXmlDocProcessor
{
    protected virtual void ProcessCommentNodeChild(XmlNode node, List<XmlDocCommentComponent> components)
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

    public virtual List<XmlDocCommentComponent> ProcessCommentNode(XmlElement commentNode)
    {
        List<XmlDocCommentComponent> components = [];

        foreach (XmlNode node in commentNode.ChildNodes)
        {
            ProcessCommentNodeChild(node, components);
        }

        return components;
    }

    protected virtual void ProcessMemberNodeChild(XmlDocEntry xmlDocEntry, XmlElement childNode)
    {
        if (childNode.Name == "summary")
            {
                xmlDocEntry.Summary = ProcessCommentNode(childNode);
            }

            else if (childNode.Name == "remarks")
            {
                xmlDocEntry.Remarks = ProcessCommentNode(childNode);
            }

            else if (childNode.Name == "returns")
            {
                xmlDocEntry.Returns = ProcessCommentNode(childNode);
            }

            else if (childNode.Name == "value")
            {
                xmlDocEntry.Value = ProcessCommentNode(childNode);
            }

            else if (childNode.Name == "param")
            {
                xmlDocEntry.Parameters ??= [];
                xmlDocEntry.Parameters[childNode.GetAttribute("name")] = ProcessCommentNode(childNode);
            }

            else if (childNode.Name == "typeparam")
            {
                xmlDocEntry.TypeParameters ??= [];
                xmlDocEntry.TypeParameters[childNode.GetAttribute("name")] = ProcessCommentNode(childNode);
            }

            else if (childNode.Name == "seealso")
            {
                xmlDocEntry.SeeAlso ??= [];
                xmlDocEntry.SeeAlso.Add(new XmlDocSeeTagComponent(childNode, serviceProvider));
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
                xmlDocEntry.Exceptions ??= [];
                xmlDocEntry.Exceptions.Add(new(childNode, serviceProvider));
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

    public virtual XmlDocEntry ProcessMemberNode(XmlElement memberNode)
    {
        XmlDocEntry xmlDocEntry = new XmlDocEntry(memberNode.GetAttribute("name"));

        foreach (XmlElement childNode in memberNode.ChildNodes)
        {
            ProcessMemberNodeChild(xmlDocEntry, childNode);
        }

        return xmlDocEntry;
    }
}