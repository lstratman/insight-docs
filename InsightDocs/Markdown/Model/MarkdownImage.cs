using InsightDocs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.Markdown.Model;

public class MarkdownImage(string filePath) : ILinkTarget
{
    public string FilePath
    {
        get;
        set;
    } = filePath;

    public string LinkText
    {
        get
        {
            return "";
        }
    }
}
