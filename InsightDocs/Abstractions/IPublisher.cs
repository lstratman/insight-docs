namespace InsightDocs.Abstractions;

public interface IPublisher
{
    Task Initialize();
    Task Publish(string url, byte[] contents);
}