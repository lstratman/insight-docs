using System.Xml;
using InsightDocs.DotNet.Model;

namespace InsightDocs.DotNet.Abstractions;

public interface IXmlDocProcessor
{
    XmlDocEntry ProcessMemberNode(XmlElement memberNode);
    List<XmlDocCommentComponent> ProcessCommentNode(XmlElement commentNode);
}