using InsightDocs.Markdown.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.Markdown.Abstractions;

public interface IMarkdownLoader
{
    MarkdownFile LoadMarkdownFile(string markdownFilePath);
    Task<string> GetHtml(MarkdownFile markdownFile);
}
