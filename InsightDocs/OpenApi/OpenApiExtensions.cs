using InsightDocs.Abstractions;
using InsightDocs.OpenApi.Abstractions;
using InsightDocs.OpenApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Text;

namespace InsightDocs.OpenApi;

public static class OpenApiExtensions
{
    public static InsightDocsBuilder UseOpenApi(this InsightDocsBuilder builder)
    {
        builder.Services.AddScoped<IOpenApiLoader, OpenApiLoader>();
        builder.Services.AddScoped<IOpenApiPublisher, OpenApiPublisher>();

        return builder;
    }
}

public static class OpenApiTocItemExtensions
{
    public static TocItem IncludeOpenApiSpecFile(this TocItem tocItem, string openApiSpecFilePath, string? title = null)
    {
        tocItem.RegisterExecutor(async (serviceProvider) =>
        {
            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
                prefixProvider.UrlPrefix = tocItem.FullUrlPrefix;

                IOpenApiPublisher openApiPublisher = serviceScope.ServiceProvider.GetRequiredService<IOpenApiPublisher>();
                await openApiPublisher.PublishTopics(tocItem, openApiSpecFilePath);

                if (!String.IsNullOrEmpty(title))
                {
                    tocItem.Title = title;
                }
            }
        });

        return tocItem;
    }
}

public static class OpenApiModelExtensions
{
    public static string ToJsonSchemaText(this IOpenApiSchema schema)
    {
        return schema.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1).Result;
    }

    public static string ToXmlSchemaText(this IOpenApiSchema schema, string elementName)
    {
        StringBuilder output = new StringBuilder(@"<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">");

        schema.ToXmlSchemaTextInternal(elementName, output, []);
        output.Append("</xs:schema>");

        return output.ToString();
    }

    private static void ToXmlSchemaTextInternal(this IOpenApiSchema schema, string elementName, StringBuilder output, List<string> renderedTypes)
    {
        if (schema.Type == JsonSchemaType.Object)
        {
            if (renderedTypes.Contains(elementName))
            {
                return;
            }

            renderedTypes.Add(elementName);

            output.AppendLine("");
            output.AppendLine($@"  <xs:complexType name=""{elementName}"">");

            Dictionary<string, IOpenApiSchema> deferredSchemas = [];

            if (schema.Properties != null)
            {
                IEnumerable<KeyValuePair<string, IOpenApiSchema>> elementProperties = schema.Properties.Where(p => p.Value.Xml == null || !p.Value.Xml.Attribute);
                IEnumerable<KeyValuePair<string, IOpenApiSchema>> attributeProperties = schema.Properties.Where(p => p.Value.Xml != null && p.Value.Xml.Attribute);

                if (elementProperties.Any())
                {
                    output.AppendLine("    <xs:sequence>");

                    foreach (KeyValuePair<string, IOpenApiSchema> property in elementProperties)
                    {
                        string propertyElementName = property.Value.Xml?.Name ?? property.Key;
                        string propertyType = property.Value.Type == JsonSchemaType.Array
                            ? $"{property.Key}Array"
                            : propertyElementName;

                        output.AppendLine($@"      <xs:element name=""{propertyElementName}"" type=""{propertyType}"" minOccurs=""{(schema.Required != null && schema.Required.Contains(property.Key) ? "1" : "0")}"" maxOccurs=""1""/>");
                        deferredSchemas.Add(propertyElementName, property.Value);
                    }

                    output.AppendLine("    </xs:sequence>");
                }

                if (attributeProperties.Any())
                {
                    foreach (KeyValuePair<string, IOpenApiSchema> property in attributeProperties)
                    {
                        string xsdType = "";

                        if (property.Value.Type == JsonSchemaType.String)
                        {
                            xsdType = "xs:string";
                        }

                        else if (property.Value.Type == JsonSchemaType.Integer)
                        {
                            xsdType = "xs:integer";
                        }

                        else if (property.Value.Type == JsonSchemaType.Number)
                        {
                            xsdType = "xs:decimal";
                        }

                        else if (property.Value.Type == JsonSchemaType.Boolean)
                        {
                            xsdType = "xs:boolean";
                        }

                        else if (property.Value.Type == null)
                        {
                            xsdType = "xs:any";
                        }

                        else
                        {
                            throw new Exception("Unsupported schema type for XML attribute: " + property.Value.Type);
                        }

                        output.AppendLine($@"    <xs:attribute name=""{property.Key}"" type=""{xsdType}""{(schema.Required == null || !schema.Required.Contains(property.Key) ? "" : " use=\"optional\"")}/>");
                    }
                }
            }

            output.AppendLine("  </xs:complexType>");

            if (deferredSchemas.Count > 0)
            {
                foreach (KeyValuePair<string, IOpenApiSchema> deferredSchema in deferredSchemas)
                {
                    deferredSchema.Value.ToXmlSchemaTextInternal(deferredSchema.Key, output, renderedTypes);
                }
            }
        }

        else if (schema.Type == JsonSchemaType.Array)
        {
            if (renderedTypes.Contains($"{elementName}Array"))
            {
                return;
            }

            if (schema.Items == null)
            {
                throw new Exception("Array schema must have items defined.");
            }

            string itemElementName = elementName;

            if (schema.Items.Xml != null && !String.IsNullOrEmpty(schema.Items.Xml.Name))
            {
                itemElementName = schema.Items.Xml.Name;
            }

            output.AppendLine("");
            output.AppendLine($@"  <xs:complexType name=""{elementName}Array"">");
            output.AppendLine("    <xs:sequence>");
            output.AppendLine($@"      <xs:element name=""{itemElementName}"" type=""{itemElementName}"" minOccurs=""1"" maxOccurs=""unbounded""/>");
            output.AppendLine("    </xs:sequence>");
            output.AppendLine("  </xs:complexType>");

            schema.Items.ToXmlSchemaTextInternal(itemElementName, output, renderedTypes);
        }

        else
        {
            output.AppendLine($@"  <xs:element name=""{elementName}"" type=""xs:string""/>");
        }
    }
}