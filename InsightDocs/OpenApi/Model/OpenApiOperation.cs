using InsightDocs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.OpenApi.Model;

public class OpenApiOperation : ILinkTarget
{
    public OpenApiOperation(string method, string url, string? description = null)
    {
        Method = method;
        Url = url;
        Description = description;
    }
    public string Method 
    { 
        get; 
        set; 
    }
    
    public string Url 
    { 
        get; 
        set; 
    }
    
    public string? Description 
    { 
        get; 
        set;
    }

    public List<OpenApiParameter>? Parameters 
    { 
        get; 
        set; 
    }

    public string LinkText
    {
        get
        {
            return $"{Method} {Url}";
        }
    }
}
