using InsightDocs.TypeScript.Abstractions;
using InsightDocs.TypeScript.Model;
using InsightDocs.TypeScript.Model.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace InsightDocs.TypeScript.Services;

public partial class TypeScriptLoader(ILoggerFactory loggerFactory) : ITypeScriptLoader
{
    [LoggerMessage(LogLevel.Information, "Loading {definitionFilePath}")]
    public static partial void LogDefinitionFileLoad(ILogger logger, string definitionFilePath);

    [LoggerMessage(LogLevel.Information, "Finished loading definition files")]
    public static partial void LogFinishedDefinitionFilesLoad(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Extracting definition file processor to {destination}")]
    public static partial void LogExtractingDefinitionFileProcessor(ILogger logger, string destination);

    [LoggerMessage(LogLevel.Information, "Finished extracting definition file processor")]
    public static partial void LogFinishedExtractingDefinitionFileProcessor(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Running definition file processor")]
    public static partial void LogRunningDefinitionFileProcessor(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished running definition file processor")]
    public static partial void LogFinishedRunningDefinitionFileProcessor(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Reading output from definition file processor from {outputJson}")]
    public static partial void LogReadingDefinitionFileProcessorOutput(ILogger logger, string outputJson);

    [LoggerMessage(LogLevel.Information, "Finished reading output from definition file processor")]
    public static partial void LogFinishedReadingDefinitionFileProcessorOutput(ILogger logger);

    private static bool DefinitionFileProcessorExtracted = false;

    private static string? DefinitionFileProcessorPath = null;

    protected async Task ExtractDefinitionFileProcessor(ILogger logger)
    {
        if (DefinitionFileProcessorExtracted)
        {
            return;
        }

        string outputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(outputDirectory);

        LogExtractingDefinitionFileProcessor(logger, outputDirectory);

        foreach (string resourceName in GetType().Assembly.GetManifestResourceNames())
        {
            if (resourceName.StartsWith("InsightDocs.TypeScript.Resources."))
            {
                using (Stream resourceStream = GetType().Assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Failed to find embedded resource {resourceName}."))
                {
                    string outputFilePath = Path.Combine(outputDirectory, resourceName[33..]);

                    using (FileStream fileStream = new(outputFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await resourceStream.CopyToAsync(fileStream);
                    }
                }
            }
        }

        DefinitionFileProcessorPath = Path.Combine(outputDirectory, "get-typescript-api-json.js");
        DefinitionFileProcessorExtracted = true;

        LogFinishedExtractingDefinitionFileProcessor(logger);
    }

    public static string FindCommonRoot(List<string> filePaths)
    {
        if (filePaths == null || filePaths.Count == 0)
        {
            return String.Empty;
        }

        string[][] separated = [.. filePaths.Select(path => Path.GetFullPath(path).Split(Path.DirectorySeparatorChar))];
        int minLength = separated.Min(s => s.Length - 1);
        List<string> commonSegments = [];

        for (int i = 0; i < minLength; i++)
        {
            // Take the segment from the first path
            string segment = separated[0][i];

            // Check if all paths have the same segment at this position
            if (separated.All(s => s[i] == segment))
            {
                commonSegments.Add(segment);
            }

            else
            {
                break;
            }
        }

        return commonSegments.Count == 0 ? String.Empty : String.Join(Path.DirectorySeparatorChar.ToString(), commonSegments);
    }

    public async Task<TypeScriptProject> LoadDefinitionFiles(List<string> definitionFilePaths, bool excludePackageRoot)
    {
        ILogger logger = loggerFactory.CreateLogger<TypeScriptLoader>();

        foreach (string definitionFilePath in definitionFilePaths)
        {
            LogDefinitionFileLoad(logger, definitionFilePath);
        }

        await ExtractDefinitionFileProcessor(logger);

        string outputJson = Path.GetTempFileName();
        string commonRootDirectory = FindCommonRoot(definitionFilePaths);
        string processorArguments = $"{DefinitionFileProcessorPath} --rootDirectory \"{commonRootDirectory}\" --outputFile \"{outputJson}\" --inputFiles";

        foreach (string definitionFilePath in definitionFilePaths)
        {
            processorArguments += $" \"{definitionFilePath[(commonRootDirectory.Length + 1)..]}\"";
        }

        if (excludePackageRoot)
        {
            processorArguments += " --excludePackageRoot";
        }

        TypeScriptProject api;

        try
        {
            LogRunningDefinitionFileProcessor(logger);

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = processorArguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Definition file processor exited with code {process.ExitCode}.");
            }

            LogFinishedRunningDefinitionFileProcessor(logger);
            LogReadingDefinitionFileProcessorOutput(logger, outputJson);

            string fileText = await File.ReadAllTextAsync(outputJson);
            api = JsonConvert.DeserializeObject<TypeScriptProject>(fileText) ?? throw new Exception($"Failed to deserialize TypeScript project from {outputJson}.");

            LogFinishedReadingDefinitionFileProcessorOutput(logger);
        }

        finally
        {
            File.Delete(outputJson);
        }

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
                if (module.ExportType is not null and ReferenceType exportTypeReference)
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

        LogFinishedDefinitionFilesLoad(logger);

        return api;
    }
}
