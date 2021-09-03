using System;
using AzureQueuedMessageMover.Decorators;
using AzureQueuedMessageMover.Interfaces;
using AzureQueuedMessageMover.Queues;
using AzureQueuedMessageMover.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AzureQueuedMessageMover
{
    public static class Startup
    {
        private static IConfigurationRoot Configuration;
        public static IServiceProvider ConfigureServices()
        {
            Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            RegisterServices(serviceCollection);

            var provider = serviceCollection.BuildServiceProvider();

            var storageConnectionString = Configuration.GetConnectionString("AzureStorage");
            var sourceQueueName = Configuration.GetValue<string>("AzureQueues:Source");
            var targetQueueName = Configuration.GetValue<string>("AzureQueues:Target");

            var sourceQueue = new SourceQueue(QueueFactory.GetQueue(storageConnectionString, sourceQueueName));
            var targetQueue = new TargetQueue(QueueFactory.GetQueue(storageConnectionString, targetQueueName));

            var validator = new DuplicateMessageValidator();
            var mover = new MessageMover(sourceQueue, targetQueue, provider.GetService<ILogger<MessageMover>>());
            serviceCollection.AddSingleton<IMessageMover>(new MessageMoverWithDuplicateRemoval(mover));
            return serviceCollection.BuildServiceProvider();
        }

        public static void RegisterServices(IServiceCollection services)
        {
            services.AddLogging(config => config.AddSerilog())
                .AddTransient<IMessageValidator, DuplicateMessageValidator>();
        }


    }
}