using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Services;

public partial class TypeScriptLoader(ILoggerFactory loggerFactory) : ITypeScriptLoader
{
    [LoggerMessage(LogLevel.Information, "Loading {jsonFile}")]
    public static partial void LogJsonLoad(ILogger logger, string jsonFile);

    [LoggerMessage(LogLevel.Information, "Finished loading {jsonFile}")]
    public static partial void LogFinishedJsonLoad(ILogger logger, string jsonFile);

    public async Task<TypeScriptProject> LoadApiJson(string filename)
    {
        ILogger logger = loggerFactory.CreateLogger<TypeScriptLoader>();

        LogJsonLoad(logger, filename);

        string fileText = await File.ReadAllTextAsync(filename);
        TypeScriptProject api = JsonConvert.DeserializeObject<TypeScriptProject>(fileText) ?? throw new Exception($"Failed to deserialize TypeScript project from {filename}.");

        if (api.Types != null)
        {
            foreach (TypeScriptTypeDeclaration type in api.Types.Values)
            {
                ReferenceType.AllTypes[type.Id] = type;
                ReferenceType.AllTypesByName[type.FullName] = type;

                type.Project = api;

                if (type is TypeScriptNamespacedTypeDeclaration typeScriptNamespacedType && type.FullName.Contains('.'))
                {
                    api.Namespaces ??= [];

                    if (!api.Namespaces.TryGetValue(type.FullName[..type.FullName.LastIndexOf('.')], out TypeScriptNamespace? ns))
                    {
                        ns = new TypeScriptNamespace
                        {
                            Name = type.FullName[..type.FullName.LastIndexOf('.')]
                        };

                        api.Namespaces[type.FullName[..type.FullName.LastIndexOf('.')]] = ns;
                    }

                    ns.Types.Add(type);
                    typeScriptNamespacedType.Namespace = ns;
                }

                // TODO: remove prototype property
            }

            foreach (TypeScriptTypeDeclaration type in api.Types.Values)
            {
                type.Normalize();
            }
        }

        if (api.Modules != null)
        {
            foreach (TypeScriptModule module in api.Modules.Values)
            {
                if (module.ExportType != null && module.ExportType is ReferenceType exportTypeReference)
                {
                    if (api.Types != null)
                    {
                        TypeScriptInterface exportedInterface = (TypeScriptInterface)ReferenceType.AllTypes[exportTypeReference.Target];
                        
                        module.Exports = exportedInterface;
                        exportedInterface.ExportedFromModule = module;
                    }
                }
            }
        }

        LogFinishedJsonLoad(logger, filename);

        return api;
    }
}
