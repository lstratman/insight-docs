namespace InsightDocs.Abstractions;

public interface IAdditionalCssProvider
{
    IAsset[] GetAdditionalCssAssets();
}

public interface IAdditionalCssProvider<T>
{
    IAsset[] GetAdditionalCssAssets(T item);
}
