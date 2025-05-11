namespace InsightDocs.Abstractions;

public interface IItemTemplateProvider<T>
{
    Task<byte[]> GetContent(T item);
}