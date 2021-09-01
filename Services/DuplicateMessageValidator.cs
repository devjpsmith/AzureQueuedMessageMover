using AzureQueuedMessageMover.Interfaces;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Services
{
    public class DuplicateMessageValidator : IMessageValidator
    {
        public bool IsValid(CloudQueueMessage cloudQueueMessage)
        {
            return true;
        }
    }
}