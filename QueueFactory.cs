using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover
{
    public static class QueueFactory
    {
        public static CloudQueue GetQueue(string storage, string queueName)
        {
            var storageAccount = CloudStorageAccount.Parse(storage);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();
            return queue;
        }
    }
}