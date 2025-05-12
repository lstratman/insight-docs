using System.Reflection;
using InsightDocs.Abstractions;

namespace InsightDocs.Templates.MSDN;

public class TemplateAssetProvider(IPublisher publisher) : ITemplateAssetProvider
{
    protected IPublisher Publisher
    {
        get;
        set;
    } = publisher;

    public async Task PublishAssets()
    {
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        Stream stylesheetResourceStream = executingAssembly.GetManifestResourceStream("InsightDocs.Templates.MSDN.Assets.msdn.css") ?? throw new Exception("Unable to load the resource stream for the MSDN template stylesheet.");

        using (MemoryStream memoryStream = new())
        {
            await stylesheetResourceStream.CopyToAsync(memoryStream);
            await Publisher.Publish("msdn.css", memoryStream.ToArray());
        }
    }
}