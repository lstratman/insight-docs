namespace InsightDocs;

public class TocItem
{
    public TocItem()
    {
    }

    public TocItem(string title, TocItem? parent, string? urlPrefix = null)
    {
        Title = title;
        Parent = parent;
        UrlPrefix = urlPrefix;
    }

    public virtual string? Title
    {
        get;
        set;
    }

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

            return UrlPrefix;
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

    protected virtual List<Func<IServiceProvider, Task>> Executors
    {
        get;
        private set;
    } = [];

    public virtual async Task Execute(IServiceProvider serviceProvider)
    {
        foreach (Func<IServiceProvider, Task> executor in Executors)
        {
            await executor(serviceProvider);
        }

        foreach (TocItem child in Children)
        {
            await child.Execute(serviceProvider);
        }
    }

    public TocItem AddTocItem(string title, string? urlPrefix = null)
    {
        TocItem newTocItem = new(title, this, urlPrefix);
        Children.Add(newTocItem);

        return newTocItem;
    }

    public virtual void RegisterExecutor(Func<IServiceProvider, Task> executor)
    {
        Executors.Add(executor);
    }
}