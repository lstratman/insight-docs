using InsightDocs.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace InsightDocs.Services;

public partial class UrlChecker(IPublisher publisher, ILoggerFactory loggerFactory) : IUrlChecker
{
    protected readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _urlSources = [];

    [LoggerMessage(LogLevel.Warning, "Topic for URL {url} does not exist, referenced by {sourceUrls}")]
    public static partial void LogWarningTopicUrlDoesNotExist(ILogger logger, string url, string sourceUrls);

    public async Task CheckUrls()
    {
        ILogger logger = loggerFactory.CreateLogger<UrlChecker>();

        foreach (KeyValuePair<string, ConcurrentDictionary<string, bool>> urlSource in _urlSources)
        {
            string url = urlSource.Key;
            ConcurrentDictionary<string, bool> sources = urlSource.Value;

            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                // TODO
                continue;
            }

            else
            {
                if (url.Contains("#"))
                {
                    // TODO: verify that anchor exists
                    url = url[..url.IndexOf('#')];
                }

                if (!await publisher.UrlWasPublished(url))
                {
                    LogWarningTopicUrlDoesNotExist(logger, url, String.Join(", ", sources.Keys));
                }
            }
        }
    }

    public void RegisterUrl(string url, string sourceUrl)
    {
        if (String.IsNullOrEmpty(url))
        {
            return;
        }

        if (url.StartsWith("#"))
        {
            url = sourceUrl + url;
        }

        ConcurrentDictionary<string, bool> sources = _urlSources.GetOrAdd(url, _ => new ConcurrentDictionary<string, bool>());
        sources.GetOrAdd(sourceUrl, true);
    }
}
