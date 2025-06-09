using InsightDocs.Abstractions;
using InsightDocs.Markdown.Abstractions;
using InsightDocs.Markdown.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace InsightDocs.Markdown.Services;

public partial class MarkdownPublisher(
    ILoggerFactory loggerFactory,
    IUrlProvider<MarkdownFile> markdownFileUrlProvider,
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
