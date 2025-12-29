namespace InsightDocs.Abstractions
{
    public interface IApplicationExitService
    {
        Task Exit();
        void RegisterExitAction(Func<Task> action);
    }
}
