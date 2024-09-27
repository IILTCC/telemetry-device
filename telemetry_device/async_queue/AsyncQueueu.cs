using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device.async_queue
{
    class AsyncQueueu<T> : IAsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> Queue = new ConcurrentQueue<T>();

        public Task<T> DequeueItem()
        {
            Queue.TryDequeue(out var message);
            return Task.FromResult(message);
        }

        public Task EnqueueItem(T item)
        {
            Queue.Enqueue(item);
            return Task.CompletedTask;

        }
    }
}
