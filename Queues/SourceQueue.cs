using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureQueuedMessageMover.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Queues
{
    public class SourceQueue : ISourceQueue
    {
        private readonly CloudQueue _queue;

        public SourceQueue(CloudQueue queue)
        {
            _queue = queue;
        }

        public Task DeleteMessageAsync(CloudQueueMessage cloudQueueMessage)
        {
            return _queue.DeleteMessageAsync(cloudQueueMessage);
        }

        public Task<IEnumerable<CloudQueueMessage>> GetMessagesAsync(int maxReads, int visibilityTimeoutSeconds)
        {
            return _queue.GetMessagesAsync(maxReads, TimeSpan.FromSeconds(visibilityTimeoutSeconds), new QueueRequestOptions(), new OperationContext());
        }
    }
}