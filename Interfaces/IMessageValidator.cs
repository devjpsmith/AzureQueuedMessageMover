using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Interfaces
{
    public interface IMessageValidator
    {
         bool IsValid(CloudQueueMessage cloudQueueMessage);
    }
}