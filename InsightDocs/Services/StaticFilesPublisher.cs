using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace InsightDocs.Services;

public class StaticFilesPublisher(StaticFilesPublisherOptions options) : IPublisher
{
    protected ConcurrentDictionary<string, bool> PublishedUrls 
    { 
        get; 
    } = new ConcurrentDictionary<string, bool>();

    protected StaticFilesPublisherOptions Options
    {
        get;
        set;
    } = options;

    public Task Initialize()
    {
        if (!Directory.Exists(Options.OutputDirectory))
        {
            Directory.CreateDirectory(Options.OutputDirectory);
        }

        if (Options.CleanOutputDirectory)
        {
            foreach (string filePath in Directory.GetFiles(Options.OutputDirectory))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            foreach (string directoryPath in Directory.GetDirectories(Options.OutputDirectory))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        return Task.CompletedTask;
    }

    public async Task Publish(string url, byte[] contents)
    {
        if (PublishedUrls.ContainsKey(url))
        {
            throw new Exception($"The URL {url} has already been published. Each URL must be unique.");
        }

        PublishedUrls.AddOrUpdate(url, true, (key, oldValue) => true);

        string outputPath = Path.Combine(Options.OutputDirectory, url.StartsWith('/') ? url[1..] : url);

        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        }

        await File.WriteAllBytesAsync(outputPath, contents);
    }
}

public class StaticFilesPublisherOptions(string outputDirectory)
{
    public string OutputDirectory
    {
        get;
        set;
    } = outputDirectory;

    public bool CleanOutputDirectory
    {
        get;
        set;
    }
}

public static class StaticFilesPublisherExtensions
{
    public static InsightDocsBuilder PublishToStaticFiles(this InsightDocsBuilder builder, StaticFilesPublisherOptions options)
    {
        builder.Services.AddSingleton<IPublisher, StaticFilesPublisher>((provider) => {
            return new StaticFilesPublisher(options);
        });

        return builder;
    }
}