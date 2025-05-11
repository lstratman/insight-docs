using Microsoft.Extensions.DependencyInjection;

namespace InsightDocs;

public class Builder(IServiceCollection services)
{
    public TocItem TocRoot
    {
        get;
        private set;
    } = new TocItem("", null);

    public IServiceCollection Services
    {
        get;
        private set;
    } = services;

    public TocItem AddTocItem(string title, string? urlPrefix = null)
    {
        return TocRoot.AddTocItem(title, urlPrefix);
    }

    public Task Execute()
    {
        IServiceProvider serviceProvider = Services.BuildServiceProvider();
        return TocRoot.Execute(serviceProvider);
    }
}