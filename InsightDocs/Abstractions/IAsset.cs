namespace InsightDocs.Abstractions;

public interface IAsset : ILinkTarget
{
    string FilePath
    {
        get;
    }

    string MimeType
    {
        get;
    }

    Task<byte[]> GetContents();
}
