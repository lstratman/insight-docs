using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.Abstractions;

public interface IUrlChecker
{
    void RegisterUrl(string url, string sourceUrl);
    Task CheckUrls();
}
