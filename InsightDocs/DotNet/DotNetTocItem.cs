using InsightDocs.Abstractions;
using InsightDocs.DotNet.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;

namespace InsightDocs.DotNet;

public partial class DotNetTocItem : TocItem
{
    protected TocItem _baseTocItem;
    public Dictionary<string, Matcher>? RootedAssemblyGlobMatchers;
    public Dictionary<string, Matcher>? RootedRuntimeAssemblyGlobMatchers;
    public Func<Type, bool>? TypeFilter;

    public DotNetTocItem(TocItem baseTocItem)
    {
        _baseTocItem = baseTocItem;
        RegisterExecutor(DotNetExecutor);
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

    public override void RegisterPostExecutor(Func<IServiceProvider, Task> executor)
    {
        _baseTocItem.RegisterPostExecutor(executor);
    }

    protected async Task DotNetExecutor(IServiceProvider serviceProvider)
    {
        if (RootedAssemblyGlobMatchers != null)
        {
            List<string> assemblyPaths = [];
            List<string> runtimeAssemblyPaths = [];

            foreach (KeyValuePair<string, Matcher> matcherAndRoot in RootedAssemblyGlobMatchers)
            {
                assemblyPaths.AddRange(matcherAndRoot.Value.GetResultsInFullPath(matcherAndRoot.Key));
            }

            if (RootedRuntimeAssemblyGlobMatchers != null)
            {
                foreach (KeyValuePair<string, Matcher> matcherAndRoot in RootedRuntimeAssemblyGlobMatchers)
                {
                    runtimeAssemblyPaths.AddRange(matcherAndRoot.Value.GetResultsInFullPath(matcherAndRoot.Key));
                }
            }

            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                IUrlPrefixProvider prefixProvider = serviceScope.ServiceProvider.GetRequiredService<IUrlPrefixProvider>();
                prefixProvider.UrlPrefix = FullUrlPrefix;

                IDotNetPublisher dotNetPublisher = serviceScope.ServiceProvider.GetRequiredService<IDotNetPublisher>();
                await dotNetPublisher.PublishTopics(this, assemblyPaths, runtimeAssemblyPaths, TypeFilter);
            }
        }
    }
}