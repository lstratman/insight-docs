using InsightDocs.Abstractions;
using InsightDocs.Markdown.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace InsightDocs.Markdown;

public partial class MarkdownTocItem : TocItem
{
    protected TocItem _baseTocItem;
    public Dictionary<string, Matcher>? RootedMarkdownGlobMatchers;

    public MarkdownTocItem(TocItem baseTocItem)
    {
        _baseTocItem = baseTocItem;
        RegisterExecutor(MarkdownExecutor);
    }

    public override List<TocItem> Children
    {
        get
        {
            return _baseTocItem.Children;
        }
    }

    public override TocItem? Parent
    {
        get
        {
            return _baseTocItem.Parent;
        }

        set
        {
            _baseTocItem.Parent = value;
        }
    }

    public override string Title
    {
        get
        {
            return _baseTocItem.Title;
        }

        set
        {
            _baseTocItem.Title = value;
        }
    }

    public override string? UrlPrefix
    {
        get
        {
            return _baseTocItem.UrlPrefix;
        }

        set
        {
            _baseTocItem.UrlPrefix = value;
        }
    }

    public override void RegisterExecutor(Func<IServiceProvider, Task> executor)
    {
        _baseTocItem.RegisterExecutor(executor);
    }

    protected async Task MarkdownExecutor(IServiceProvider serviceProvider)
    {
        if (RootedMarkdownGlobMatchers != null)
        {
            List<string> markdownFilePaths = [];

            foreach (KeyValuePair<string, Matcher> matcherAndRoot in RootedMarkdownGlobMatchers)
            {
                markdownFilePaths.AddRange(matcherAndRoot.Value.GetResultsInFullPath(matcherAndRoot.Key));
            }

            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
                prefixProvider.UrlPrefix = FullUrlPrefix;

                IMarkdownPublisher markdownPublisher = serviceScope.ServiceProvider.GetRequiredService<IMarkdownPublisher>();
                await markdownPublisher.PublishTopics(this, markdownFilePaths);
            }
        }
    }
}
