using System;
using System.Threading.Tasks;
using AzureQueuedMessageMover.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static AzureQueuedMessageMover.Startup;

namespace AzureQueuedMessageMover
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            var task = Task.Run(() => MainAsync(serviceProvider));
            task.Wait();
        }

        private static async Task MainAsync(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger.LogInformation("Starting Execution");
            var mover = serviceProvider.GetService<MessageMover>();
            await mover.Execute();
        }
    }
}
