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
        Stream resourceStream = executingAssembly.GetManifestResourceStream("InsightDocs.Templates.MSDN.Assets.msdn.css") ?? throw new Exception("Unable to load the resource stream for the MSDN template stylesheet.");

        using (MemoryStream memoryStream = new())
        {
            await resourceStream.CopyToAsync(memoryStream);
            await Publisher.Publish("msdn.css", memoryStream.ToArray());
        }

        resourceStream = executingAssembly.GetManifestResourceStream("InsightDocs.Templates.MSDN.Assets.docons.woff2") ?? throw new Exception("Unable to load the resource stream for docons font.");

        using (MemoryStream memoryStream = new())
        {
            await resourceStream.CopyToAsync(memoryStream);
            await Publisher.Publish("docons.woff2", memoryStream.ToArray());
        }

        resourceStream = executingAssembly.GetManifestResourceStream("InsightDocs.Templates.MSDN.Assets.segoe-ui.woff2") ?? throw new Exception("Unable to load the resource stream for Segoe UI font.");

        using (MemoryStream memoryStream = new())
        {
            await resourceStream.CopyToAsync(memoryStream);
            await Publisher.Publish("segoe-ui.woff2", memoryStream.ToArray());
        }

        resourceStream = executingAssembly.GetManifestResourceStream("InsightDocs.Templates.MSDN.Assets.segoe-ui-roman-vf.woff2") ?? throw new Exception("Unable to load the resource stream for Segoe UI Roman VF font.");

        using (MemoryStream memoryStream = new())
        {
            await resourceStream.CopyToAsync(memoryStream);
            await Publisher.Publish("segoe-ui-roman-vf.woff2", memoryStream.ToArray());
        }

        resourceStream = executingAssembly.GetManifestResourceStream("InsightDocs.Templates.MSDN.Assets.highlightjs.highlight.min.js") ?? throw new Exception("Unable to load the resource stream for highlight.js JavaScript.");

        using (MemoryStream memoryStream = new())
        {
            await resourceStream.CopyToAsync(memoryStream);
            await Publisher.Publish("_assets/highlightjs/highlight.min.js", memoryStream.ToArray());
        }

        resourceStream = executingAssembly.GetManifestResourceStream("InsightDocs.Templates.MSDN.Assets.highlightjs.styles.vs.min.css") ?? throw new Exception("Unable to load the resource stream for highlight.js CSS.");

        using (MemoryStream memoryStream = new())
        {
            await resourceStream.CopyToAsync(memoryStream);
            await Publisher.Publish("_assets/highlightjs/styles/vs.min.css", memoryStream.ToArray());
        }
    }
}