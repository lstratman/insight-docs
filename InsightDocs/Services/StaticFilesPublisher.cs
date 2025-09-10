using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text;

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

    public async Task<byte[]> GetUrlContents(string url)
    {
        return await File.ReadAllBytesAsync(Path.Combine(Options.OutputDirectory, url.StartsWith('/') ? url[1..] : url));
    }

    public Task<string> GetUrlMimeType(string url)
    {
        return Task.FromResult(MimeTypes.GetMimeType(url));
    }

    public virtual Task Initialize()
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

    public virtual async Task Publish(string url, object contents, string mimeType, string? title = null)
    {
        if (PublishedUrls.ContainsKey(url))
        {
            throw new Exception($"The URL {url} has already been published. Each URL must be unique.");
        }

        if (contents is not byte[] contentBytes)
        {
            contentBytes = contents is string contentString
                ? Encoding.UTF8.GetBytes(contentString)
                : throw new ArgumentException("Contents must be a byte array or a string.", nameof(contents));
        }

        PublishedUrls.AddOrUpdate(url, true, (key, oldValue) => true);

        string outputPath = Path.Combine(Options.OutputDirectory, url.StartsWith('/') ? url[1..] : url);

        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        }

        await File.WriteAllBytesAsync(outputPath, contentBytes);
    }

    public virtual void RegisterPublishedAnchor(string url, string anchor)
    {
        string fullUrl = url + "#" + anchor;
        PublishedUrls.AddOrUpdate(fullUrl, true, (key, oldValue) => true);
    }

    public virtual Task<bool> UrlWasPublished(string url)
    {
        return Task.FromResult(PublishedUrls.ContainsKey(url));
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
        builder.Services.AddSingleton<IPublisher, StaticFilesPublisher>((provider) =>
        {
            return new StaticFilesPublisher(options);
        });

        return builder;
    }
}