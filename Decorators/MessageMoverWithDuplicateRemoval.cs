using System.Collections.Generic;
using System.Threading.Tasks;
using AzureQueuedMessageMover.Interfaces;
using AzureQueuedMessageMover.Services;
using Microsoft.Azure.Storage.Queue;

namespace AzureQueuedMessageMover.Decorators
{
    public class MessageMoverWithDuplicateRemoval : IMessageMover
    {
        private readonly MessageMoverBase _messageMoverBase;
        public MessageMoverWithDuplicateRemoval(MessageMoverBase messageMoverBase)
        {
            _messageMoverBase = messageMoverBase;
            var validators = new List<IMessageValidator>();
            validators.AddRange(_messageMoverBase._messageValidators);
            validators.Add(new DuplicateMessageValidator());
        }

        public Task ExecuteMove()
        {
            return _messageMoverBase.ExecuteMove();
        }
    }
}