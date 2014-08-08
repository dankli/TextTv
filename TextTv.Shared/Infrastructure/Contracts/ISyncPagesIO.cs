using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextTv.AppContext.Model;

namespace TextTv.AppContext.Infrastructure.Contracts
{
    public interface ISyncPagesIO
    {
        Task Initialize();
        Task Clear();
        Task WriteMonitorPagesToLocalStorage(List<MonitorPage> pages);
        Task<List<MonitorPage>> ReadMonitorPagesFromLocalStorage();
    }
}
