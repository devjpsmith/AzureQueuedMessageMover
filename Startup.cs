using System;
using AzureQueuedMessageMover.Interfaces;
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

            var sourceQueue = QueueFactory.GetQueue(storageConnectionString, sourceQueueName);
            var targetQueue = QueueFactory.GetQueue(storageConnectionString, targetQueueName);

            var validator = new DuplicateMessageValidator();
            var mover = new MessageMover(sourceQueue, targetQueue, provider.GetService<ILogger<MessageMover>>(), provider.GetService<IMessageValidator>());
            serviceCollection.AddSingleton<MessageMover>(mover);
            return serviceCollection.BuildServiceProvider();
        }

        public static void RegisterServices(IServiceCollection services)
        {
            services.AddLogging(config => config.AddSerilog())
                .AddTransient<IMessageValidator, DuplicateMessageValidator>();
        }


    }
}