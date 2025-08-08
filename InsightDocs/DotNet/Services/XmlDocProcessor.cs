using InsightDocs.DotNet.Abstractions;
using InsightDocs.DotNet.Model;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Xml;

namespace InsightDocs.DotNet.Services;

public partial class XmlDocProcessor(IServiceProvider serviceProvider, DotNetOptions dotNetOptions, ILoggerFactory loggerFactory) : IXmlDocProcessor
{
    protected ILogger _logger = loggerFactory.CreateLogger<XmlDocProcessor>();

    [LoggerMessage(LogLevel.Warning, "Unsupported XMLDoc node: {nodeName}")]
    public static partial void LogWarnUnsupportedXmlDocNode(ILogger logger, string nodeName);

    [LoggerMessage(LogLevel.Warning, "Unsupported XMLDoc node type: {nodeType}")]
    public static partial void LogWarnUnsupportedXmlDocNodeType(ILogger logger, string nodeType);

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
                    if (parentElement.GetAttribute("type") is "bullet" or "number")
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
                    if (grandParentElement.GetAttribute("type") is "bullet" or "number")
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
                    if (grandParentElement.GetAttribute("type") is "bullet" or "number")
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

            else if (xmlElement.Name == "c")
            {
                components.Add(new XmlDocCommentHtmlTagComponent("code", xmlElement, serviceProvider));
            }

            else if (xmlElement.Name == "code")
            {
                if (xmlElement.ParentNode is XmlElement parentElement && parentElement.Name == "example")
                {
                    XmlDocCommentHtmlTagComponent preTag = new XmlDocCommentHtmlTagComponent("pre", null, serviceProvider);

                    if (xmlElement.ChildNodes != null && xmlElement.ChildNodes[0] is XmlText textNode)
                    {
                        string codeText = textNode.InnerText;

                        if (codeText.StartsWith('\r'))
                        {
                            codeText = codeText[1..];
                        }

                        if (codeText.StartsWith('\n'))
                        {
                            codeText = codeText[1..];
                        }

                        Match leadingSpaces = Regex.Match(codeText, @"^\s+");

                        if (leadingSpaces.Success)
                        {
                            codeText = codeText.Trim().Replace("\n" + leadingSpaces.Value, "\n");
                        }

                        XmlDocCommentHtmlTagComponent codeTag = new XmlDocCommentHtmlTagComponent("code", null, serviceProvider);
                        codeTag.ChildComponents.Add(new XmlDocCommentTextComponent(codeText));

                        preTag.ChildComponents.Add(codeTag);
                    }

                    else
                    {
                        preTag.ChildComponents.Add(new XmlDocCommentHtmlTagComponent("code", xmlElement, serviceProvider));
                    }

                    components.Add(preTag);
                }

                else
                {
                    components.Add(new XmlDocCommentHtmlTagComponent("code", xmlElement, serviceProvider));
                }
            }

            else if (xmlElement.Name is "see" or "seealso")
            {
                components.Add(new XmlDocSeeTagComponent(xmlElement, serviceProvider));
            }

            else if (xmlElement.Name is "paramref" or "typeparamref")
            {
                XmlDocCommentHtmlTagComponent codeTag = new("code", null, serviceProvider);
                codeTag.ChildComponents.Add(new XmlDocCommentTextComponent(xmlElement.GetAttribute("name")));

                components.Add(codeTag);
            }

            else
            {
                if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Warn)
                {
                    LogWarnUnsupportedXmlDocNode(_logger, xmlElement.Name);
                }

                else if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Error)
                {
                    throw new Exception("Unsupported XMLDoc node: " + xmlElement.Name + ".");
                }
            }
        }

        else
        {
            if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Warn)
            {
                LogWarnUnsupportedXmlDocNodeType(_logger, node.NodeType.ToString("G"));
            }

            else if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Error)
            {
                throw new Exception("Unsupported XMLDoc node type: " + node.NodeType.ToString("G") + ".");
            }
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
            List<XmlDocCommentComponent> example = ProcessCommentNode(childNode);
            string sampleDescription = "";

            if (example.Count > 0)
            {
                xmlDocEntry.Examples ??= [];

                if (example[0] is XmlDocCommentTextComponent textComponent)
                {
                    sampleDescription = textComponent.Text;
                    example.RemoveAt(0);
                }

                xmlDocEntry.Examples.Add(new Tuple<string, List<XmlDocCommentComponent>>(sampleDescription, example));
            }
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
            if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Warn)
            {
                LogWarnUnsupportedXmlDocNode(_logger, childNode.Name);
            }

            else if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Error)
            {
                throw new Exception("Unsupported XMLDoc node: " + childNode.Name + ".");
            }
        }
    }

    public virtual XmlDocEntry ProcessMemberNode(XmlElement memberNode)
    {
        XmlDocEntry xmlDocEntry = new XmlDocEntry(memberNode.GetAttribute("name"));

        foreach (XmlNode childNode in memberNode.ChildNodes)
        {
            if (childNode is XmlElement childElement)
            {
                ProcessMemberNodeChild(xmlDocEntry, childElement);
            }

            else
            {
                if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Warn)
                {
                    LogWarnUnsupportedXmlDocNodeType(_logger, childNode.NodeType.ToString("G"));
                }

                else if (dotNetOptions.UnsupportedXmlDocTagBehavior == ErrorBehavior.Error)
                {
                    throw new Exception("Unsupported XMLDoc node type: " + childNode.NodeType.ToString("G") + ".");
                }
            }
        }

        return xmlDocEntry;
    }
}