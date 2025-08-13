using InsightDocs.Abstractions;
using InsightDocs.OpenApi.Abstractions;
using InsightDocs.OpenApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Nodes;

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

    public static string ToXmlSchemaText(this IOpenApiSchema schema, string typeName)
    {
        StringBuilder output = new StringBuilder(@"<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">");

        schema.ToXmlSchemaTextInternal(typeName, output, []);
        output.Append("</xs:schema>");

        return output.ToString();
    }

    private static void ToXmlSchemaTextInternal(this IOpenApiSchema schema, string typeName, StringBuilder output, List<string> renderedTypes)
    {
        if (schema.Type == JsonSchemaType.Object)
        {
            if (renderedTypes.Contains(typeName))
            {
                return;
            }

            renderedTypes.Add(typeName);

            output.AppendLine("");
            output.AppendLine($@"  <xs:complexType name=""{typeName}"">");

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
                        string propertyType;
                        string minOccurs = "0";
                        string maxOccurs = "1";

                        if (property.Value is OpenApiSchemaReference reference)
                        {
                            if (property.Value.Type == JsonSchemaType.Object)
                            {
                                propertyElementName = property.Key;
                            }

                            propertyType = reference.Reference.Id!;
                            minOccurs = schema.Required != null && schema.Required.Contains(property.Key) ? "1" : "0";
                        }

                        else
                        {
                            if (property.Value.Type == JsonSchemaType.Array)
                            {
                                if ((property.Value.Xml == null || !property.Value.Xml.Wrapped) && property.Value.Items is OpenApiSchemaReference itemsReference)
                                {
                                    propertyType = itemsReference.Reference.Id!;
                                    minOccurs = schema.Required != null && schema.Required.Contains(property.Key) ? "1" : "0";
                                    maxOccurs = "unbounded";
                                }

                                else
                                {
                                    propertyType = $"{property.Key}Array";
                                    deferredSchemas.Add(propertyType, property.Value);
                                }
                            }

                            else
                            {
                                propertyType = propertyElementName;
                                minOccurs = schema.Required != null && schema.Required.Contains(property.Key) ? "1" : "0";
                                deferredSchemas.Add(propertyType, property.Value);
                            }
                        }

                        output.Append($@"      <xs:element name=""{propertyElementName}"" type=""{propertyType}"" minOccurs=""{minOccurs}"" maxOccurs=""{maxOccurs}""");

                        if (!String.IsNullOrEmpty(property.Value.Description))
                        {
                            output.AppendLine($">");
                            output.AppendLine("        <xs:annotation>");
                            output.AppendLine($"          <xs:documentation>{property.Value.Description}</xs:documentation>");
                            output.AppendLine("        </xs:annotation>");
                            output.AppendLine("      </xs:element>");
                        }

                        else
                        {
                            output.AppendLine("/>");
                        }
                    }

                    output.AppendLine("    </xs:sequence>");
                }

                if (attributeProperties.Any())
                {
                    foreach (KeyValuePair<string, IOpenApiSchema> property in attributeProperties)
                    {
                        string xsdType = "";

                        if (property.Value is OpenApiSchemaReference schemaReference)
                        {
                            xsdType = schemaReference.Reference.Id!;
                        }

                        else if (property.Value.Type == JsonSchemaType.String)
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

#pragma warning disable IDE0045 // Convert to conditional expression
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
#pragma warning restore IDE0045 // Convert to conditional expression

                        output.Append($@"    <xs:attribute name=""{property.Key}"" type=""{xsdType}""{(schema.Required != null && schema.Required.Contains(property.Key) ? " use=\"required\"" : "")}");

                        if (property.Value.Default != null)
                        {
                            output.Append($" default=\"{property.Value.Default.ToString()}\"");
                        }

                        if (!String.IsNullOrEmpty(property.Value.Description))
                        {
                            output.AppendLine($">");
                            output.AppendLine("      <xs:annotation>");
                            output.AppendLine($"        <xs:documentation>{property.Value.Description}</xs:documentation>");
                            output.AppendLine("      </xs:annotation>");
                            output.AppendLine("    </xs:attribute>");
                        }

                        else
                        {
                            output.AppendLine("/>");
                        }
                    }
                }

                if (schema.AdditionalProperties != null && schema.AdditionalProperties.Xml != null && schema.AdditionalProperties.Xml.Attribute)
                {
                    output.AppendLine("    <xs:anyAttribute processContents=\"lax\" namespace=\"##other\"/>");
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
            if (renderedTypes.Contains($"{typeName}Array"))
            {
                return;
            }

            if (schema.Items == null)
            {
                throw new Exception("Array schema must have items defined.");
            }

            string itemElementName = typeName;

            if (schema.Items.Xml != null && !String.IsNullOrEmpty(schema.Items.Xml.Name))
            {
                itemElementName = schema.Items.Xml.Name;
            }

            string itemType = schema.Items is OpenApiSchemaReference reference ? reference.Reference.Id! : itemElementName;

            output.AppendLine("");
            output.AppendLine($@"  <xs:complexType name=""{typeName}"">");
            output.AppendLine("    <xs:sequence>");
            output.AppendLine($@"      <xs:element name=""{itemElementName}"" type=""{itemType}"" minOccurs=""{(schema.MinItems == null ? "1" : schema.MinItems.ToString())}"" maxOccurs=""{(schema.MaxItems == null ? "unbounded" : schema.MaxItems.ToString())}""/>");
            output.AppendLine("    </xs:sequence>");
            output.AppendLine("  </xs:complexType>");

            if (schema.Items is not OpenApiSchemaReference)
            {
                schema.Items.ToXmlSchemaTextInternal(itemElementName, output, renderedTypes);
            }
        }

        else if (schema.Enum != null)
        {
            if (renderedTypes.Contains(typeName))
            {
                return;
            }

            renderedTypes.Add(typeName);

            output.AppendLine("");
            output.AppendLine($@"  <xs:simpleType name=""{typeName}"">");
            output.AppendLine($"    <xs:restriction base=\"{(schema.Type == JsonSchemaType.Integer ? "xs:integer" : "xs:string")}\">");

            foreach (JsonNode enumValue in schema.Enum)
            {
                string escapedValue = enumValue.ToString();
                output.AppendLine($@"      <xs:enumeration value=""{escapedValue}""/>");
            }

            output.AppendLine("    </xs:restriction>");
            output.AppendLine("  </xs:simpleType>");
        }

        else
        {
            output.AppendLine($@"  <xs:element name=""{typeName}"" type=""xs:string""/>");
        }
    }
}