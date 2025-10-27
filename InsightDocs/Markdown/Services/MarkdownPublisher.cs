using InsightDocs.Abstractions;
using InsightDocs.Markdown.Abstractions;
using InsightDocs.Markdown.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
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
    IUrlPrefixProvider urlPrefixProvider,
    IServiceProvider serviceProvider,
    MarkdownOptions markdownOptions,
    IUrlChecker urlChecker
) : IMarkdownPublisher
{
    [LoggerMessage(LogLevel.Information, "Publishing topics")]
    public static partial void LogPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics")]
    public static partial void LogFinishedPublishingTopics(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Publishing topics for {prefix}")]
    public static partial void LogPublishingTopicsForPrefix(ILogger logger, string prefix);

    [LoggerMessage(LogLevel.Information, "Finished publishing topics for {prefix}")]
    public static partial void LogFinishedPublishingTopicsForPrefix(ILogger logger, string prefix);

    [LoggerMessage(LogLevel.Debug, "Publishing topic for Markdown file {file}")]
    public static partial void LogPublishingMarkdownFile(ILogger logger, string file);

    protected static readonly Regex ImageTagsRegex = new Regex(@"<img\s+(?<otherAttributes>[^>]*)src\s*=\s*[""'](?<url>[^""']+)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex HeaderTagsRegex = new Regex(@"<h(?<level>\d+)(?<otherAttributes>[^>]*)id=[""'](?<id>[^""']+)[""'](?<otherAttributes2>[^>]*)>(?<title>.*?)</h\d+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex HeaderTitleRegex = new Regex(@"<h1(?<otherAttributes>[^>]*)>(?<title>.*?)</h1>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    protected static readonly Regex LinkMatcher = new Regex(@"<a href\s*=\s*(['""])(?<url>.*?)\1");
    protected Dictionary<string, string> _imageUrls = [];

    public virtual async Task PublishTopic(TocItem tocItem, string markdownFilePath)
    {
        ILogger logger = loggerFactory.CreateLogger<MarkdownPublisher>();
        await ProcessTopic(null, tocItem, markdownFilePath, logger);
    }

    public virtual async Task PublishTopics(TocItem tocRoot, string rootDirectoryPath, IEnumerable<string>? exclusionGlobs = null)
    {
        ILogger logger = loggerFactory.CreateLogger<MarkdownPublisher>();

        if (urlPrefixProvider.UrlPrefix != null)
        {
            LogPublishingTopicsForPrefix(logger, urlPrefixProvider.UrlPrefix);
        }

        else
        {
            LogPublishingTopics(logger);
        }

        List<string> markdownFilePaths = [];

        if (!File.Exists(Path.Join(rootDirectoryPath, ".order")))
        {
            Matcher matcher = new Matcher();

            matcher.AddInclude("*.md");

            if (exclusionGlobs != null)
            {
                foreach (string exclusionGlob in exclusionGlobs)
                {
                    matcher.AddExclude(exclusionGlob);
                }
            }

            markdownFilePaths.AddRange(matcher.GetResultsInFullPath(rootDirectoryPath));
        }

        else
        {
            string[] orderFileLines = await File.ReadAllLinesAsync(Path.Join(rootDirectoryPath, ".order"));

            foreach (string line in orderFileLines)
            {
                string trimmedLine = line.Trim();

                if (!trimmedLine.EndsWith(".md"))
                {
                    trimmedLine += ".md";
                }

                if (String.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                {
                    continue;
                }

                markdownFilePaths.Add(Path.GetFullPath(Path.Combine(rootDirectoryPath, trimmedLine)));
            }
        }

        foreach (string markdownFilePath in markdownFilePaths)
        {
            await ProcessTopic(tocRoot, null, markdownFilePath, logger);
        }

        if (urlPrefixProvider.UrlPrefix != null)
        {
            LogFinishedPublishingTopicsForPrefix(logger, urlPrefixProvider.UrlPrefix);
        }

        else
        {
            LogFinishedPublishingTopics(logger);
        }
    }

    protected virtual async Task<MarkdownFile> ProcessTopic(TocItem? tocRoot, TocItem? tocItem, string markdownFilePath, ILogger logger)
    {
        LogPublishingMarkdownFile(logger, markdownFilePath);

        MarkdownFile markdownFile = markdownLoader.LoadMarkdownFile(markdownFilePath);
        IItemTemplateProvider<MarkdownFile>? markdownTemplateProvider = serviceProvider.GetService<IItemTemplateProvider<MarkdownFile>>();

        markdownFile.Html = await markdownLoader.GetHtml(markdownFile);

        if (markdownOptions.GetTitleFromHtml)
        {
            Match headerTitleMatch = HeaderTitleRegex.Match(markdownFile.Html);

            if (headerTitleMatch.Success)
            {
                markdownFile.Title = headerTitleMatch.Groups["title"].Value;
            }
        }

        string url = markdownFileUrlProvider.GetUrl(markdownFile);

        if (!url.StartsWith('/'))
        {
            url = "/" + url;
        }

        if (tocItem == null)
        {
            tocItem = tocRoot!.AddTocItem(markdownFile.Title, url);
        }

        else
        {
            tocItem.Title = markdownFile.Title;
            tocItem.Url = url;
        }

        string parentDirectory = Path.GetDirectoryName(markdownFilePath)!;
        Dictionary<string, MarkdownImage> markdownImagesToPublish = [];

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

        foreach (KeyValuePair<string, MarkdownImage> markdownImage in markdownImagesToPublish)
        {
            byte[] imageContent = await File.ReadAllBytesAsync(markdownImage.Value.FilePath);
            await publisher.Publish(markdownImage.Key, imageContent, MimeTypes.GetMimeType(markdownImage.Value.FilePath), null);
        }

        Stack<Tuple<int, TocItem>> headerStack = new Stack<Tuple<int, TocItem>>([new Tuple<int, TocItem>(1, tocItem)]);

        HeaderTagsRegex.Matches(markdownFile.Html).ToList().ForEach(match =>
        {
            string id = match.Groups["id"].Value;
            string title = match.Groups["title"].Value;
            int level = Int32.Parse(match.Groups["level"].Value);

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

        LinkMatcher.Matches(markdownFile.Html).ToList().ForEach(match =>
        {
            urlChecker.RegisterUrl(match.Groups["url"].Value, url);
        });

        if (markdownTemplateProvider != null)
        {
            markdownFile.Html = await markdownTemplateProvider.GetContent(markdownFile, url);
            await publisher.Publish(url, markdownFile.Html, "text/html", markdownFile.Title);
        }

        else
        {
            await publisher.Publish(url, Encoding.UTF8.GetBytes(markdownFile.Html), "text/html", markdownFile.Title);
        }

        return markdownFile;
    }
}
