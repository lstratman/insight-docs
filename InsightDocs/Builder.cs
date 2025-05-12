using InsightDocs.Abstractions;
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

    public async Task Execute()
    {
        IServiceProvider serviceProvider = Services.BuildServiceProvider();
        IPublisher publisher = serviceProvider.GetService<IPublisher>() ?? throw new Exception("No IPublisher service was registered.");

        await publisher.Initialize();
        await TocRoot.Execute(serviceProvider);
    }
}