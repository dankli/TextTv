using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextTv.AppContext.Infrastructure.Contracts
{
    public interface INotifierTaskHandler
    {
        Task<bool> RegisterTask(uint interval = 15);
        void UnRegisterTask();
    }
}
