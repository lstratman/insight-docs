namespace InsightDocs.Abstractions;

public interface IPublisher
{
    Task Publish(string url, byte[] contents);
}