using InsightDocs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

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

    [LoggerMessage(LogLevel.Warning, "Topic for URL {url} does not exist (HTTP {httpStatusCode}), referenced by {sourceUrls}")]
    public static partial void LogWarningTopicUrlDoesNotExist(ILogger logger, string url, int httpStatusCode, string sourceUrls);

    [LoggerMessage(LogLevel.Warning, "Unable to download the contents of URL {url}: {message}")]
    public static partial void LogWarningFailedToDownloadUrl(ILogger logger, string url, string message);

    public async Task CheckUrls()
    {
        ILogger logger = loggerFactory.CreateLogger<UrlChecker>();
        Dictionary<string, List<string>> externalUrlReferences = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

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
                if (!externalUrlReferences.TryGetValue(url, out List<string>? sourceList))
                {
                    sourceList = [];
                    externalUrlReferences[url] = sourceList;
                }

                sourceList.AddRange(sources.Keys);
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

        if (options.CheckExternalUrls && externalUrlReferences.Count > 0)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpStatusCode[] redirectStatusCodes =
                [
                    HttpStatusCode.MovedPermanently,
                    HttpStatusCode.Found,
                    HttpStatusCode.SeeOther,
                    HttpStatusCode.TemporaryRedirect,
                    HttpStatusCode.PermanentRedirect
                ];

                ConcurrentDictionary<string, HashSet<string>> anchorCache = new ConcurrentDictionary<string, HashSet<string>>();

                await Parallel.ForEachAsync(externalUrlReferences, async (urlReference, cancellationToken) =>
                {
                    string baseUrl = urlReference.Key;
                    string? anchor = null;
                    int anchorIndex = urlReference.Key.IndexOf('#');

                    if (anchorIndex >= 0)
                    {
                        baseUrl = urlReference.Key[..anchorIndex];
                        anchor = urlReference.Key[(anchorIndex + 1)..];
                    }

                    // HEAD request to check if URL exists
                    try
                    {
                        using (HttpRequestMessage headRequest = new HttpRequestMessage(HttpMethod.Head, baseUrl))
                        using (HttpResponseMessage headResponse = await httpClient.SendAsync(headRequest, cancellationToken))
                        {
                            if (!headResponse.IsSuccessStatusCode && !redirectStatusCodes.Contains(headResponse.StatusCode))
                            {
                                LogWarningTopicUrlDoesNotExist(logger, baseUrl, (int)headResponse.StatusCode, String.Join(", ", urlReference.Value));
                                return;
                            }
                        }
                    }

                    catch (Exception)
                    {
                        LogWarningTopicUrlDoesNotExist(logger, baseUrl, String.Join(", ", urlReference.Value));
                        return;
                    }

                    // If anchor is present, check for it in the document
                    if (anchor != null)
                    {
                        if (!anchorCache.TryGetValue(baseUrl, out HashSet<string>? anchors))
                        {
                            try
                            {
                                string content = await httpClient.GetStringAsync(baseUrl, cancellationToken);
                                anchors = ExtractAnchors(content);
                                anchorCache[baseUrl] = anchors;
                            }

                            catch (Exception ex)
                            {
                                LogWarningFailedToDownloadUrl(logger, baseUrl, ex.Message);
                                return;
                            }
                        }

                        if (!anchors.Contains(anchor))
                        {
                            LogWarningTopicUrlDoesNotExist(logger, baseUrl + "#" + anchor, String.Join(", ", urlReference.Value));
                        }
                    }
                });
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

    private static HashSet<string> ExtractAnchors(string html)
    {
        HashSet<string> anchors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int currentIndex = 0;

        while ((currentIndex = html.IndexOf("id=\"", currentIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            currentIndex += 4;
            int endIndex = html.IndexOf('"', currentIndex);

            if (endIndex > currentIndex)
            {
                string anchor = html[currentIndex..endIndex];
                anchors.Add(anchor);
            }

            currentIndex = endIndex;
        }

        return anchors;
    }
}

public class UrlCheckerOptions
{
    public bool CheckExternalUrls
    {
        get;
        set;
    } = true;

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