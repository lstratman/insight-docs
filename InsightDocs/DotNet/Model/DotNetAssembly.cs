using System.Reflection;
using System.Xml;
using InsightDocs.DotNet.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.DotNet.Model;

public class DotNetAssembly
{
    private readonly static Dictionary<string, DotNetAssembly> AssemblyCache = [];

    public static DotNetAssembly Resolve(Assembly assembly, IServiceProvider serviceProvider)
    {
        string key = assembly.Location;

        if (!AssemblyCache.TryGetValue(key, out DotNetAssembly? assemblyMetadata))
        {
            assemblyMetadata = new DotNetAssembly(assembly, serviceProvider);
        }

        return assemblyMetadata;
    }

    protected DotNetAssembly(Assembly assembly, IServiceProvider serviceProvider)
    {
        AssemblyName assemblyName = assembly.GetName();

        Name = assemblyName.Name;
        FullName = assemblyName.FullName;

        string? assemblyDirectory = Path.GetDirectoryName(assembly.Location);

        if (!String.IsNullOrEmpty(assemblyDirectory) && !String.IsNullOrEmpty(assemblyName.Name) && File.Exists(Path.Combine(assemblyDirectory, assemblyName.Name + ".xml")))
        {
            XmlDocument xmlDocDocument = new();
            // TODO: move >> replacement to custom
            string xmlDocText = File.ReadAllText(Path.Combine(assemblyDirectory, assemblyName.Name + ".xml")).Replace(">>", ">");
            
            xmlDocDocument.LoadXml(xmlDocText);

            XmlNodeList? memberNodes = xmlDocDocument.SelectNodes("/doc/members/member");

            if (memberNodes != null)
            {
                XmlDocEntries = [];

                IXmlDocProcessor xmlDocProcessor = serviceProvider.GetRequiredService<IXmlDocProcessor>();

                foreach (XmlElement memberNode in memberNodes)
                {
                    XmlDocEntry xmlDocEntry = xmlDocProcessor.ProcessMemberNode(memberNode);
                    XmlDocEntries[xmlDocEntry.Key] = xmlDocEntry;
                }
            }
        }
    }

    public string? Name
    {
        get;
        set;
    }

    public string FullName
    {
        get;
        set;
    }

    public Dictionary<string, XmlDocEntry>? XmlDocEntries
    {
        get;
        set;
    }
}