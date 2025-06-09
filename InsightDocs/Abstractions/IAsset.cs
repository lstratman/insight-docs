using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.Abstractions;

public interface IAsset : ILinkTarget
{
    string FilePath
    {
        get;
    }

    Task<byte[]> GetContents();
}
