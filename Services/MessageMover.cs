using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using AzureQueuedMessageMover.Interfaces;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;

namespace AzureQueuedMessageMover.Services
{
    public class MessageMover
    {
        private const int MAX_MESSAGE_READS = 32;

        private readonly CloudQueue _sourceQueue;
        private readonly CloudQueue _targetQueue;
        private readonly ILogger<MessageMover> _logger;
        private readonly IMessageValidator _messageValidator;

        private Channel<CloudQueueMessage> _channel = Channel.CreateUnbounded<CloudQueueMessage>();

        public MessageMover(CloudQueue sourceQueue, CloudQueue targetQueue, ILogger<MessageMover> logger, IMessageValidator messageValidator)
        {
            _sourceQueue = sourceQueue;
            _targetQueue = targetQueue;
            _logger = logger;
            _messageValidator = messageValidator;
        }

        private async Task ConsumerChannel(ChannelReader<CloudQueueMessage> channelReader, string name)
        {
            while (await channelReader.WaitToReadAsync())
            {
                var cloudQueueMessage = await channelReader.ReadAsync();
                try
                {
                    if (_messageValidator.IsValid(cloudQueueMessage))
                    {
                        await _targetQueue.AddMessageAsync(cloudQueueMessage);
                        _logger.LogTrace($"Added message to target {cloudQueueMessage.AsString}");
                    }
                    else
                        _logger.LogTrace($"Skipping duplicate message {cloudQueueMessage.AsString}");
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

        private async Task ProducerChannel(ChannelWriter<CloudQueueMessage> channelWriter)
        {
            IEnumerable<CloudQueueMessage> cloudQueueMessages = Enumerable.Empty<CloudQueueMessage>();
            do
            {
                cloudQueueMessages = await _sourceQueue.GetMessagesAsync(MAX_MESSAGE_READS);
                _logger.LogInformation($"Read {cloudQueueMessages.Count()} messages");
                foreach (var cloudQueueMessage in cloudQueueMessages)
                {
                    await _channel.Writer.WriteAsync(cloudQueueMessage);
                }
            } while (cloudQueueMessages.Any());
            _channel.Writer.Complete();
        }

        public async Task Execute()
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