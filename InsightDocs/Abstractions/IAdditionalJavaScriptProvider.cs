namespace InsightDocs.Abstractions;

public interface IAdditionalJavaScriptProvider<T>
{
    IAsset[] GetAdditionalJavaScriptAssets(T item);
}
