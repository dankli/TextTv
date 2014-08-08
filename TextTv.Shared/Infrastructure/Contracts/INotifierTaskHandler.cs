using System.Threading.Tasks;

namespace TextTv.Shared.Infrastructure.Contracts
{
    public interface INotifierTaskHandler
    {
        Task<bool> RegisterTask(uint interval = 15);
        void UnRegisterTask();
    }
}
