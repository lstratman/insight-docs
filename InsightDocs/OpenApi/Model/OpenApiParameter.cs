using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsightDocs.OpenApi.Model;

public class OpenApiParameter
{
    public OpenApiParameter(string name, string? description, OpenApiParameterLocation location, bool isRequired, string type)
    {
        Name = name;
        Description = description;
        Location = location;
        IsRequired = isRequired;
        Type = type;
    }

    public string Name
    {
        get;
        set;
    }

    public string? Description
    {
        get;
        set;
    }

    public OpenApiParameterLocation Location
    {
        get;
        set;
    }

    public bool IsRequired
    {
        get;
        set;
    }

    public string Type
    {
        get;
        set;
    }
}

public enum OpenApiParameterLocation
{
    Query,
    Header,
    Path,
    Cookie,
    Form
}
