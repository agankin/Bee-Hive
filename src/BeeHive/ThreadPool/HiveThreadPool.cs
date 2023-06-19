namespace BeeHive
{
    internal class HiveThreadPool
    {
        private readonly object _schedulingLock = new object();
        private readonly ConcurrentSet<HiveThread> _threads = new();

        private readonly int _minLiveThreads;
        private readonly int _maxParallelCount;
        private readonly ISchedulingStrategy _scheduleStrategy;

        public HiveThreadPool(ComputationConfiguration configuration)
        {
            _minLiveThreads = configuration.MinLiveThreads;
            _maxParallelCount = configuration.MaxParallelExecution;
            _scheduleStrategy = configuration.SchedulingStrategy;
        }

        public void Load(Action computation)
        {
            lock (_schedulingLock)
                Schedule(computation);
        }

        private void Schedule(Action computation)
        {
            if (_threads.Count < _maxParallelCount)
                ScheduleNew(computation);
            else
                ScheduleExisting(computation);
        }

        private void ScheduleNew(Action computation)
        {
            var newThread = new HiveThread(_schedulingLock, OnThreadFinishing);

            newThread.Load(computation);
            newThread.Run();

            _threads.Add(newThread);
        }

        private void ScheduleExisting(Action computation) =>
            _scheduleStrategy.Schedule(_threads.ToList(), computation);

        private bool OnThreadFinishing(HiveThread finishedThread)
        {
            lock (_schedulingLock)
            {
                var canFinish = CanThreadFinish();

                if (canFinish)
                    _threads.Remove(finishedThread);

                return canFinish;
            }
        }

        private bool CanThreadFinish() => _threads.Count > _minLiveThreads;
    }
}