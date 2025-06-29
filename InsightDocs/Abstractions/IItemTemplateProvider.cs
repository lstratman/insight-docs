namespace InsightDocs.Abstractions;

public interface IItemTemplateProvider<T>
{
    Task<string> GetContent(T item);
}