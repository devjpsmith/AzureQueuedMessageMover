using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using AzureQueuedMessageMover.DTO;
using AzureQueuedMessageMover.Interfaces;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;

namespace AzureQueuedMessageMover.Services
{
    public abstract class MessageMoverBase : IMessageMover
    {
        private const int MAX_MESSAGE_READS = 32;
        private const int READ_VISIBILITY_TIMEOUT = 60;

        private readonly ISourceQueue _sourceQueue;
        private readonly ITargetQueue _targetQueue;
        private readonly ILogger<MessageMoverBase> _logger;

        public IEnumerable<IMessageValidator> _messageValidators = Enumerable.Empty<IMessageValidator>();

        private Channel<RetrievedMessage> _channel = Channel.CreateUnbounded<RetrievedMessage>();

        protected MessageMoverBase(ISourceQueue sourceQueue, ITargetQueue targetQueue, ILogger<MessageMoverBase> logger)
        {
            _sourceQueue = sourceQueue;
            _targetQueue = targetQueue;
            _logger = logger;
        }

        private bool IsValid(CloudQueueMessage cloudQueueMessage)
        {
            return _messageValidators.All(v => v.IsValid(cloudQueueMessage));
        }

        private bool IsExpired(RetrievedMessage retrievedMessage)
        {
            var expirationTime = DateTime.Now.AddSeconds(READ_VISIBILITY_TIMEOUT);
            return retrievedMessage.ReadTime >= expirationTime;
        }

        private async Task ConsumerChannel(ChannelReader<RetrievedMessage> channelReader, string name)
        {
            while (await channelReader.WaitToReadAsync())
            {
                var retrievedMessage = await channelReader.ReadAsync();
                if (IsExpired(retrievedMessage))
                {
                    _logger.LogWarning("Discarding expired message in consumer channel: message will be requeued");
                    continue;
                }
                var cloudQueueMessage = retrievedMessage.CloudQueueMessage;
                try
                {
                    if (IsValid(cloudQueueMessage))
                    {
                        await _targetQueue.AddMessageAsync(cloudQueueMessage);
                        _logger.LogTrace($"Added message to target {cloudQueueMessage.AsString}");
                    }
                    else
                        _logger.LogTrace($"Skipping invalid message {cloudQueueMessage.AsString}");
                    await _sourceQueue.DeleteMessageAsync(cloudQueueMessage);

                    if (name.Equals("Consumer 8"))
                        _logger.LogInformation("Consumer Cycle iteration complete");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    _logger.LogError(cloudQueueMessage.AsString);
                }
            }
        }

        private async Task ProducerChannel(ChannelWriter<RetrievedMessage> channelWriter)
        {
            IEnumerable<CloudQueueMessage> cloudQueueMessages = Enumerable.Empty<CloudQueueMessage>();
            do
            {
                var now = DateTime.Now;
                cloudQueueMessages = await _sourceQueue.GetMessagesAsync(MAX_MESSAGE_READS, READ_VISIBILITY_TIMEOUT);
                _logger.LogInformation($"Read {cloudQueueMessages.Count()} messages");
                foreach (var cloudQueueMessage in cloudQueueMessages)
                {
                    await _channel.Writer.WriteAsync(new RetrievedMessage { CloudQueueMessage = cloudQueueMessage, ReadTime = now });
                }
            } while (cloudQueueMessages.Any());
            _channel.Writer.Complete();
        }

        public virtual async Task ExecuteMove()
        {
                var channelReader = _channel.Reader;
                var consumerTask1 = ConsumerChannel(channelReader, $"Consumer 1");
                var consumerTask2 = ConsumerChannel(channelReader, $"Consumer 2");
                var consumerTask3 = ConsumerChannel(channelReader, $"Consumer 3");
                var consumerTask4 = ConsumerChannel(channelReader, $"Consumer 4");
                var consumerTask5 = ConsumerChannel(channelReader, $"Consumer 5");
                var consumerTask6 = ConsumerChannel(channelReader, $"Consumer 6");
                var consumerTask7 = ConsumerChannel(channelReader, $"Consumer 7");
                var consumerTask8 = ConsumerChannel(channelReader, $"Consumer 8");

                var producerTask = ProducerChannel(_channel.Writer);

                await Task.WhenAll(
                    consumerTask1,
                    consumerTask2,
                    consumerTask3,
                    consumerTask4,
                    consumerTask5,
                    consumerTask6,
                    consumerTask7,
                    consumerTask8,
                    producerTask);
        }
    }
}