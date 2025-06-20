using InsightDocs.Abstractions;
using InsightDocs.DotNet;
using InsightDocs.Markdown.Abstractions;
using InsightDocs.Markdown.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace InsightDocs.Markdown;

public class MarkdownOptions
{
    public bool GetTitleFromHtml
    {
        get;
        set;
    } = true;
}

public static class MarkdownExtensions
{
    public static InsightDocsBuilder UseMarkdown(this InsightDocsBuilder builder, Action<MarkdownOptions>? optionsFactory = null)
    {
        builder.Services.AddScoped<IMarkdownLoader, MarkdownLoader>();
        builder.Services.AddScoped<IMarkdownPublisher, MarkdownPublisher>();

        if (optionsFactory != null)
        {
            builder.Services.AddSingleton((serviceProvider) =>
            {
                MarkdownOptions options = new();
                optionsFactory(options);

                return options;
            });
        }

        else
        {
            builder.Services.AddSingleton(new MarkdownOptions());
        }

        return builder;
    }
}

public static class MarkdownTocItemExtensions
{
    public static TocItem IncludeMarkdownFile(this TocItem parentTocItem, string markdownFilePath, string title = null)
    {
        TocItem tocItem = parentTocItem.AddTocItem("");

        tocItem.RegisterExecutor(async (serviceProvider) =>
        {
            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
                prefixProvider.UrlPrefix = tocItem.FullUrlPrefix;

                IMarkdownPublisher markdownPublisher = serviceScope.ServiceProvider.GetRequiredService<IMarkdownPublisher>();
                await markdownPublisher.PublishTopic(tocItem, markdownFilePath);

                if (!String.IsNullOrEmpty(title))
                {
                    tocItem.Title = title;
                }
            }
        });

        return tocItem;
    }

    public static TocItem IncludeMarkdownFilesInDirectory(this TocItem tocItem, string directoryPath)
    {
        if (tocItem is not MarkdownTocItem markdownTocItem)
        {
            markdownTocItem = new MarkdownTocItem(tocItem);
        }

        if (!Path.IsPathRooted(directoryPath))
        {
            directoryPath = Path.Combine(AppContext.BaseDirectory, directoryPath);
        }

        markdownTocItem.DirectoryPath = directoryPath;

        string root = Path.GetPathRoot(directoryPath)!;

        markdownTocItem.RootedMarkdownGlobMatchers ??= [];

        if (!markdownTocItem.RootedMarkdownGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            markdownTocItem.RootedMarkdownGlobMatchers[root] = matcher;
        }

        matcher.AddInclude(directoryPath[root.Length..] + Path.DirectorySeparatorChar.ToString() + "*.md");

        return markdownTocItem;
    }

    public static TocItem ExcludeMarkdownFiles(this TocItem tocItem, string glob)
    {
        if (tocItem is not MarkdownTocItem markdownTocItem)
        {
            markdownTocItem = new MarkdownTocItem(tocItem);
        }

        if (!Path.IsPathRooted(glob))
        {
            glob = Path.Combine(AppContext.BaseDirectory, glob);
        }

        string root = Path.GetPathRoot(glob)!;

        markdownTocItem.RootedMarkdownGlobMatchers ??= [];

        if (!markdownTocItem.RootedMarkdownGlobMatchers.TryGetValue(root, out Matcher? matcher))
        {
            matcher = new Matcher();
            markdownTocItem.RootedMarkdownGlobMatchers[root] = matcher;
        }

        matcher.AddExclude(glob[root.Length..]);

        return markdownTocItem;
    }
}