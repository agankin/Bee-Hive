# Bee Hive

This is a library for creating a dedicated thread pool for parallel computations.

### Features

- Works with explicit queues of computations and sets of results.
- Supports representation of queued computations as tasks.
- Can run synchronous and asynchronous computations.
- Has possibility of cancelling a pending or a progressing computation in a queue.
- Can dynamicly add new threads when having extra pending computations in a queue.
- Can dynamicly remove idle threads after configurable time without work passes.
- Has configurable min and max threads count and time for idle thread to be removed.