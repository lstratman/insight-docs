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

    protected readonly static Regex HeaderTagsRegex = new Regex(@"<h(?<level>\d+)(?<otherAttributes>[^>]*)id=[""'](?<id>[^""']+)[""'](?<otherAttributes2>[^>]*)>(?<title>.*?)</h\d+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            TocItem markdownFileTocItem = tocRoot.AddTocItem(markdownFile.Title, url);

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

            Stack<Tuple<int, TocItem>> headerStack = new Stack<Tuple<int, TocItem>>([new Tuple<int, TocItem>(1, markdownFileTocItem)]);

            HeaderTagsRegex.Matches(markdownFile.Html).ToList().ForEach(match =>
            {
                string id = match.Groups["id"].Value;
                string title = match.Groups["title"].Value;
                int level = int.Parse(match.Groups["level"].Value);

                if (level == 1)
                {
                    return;
                }

                while (headerStack.Peek().Item1 >= level)
                {
                    headerStack.Pop();
                }

                headerStack.Push(new Tuple<int, TocItem>(level, headerStack.Peek().Item2.AddTocItem(title, url + "#" + id)));
            });

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
