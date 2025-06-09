using InsightDocs.Abstractions;
using System.Reflection;

namespace InsightDocs;

public class EmbeddedResourceAsset(Assembly assembly, string containerNamespace, string filePath) : IAsset
{
    public string FilePath
    {
        get
        {
            return filePath;
        }
    }

    public string LinkText
    {
        get
        {
            return "";
        }
    }

    public async Task<byte[]> GetContents()
    {
        string resourceName = assembly.GetName().Name + "." + (String.IsNullOrEmpty(containerNamespace) ? "" : containerNamespace + ".") + filePath.Replace("\\", ".").Replace("/", ".");
        Stream resourceStream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Unable to load the resource stream for {resourceName}.");

        using (resourceStream)
        using (MemoryStream memoryStream = new MemoryStream())
        {
            await resourceStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
