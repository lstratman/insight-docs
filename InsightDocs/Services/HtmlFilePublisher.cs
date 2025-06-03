using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Services;

public class HtmlFilePublisher(HtmlFilePublisherOptions options) : IPublisher
{
    protected HtmlFilePublisherOptions Options
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
        string outputPath = Path.Combine(Options.OutputDirectory, url.StartsWith('/') ? url[1..] : url);

        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        }

        await File.WriteAllBytesAsync(outputPath, contents);
    }
}

public class HtmlFilePublisherOptions(string outputDirectory)
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

public static class HtmlFilePublisherExtensions
{
    public static InsightDocsBuilder PublishToHtmlFiles(this InsightDocsBuilder builder, HtmlFilePublisherOptions options)
    {
        builder.Services.AddSingleton<IPublisher, HtmlFilePublisher>((provider) => {
            return new HtmlFilePublisher(options);
        });

        return builder;
    }
}