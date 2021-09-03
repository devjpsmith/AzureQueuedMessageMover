using System.Collections.Concurrent;
using AzureQueuedMessageMover.Interfaces;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Validators
{
    public class DuplicateMessageValidator : IMessageValidator
    {
        private ConcurrentDictionary<int, byte> _map = new ConcurrentDictionary<int, byte>();

        public bool IsValid(CloudQueueMessage cloudQueueMessage)
        {
            if (_map.ContainsKey(cloudQueueMessage.GetHashCode()))
                return false;
            return _map.TryAdd(cloudQueueMessage.GetHashCode(), (byte)1);
        }
    }
}