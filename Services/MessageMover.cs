using AzureQueuedMessageMover.Interfaces;
using Microsoft.Extensions.Logging;

namespace AzureQueuedMessageMover.Services
{
    public class MessageMover : MessageMoverBase, IMessageMover
    {
        public MessageMover(ISourceQueue sourceQueue, ITargetQueue targetQueue, ILogger<MessageMoverBase> logger) 
            : base(sourceQueue, targetQueue, logger)
        {
        }
    }
}