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
    public static TocItem IncludeMarkdownTocFile(this TocItem tocItem, string filePath)
    {
        // TODO
        throw new NotImplementedException();
    }

    public static TocItem IncludeMarkdownFiles(this TocItem tocItem, string glob)
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

        matcher.AddInclude(glob[root.Length..]);

        return markdownTocItem;
    }
}