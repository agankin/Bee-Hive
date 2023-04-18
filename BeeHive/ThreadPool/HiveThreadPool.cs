namespace BeeHive
{
    internal class HiveThreadPool
    {
        private readonly object _schedulingLock = new object();
        private readonly List<HiveThread> _threads = new();

        private readonly ISchedulingStrategy _scheduleStrategy;
        private readonly int _maxParallelCount;

        public HiveThreadPool(ComputationConfiguration configuration)
        {
            _scheduleStrategy = configuration.SchedulingStrategy;
            _maxParallelCount = configuration.MaxParallelExecution;
        }

        public void Load(Action computation)
        {
            lock (_schedulingLock)
                Schedule(computation);
        }

        private void Schedule(Action computation)
        {
            RemoveFinished();

            if (_threads.Count < _maxParallelCount)
                ScheduleNew(computation);
            else
                ScheduleExisting(computation);
        }

        private void ScheduleNew(Action computation)
        {
            var newThread = new HiveThread(_schedulingLock);

            newThread.Load(computation);
            newThread.Run();

            _threads.Add(newThread);
        }

        private void ScheduleExisting(Action computation) => _scheduleStrategy.Schedule(_threads, computation);

        private void RemoveFinished() =>  _threads.RemoveAll(thread => !thread.IsRunning);
    }
}