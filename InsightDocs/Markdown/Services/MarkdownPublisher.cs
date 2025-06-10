using InsightDocs.Abstractions;
using InsightDocs.Markdown.Abstractions;
using InsightDocs.Markdown.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace InsightDocs.Markdown.Services;

public partial class MarkdownPublisher(
    ILoggerFactory loggerFactory,
    IUrlProvider<MarkdownFile> markdownFileUrlProvider,
    IUrlProvider<MarkdownImage> markdownImageUrlProvider,
    IMarkdownLoader markdownLoader,
    IPublisher publisher,
    IServiceProvider serviceProvider
) : IMarkdownPublisher
{
    [LoggerMessage(LogLevel.Information, "Publishing topics")]
    public static partial void LogPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics")]
    public static partial void LogFinishedPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Debug, "Publishing topic for Markdown file {file}")]
    public static partial void LogPublishingMarkdownFile(ILogger logger, string file);

    protected readonly static Regex ImageTagsRegex = new Regex(@"<img\s+(?<otherAttributes>[^>]*)src\s*=\s*[""'](?<url>[^""']+)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    protected Dictionary<string, string> _imageUrls = new Dictionary<string, string>();

    public virtual async Task PublishTopics(TocItem tocRoot, List<string> markdownFilePaths)
    {
        ILogger logger = loggerFactory.CreateLogger<MarkdownPublisher>();

        LogPublishingTopics(logger);

        foreach (string markdownFilePath in markdownFilePaths)
        {
            LogPublishingMarkdownFile(logger, markdownFilePath);

            MarkdownFile markdownFile = markdownLoader.LoadMarkdownFile(markdownFilePath);
            string url = markdownFileUrlProvider.GetUrl(markdownFile);
            IItemTemplateProvider<MarkdownFile>? markdownTemplateProvider = serviceProvider.GetService<IItemTemplateProvider<MarkdownFile>>();

            markdownFile.Html = await markdownLoader.GetHtml(markdownFile);
            tocRoot.AddTocItem(markdownFile.Title, url);

            string parentDirectory = Path.GetDirectoryName(markdownFilePath)!;
            Dictionary<string, MarkdownImage> markdownImagesToPublish = new Dictionary<string, MarkdownImage>();

            markdownFile.Html = ImageTagsRegex.Replace(markdownFile.Html, match =>
            {
                string imageUrl = match.Groups["url"].Value;
                string otherAttributes = match.Groups["otherAttributes"].Value;
                string imageFilePath = Path.GetFullPath(Path.Combine(parentDirectory, imageUrl));

                if (!_imageUrls.TryGetValue(imageFilePath, out string? existingUrl))
                {
                    MarkdownImage markdownImage = new MarkdownImage(imageFilePath);
                    existingUrl = markdownImageUrlProvider.GetUrl(markdownImage);

                    markdownImagesToPublish[existingUrl] = markdownImage;
                    _imageUrls[imageFilePath] = existingUrl;
                }

                return $"<img src=\"{_imageUrls[imageFilePath]}\" {otherAttributes}";
            });

            foreach (var markdownImage in markdownImagesToPublish)
            {
                byte[] imageContent = await File.ReadAllBytesAsync(markdownImage.Value.FilePath);
                await publisher.Publish(markdownImage.Key, imageContent);
            }

            // TODO: add sub links

            if (markdownTemplateProvider != null)
            {
                byte[] markdownHtml = await markdownTemplateProvider.GetContent(markdownFile);
                await publisher.Publish(url, markdownHtml);
            }

            else
            {
                await publisher.Publish(url, Encoding.UTF8.GetBytes(markdownFile.Html));
            }
        }

        LogFinishedPublishingTopics(logger);
    }
}
