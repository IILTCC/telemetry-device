using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telemetry_device.async_queue
{
    interface IAsyncQueue<T>
    {
        Task EnqueueItem(T item);
        Task<T> DequeueItem();
    }
}
