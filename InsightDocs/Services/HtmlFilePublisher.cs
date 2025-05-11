using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs.Services;

public class HtmlFilePublisher(string outputDirectory) : IPublisher
{
    protected string _outputDirectory = outputDirectory;

    public async Task Publish(string url, byte[] contents)
    {
        string outputPath = Path.Combine(_outputDirectory, url);

        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        }

        await File.WriteAllBytesAsync(Path.Combine(_outputDirectory, url), contents);
    }
}

public static class HtmlFilePublisherExtensions
{
    public static Builder PublishToHtmlFiles(this Builder builder, string outputDirectory)
    {
        builder.Services.AddSingleton<IPublisher, HtmlFilePublisher>((provider) => {
            return new HtmlFilePublisher(outputDirectory);
        });

        return builder;
    }
}