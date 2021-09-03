using System.Threading.Tasks;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Interfaces
{
    public interface ITargetQueue
    {
        
         Task AddMessageAsync(CloudQueueMessage cloudQueueMessage);
    }
}