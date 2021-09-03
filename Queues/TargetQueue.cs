using System.Threading.Tasks;
using AzureQueuedMessageMover.Interfaces;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Queues
{
    public class TargetQueue : ITargetQueue
    {
        private readonly CloudQueue _queue;

        public TargetQueue(CloudQueue queue)
        {
            _queue = queue;
        }

        public Task AddMessageAsync(CloudQueueMessage cloudQueueMessage)
        {
            return _queue.AddMessageAsync(cloudQueueMessage);
        }
    }
}