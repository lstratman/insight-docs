using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;
using Newtonsoft.Json;

namespace InsightDocs.TypeScript.Services;

public class TypeScriptLoader : ITypeScriptLoader
{
    public async Task<TypeScriptProject> LoadApiJson(string filename, Func<TypeScriptTypeDeclaration, bool>? typeFilter, Func<TypeScriptModule, bool>? moduleFilter)
    {
        string fileText = await File.ReadAllTextAsync(filename);
        TypeScriptProject api = JsonConvert.DeserializeObject<TypeScriptProject>(fileText) ?? throw new Exception($"Failed to deserialize TypeScript project from {filename}.");

        if (api.Types != null)
        {
            if (typeFilter != null)
            {
                api.Types = api.Types.Values.Where(typeFilter).ToDictionary(t => t.FullName);
            }

            foreach (TypeScriptTypeDeclaration type in api.Types.Values)
            {
                ReferenceType.AllTypes[type.Id] = type;
                ReferenceType.AllTypesByName[type.FullName] = type;

                type.Project = api;

                if (type is TypeScriptInterface typeScriptInterface && type.FullName.Contains('.'))
                {
                    api.Namespaces ??= new Dictionary<string, TypeScriptNamespace>();

                    if (!api.Namespaces.TryGetValue(type.FullName[..type.FullName.LastIndexOf('.')], out TypeScriptNamespace? ns))
                    {
                        ns = new TypeScriptNamespace
                        {
                            Name = type.FullName[..type.FullName.LastIndexOf('.')]
                        };

                        api.Namespaces[type.FullName[..type.FullName.LastIndexOf('.')]] = ns;
                    }

                    ns.Types.Add(type);
                    typeScriptInterface.Namespace = ns;
                }
            }

            foreach (TypeScriptTypeDeclaration type in api.Types.Values)
            {
                type.Normalize();
            }
        }

        if (api.Modules != null)
        {
            if (moduleFilter != null)
            {
                api.Modules = api.Modules.Values.Where(moduleFilter).ToDictionary(m => m.Name);
            }

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

        return api;
    }
}
