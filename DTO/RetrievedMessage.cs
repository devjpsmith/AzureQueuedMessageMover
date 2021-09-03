using System;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.DTO
{
    public class RetrievedMessage
    {
        public CloudQueueMessage CloudQueueMessage {get;set;}
        public DateTime ReadTime {get;set;}
    }
}