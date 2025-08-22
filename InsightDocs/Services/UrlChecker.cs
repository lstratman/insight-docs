using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace InsightDocs.Services;

public partial class UrlChecker(IPublisher publisher, ILoggerFactory loggerFactory, UrlCheckerOptions options) : IUrlChecker
{
    protected readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _urlSources = [];

    [LoggerMessage(LogLevel.Information, "Checking URLs")]
    public static partial void LogCheckingUrls(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Finished checking URLs")]
    public static partial void LogFinishedCheckingUrls(ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Topic for URL {url} does not exist, referenced by {sourceUrls}")]
    public static partial void LogWarningTopicUrlDoesNotExist(ILogger logger, string url, string sourceUrls);

    public async Task CheckUrls()
    {
        ILogger logger = loggerFactory.CreateLogger<UrlChecker>();

        LogCheckingUrls(logger);

        foreach (KeyValuePair<string, ConcurrentDictionary<string, bool>> urlSource in _urlSources)
        {
            string url = urlSource.Key;
            ConcurrentDictionary<string, bool> sources = urlSource.Value;

            if (options.IgnoreUrls != null && options.IgnoreUrls(url))
            {
                continue;
            }

            else if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                // TODO: check external URLs
                continue;
            }

            else if (url.StartsWith("mailto:"))
            {
                continue;
            }

            else
            {
                if (!await publisher.UrlWasPublished(url))
                {
                    LogWarningTopicUrlDoesNotExist(logger, url, String.Join(", ", sources.Keys));
                }
            }
        }

        LogFinishedCheckingUrls(logger);
    }

    public void RegisterUrl(string url, string sourceUrl)
    {
        if (String.IsNullOrEmpty(url))
        {
            return;
        }

        if (url.StartsWith('#'))
        {
            url = sourceUrl + url;
        }

        ConcurrentDictionary<string, bool> sources = _urlSources.GetOrAdd(url, _ => new ConcurrentDictionary<string, bool>());
        sources.GetOrAdd(sourceUrl, true);
    }
}

public class UrlCheckerOptions
{
    public Func<string, bool>? IgnoreUrls
    {
        get;
        set;
    } = null;
}

public static class UrlCheckerExtensions
{
    public static InsightDocsBuilder CheckUrls(this InsightDocsBuilder builder, Action<UrlCheckerOptions> optionsFactory)
    {
        builder.Services.AddSingleton<IUrlChecker, UrlChecker>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                UrlCheckerOptions options = new();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new UrlCheckerOptions());
        }

        return builder;
    }
}