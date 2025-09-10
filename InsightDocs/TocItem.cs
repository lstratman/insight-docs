namespace InsightDocs;

public class TocItem
{
    public TocItem()
    {
    }

    public TocItem(string title, TocItem? parent, string? url = null, string? urlPrefix = null)
    {
        Title = title;
        Parent = parent;
        UrlPrefix = urlPrefix;
        Url = url;
    }

    public virtual string? Url
    {
        get;
        set;
    }

    public virtual string Title
    {
        get;
        set;
    } = "";

    public virtual string? UrlPrefix
    {
        get;
        set;
    }

    public string? FullUrlPrefix
    {
        get
        {
            if (Parent != null)
            {
                string? parentUrlPrefix = Parent.FullUrlPrefix;

                if (String.IsNullOrEmpty(UrlPrefix))
                {
                    return parentUrlPrefix;
                }

                else if (!String.IsNullOrEmpty(parentUrlPrefix))
                {
                    return parentUrlPrefix + "/" + UrlPrefix;
                }
            }

            return String.IsNullOrEmpty(UrlPrefix) ? UrlPrefix : "/" + UrlPrefix;
        }
    }

    public virtual TocItem? Parent
    {
        get;
        set;
    }

    public virtual List<TocItem> Children
    {
        get;
        private set;
    } = [];

    public virtual Dictionary<string, string>? AdditionalData
    {
        get;
        set;
    }

    protected virtual List<Func<IServiceProvider, Task>>? Executors
    {
        get;
        private set;
    }

    protected virtual List<Func<IServiceProvider, Task>>? PostExecutors
    {
        get;
        private set;
    }

    public virtual void SortChildren(Comparison<TocItem> comparison, bool recursive = false)
    {
        Children.Sort(comparison);

        if (recursive)
        {
            foreach (TocItem child in Children.Where(c => c.Children.Count > 0))
            {
                child.SortChildren(comparison, recursive);
            }
        }
    }

    public virtual async Task Execute(IServiceProvider serviceProvider)
    {
        if (Executors != null)
        {
            foreach (Func<IServiceProvider, Task> executor in Executors)
            {
                await executor(serviceProvider);
            }
        }

        foreach (TocItem child in Children)
        {
            await child.Execute(serviceProvider);
        }
    }

    public virtual async Task PostExecute(IServiceProvider serviceProvider)
    {
        if (PostExecutors != null)
        {
            foreach (Func<IServiceProvider, Task> executor in PostExecutors)
            {
                await executor(serviceProvider);
            }
        }

        foreach (TocItem child in Children)
        {
            await child.PostExecute(serviceProvider);
        }
    }

    public TocItem AddTocItem(string title, string? url = null, string? urlPrefix = null)
    {
        TocItem newTocItem = new(title, this, url, urlPrefix);
        Children.Add(newTocItem);

        return newTocItem;
    }

    public virtual void RegisterExecutor(Func<IServiceProvider, Task> executor)
    {
        Executors ??= [];
        Executors.Add(executor);
    }

    public virtual void RegisterPostExecutor(Func<IServiceProvider, Task> executor)
    {
        PostExecutors ??= [];
        PostExecutors.Add(executor);
    }

    public virtual void AddData(string key, string value)
    {
        AdditionalData ??= [];
        AdditionalData[key] = value;
    }
}