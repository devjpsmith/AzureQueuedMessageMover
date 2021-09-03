using System.Threading.Tasks;

namespace AzureQueuedMessageMover.Interfaces
{
    public interface IMessageMover
    {
         Task ExecuteMove();
    }
}