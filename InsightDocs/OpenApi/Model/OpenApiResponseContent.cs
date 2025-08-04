using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.OpenApi.Model;

public class OpenApiResponseContent(string mimeType, IOpenApiSchema? schema = null)
{
    public string MimeType
    {
        get;
        set;
    } = mimeType;

    public IOpenApiSchema? Schema
    {
        get;
        set;
    } = schema;
}
