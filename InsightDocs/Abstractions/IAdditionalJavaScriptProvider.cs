namespace InsightDocs.Abstractions;

public interface IAdditionalJavaScriptProvider
{
    IAsset[] GetAdditionalJavaScriptAssets();
}

public interface IAdditionalJavaScriptProvider<T>
{
    IAsset[] GetAdditionalJavaScriptAssets(T item);
}
