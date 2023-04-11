namespace BeeHive
{
    public class HiveComputation<TRequest, TResponse>
    {
        private readonly HiveComputationId _id;
        private readonly Func<TRequest, TResponse> _compute;
        private readonly Action<HiveComputationId, Action> _queueComputation;

        internal HiveComputation(
            HiveComputationId id,
            Func<TRequest, TResponse> compute,
            Action<HiveComputationId, Action> queueComputation)
        {
            _id = id;
            _queueComputation = queueComputation;
            _compute = compute;
        }

        public async Task<TResponse> RequestAsync(TRequest request)
        {
            var completion = new TaskCompletionSource<TResponse>();
            _queueComputation(_id, CreateComputationForRequest(request, completion));

            return await completion.Task;
        }

        private Action CreateComputationForRequest(TRequest request, TaskCompletionSource<TResponse> taskCompletion)
        {
            return () =>
            {
                var response = _compute(request);
                taskCompletion.SetResult(response);
            };
        }
    }
}