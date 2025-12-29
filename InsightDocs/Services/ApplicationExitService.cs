using InsightDocs.Abstractions;

namespace InsightDocs.Services
{
    public class ApplicationExitService : IApplicationExitService
    {
        private readonly List<Func<Task>> _exitActions = [];

        public async Task Exit()
        {
            foreach (Func<Task> action in _exitActions)
            {
                await action();
            }
        }

        public void RegisterExitAction(Func<Task> action)
        {
            _exitActions.Add(action);
        }
    }
}
