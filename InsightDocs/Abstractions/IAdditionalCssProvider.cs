namespace InsightDocs.Abstractions;

public interface IAdditionalCssProvider<T>
{
    IAsset[] GetAdditionalCssAssets(T item);
}
