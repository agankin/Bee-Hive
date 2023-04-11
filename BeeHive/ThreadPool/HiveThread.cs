using System.Collections.Concurrent;

namespace BeeHive
{
    internal class HiveThread
    {
        private readonly ConcurrentQueue<Action> _computationsQueue = new();
        private readonly object _schedulingLock;

        public HiveThread(object schedulingLock) => _schedulingLock = schedulingLock;

        public int QueuedCount => _computationsQueue.Count;

        public bool IsRunning { get; private set; }

        public void Load(Action computation) => _computationsQueue.Enqueue(computation);

        public void Run()
        {
            if (IsRunning)
                throw new InvalidOperationException("Hive Thread is already in running state.");

            IsRunning = true;
            Task.Factory.StartNew(QueueHandler, TaskCreationOptions.LongRunning);
        }

        private void QueueHandler()
        {
            while (TryGetNext(out var computation))
            {
                computation?.Invoke();
            }
        }

        private bool TryGetNext(out Action? computation)
        {
            lock (_schedulingLock)
                return IsRunning = _computationsQueue.TryDequeue(out computation);
        }
    }
}