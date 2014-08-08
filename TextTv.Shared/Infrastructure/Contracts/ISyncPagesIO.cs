using System.Collections.Generic;
using System.Threading.Tasks;
using TextTv.Shared.Model;

namespace TextTv.Shared.Infrastructure.Contracts
{
    public interface ISyncPagesIO
    {
        Task Initialize();
        Task Clear();
        Task WriteMonitorPagesToLocalStorage(List<MonitorPage> pages);
        Task<List<MonitorPage>> ReadMonitorPagesFromLocalStorage();
    }
}
