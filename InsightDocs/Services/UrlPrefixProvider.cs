using InsightDocs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.Services;

public class UrlPrefixProvider : IUrlPrefixProvider
{
    public string? UrlPrefix
    {
        get;
        set;
    }
}
