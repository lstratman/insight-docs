using InsightDocs.DotNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.DotNet.Abstractions;

public interface IMicrosoftDocsUrlResolver
{
    string GetUrl(DotNetType type);
    string GetUrl(DotNetMethodOverload method);
    string GetUrl(DotNetProperty property);
    string GetUrl(DotNetField field);
    string GetUrl(DotNetNamespace ns);
    bool IsMicrosoftType(DotNetType type);
    bool IsMicrosoftNamespace(string ns);
}
