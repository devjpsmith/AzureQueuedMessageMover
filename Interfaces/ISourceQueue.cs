using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Interfaces
{
    public interface ISourceQueue
    {
         Task DeleteMessageAsync(CloudQueueMessage cloudQueueMessage);
         Task<IEnumerable<CloudQueueMessage>> GetMessagesAsync(int maxReads, int visibilityTimeoutSeconds);
    }
}