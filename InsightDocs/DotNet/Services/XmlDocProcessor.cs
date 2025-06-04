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
                if (xmlElement.GetAttribute("type") == "bullet")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent("ul", xmlElement, serviceProvider));
                }

                else if (xmlElement.GetAttribute("type") == "number")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent("ol", xmlElement, serviceProvider));
                }

                else if (xmlElement.GetAttribute("type") == "table")
                {
                    components.Add(new XmlDocCommentHtmlTagComponent("table", xmlElement, serviceProvider)
                    {
                        Attributes = new Dictionary<string, string>
                        {
                            { "class", "table table-sm table-complex" }
                        }
                    });
                }

                else
                {
                    throw new Exception("Unsupported XMLDoc <list> type attribute: " + xmlElement.GetAttribute("type") + ".");
                }
            }

            else if (xmlElement.Name == "item")
            {
                if (xmlElement.ParentNode is XmlElement parentElement)
                {
                    if (parentElement.GetAttribute("type") == "bullet" || parentElement.GetAttribute("type") == "number")
                    {
                        components.Add(new XmlDocCommentHtmlTagComponent("li", xmlElement, serviceProvider));
                    }

                    else if (parentElement.GetAttribute("type") == "table")
                    {
                        components.Add(new XmlDocCommentHtmlTagComponent("tr", xmlElement, serviceProvider));
                    }

                    else
                    {
                        throw new Exception("Unsupported XMLDoc <list> type attribute: " + parentElement.GetAttribute("type") + ".");
                    }
                }

                else
                {
                    throw new Exception("Unable to determine parent list type for <item> node.");
                }
            }

            else if (xmlElement.Name == "listheader")
            {
                XmlDocCommentHtmlTagComponent thead = new XmlDocCommentHtmlTagComponent("thead", null, serviceProvider);
                thead.ChildComponents.Add(new XmlDocCommentHtmlTagComponent("tr", xmlElement, serviceProvider));

                components.Add(thead);
            }

            else if (xmlElement.Name == "term")
            {
                if (xmlElement.ParentNode is XmlElement parentElement && parentElement.ParentNode is XmlElement grandParentElement)
                {
                    if (grandParentElement.GetAttribute("type") == "bullet" || grandParentElement.GetAttribute("type") == "number")
                    {
                        foreach (XmlNode childNode in xmlElement.ChildNodes)
                        {
                            ProcessCommentNodeChild(childNode, components);
                        }
                    }

                    else if (grandParentElement.GetAttribute("type") == "table")
                    {
                        components.Add(new XmlDocCommentHtmlTagComponent("td", xmlElement, serviceProvider));
                    }

                    else
                    {
                        throw new Exception("Unsupported XMLDoc <list> type attribute: " + grandParentElement.GetAttribute("type") + ".");
                    }
                }

                else
                {
                    throw new Exception("Unable to determine parent list type for <item> node.");
                }
            }

            else if (xmlElement.Name == "description")
            {
                if (xmlElement.ParentNode is XmlElement parentElement && parentElement.ParentNode is XmlElement grandParentElement)
                {
                    if (grandParentElement.GetAttribute("type") == "bullet" || grandParentElement.GetAttribute("type") == "number")
                    {
                        if (parentElement.SelectSingleNode("term") != null)
                        {
                            components.Add(new XmlDocCommentTextComponent(" - "));
                        }

                        foreach (XmlNode childNode in xmlElement.ChildNodes)
                        {
                            ProcessCommentNodeChild(childNode, components);
                        }
                    }

                    else if (grandParentElement.GetAttribute("type") == "table")
                    {
                        components.Add(new XmlDocCommentHtmlTagComponent("td", xmlElement, serviceProvider));
                    }

                    else
                    {
                        throw new Exception("Unsupported XMLDoc <list> type attribute: " + grandParentElement.GetAttribute("type") + ".");
                    }
                }

                else
                {
                    throw new Exception("Unable to determine parent list type for <item> node.");
                }
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