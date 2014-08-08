using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TextTv.AppContext.Infrastructure.Contracts;
using TextTv.AppContext.Model;

namespace TextTv.AppContext.Infrastructure
{
    public class SyncPages
    {
        private readonly ApiCaller apiCaller;
        private readonly ISyncPagesIO _syncPagesIo;

        public SyncPages(ApiCaller caller, ISyncPagesIO syncPagesIo)
        {
            this.apiCaller = caller;
            _syncPagesIo = syncPagesIo;
        }

        public async Task<SyncPages> Initialize()
        {
            await _syncPagesIo.Initialize();

            if (Debugger.IsAttached)
            {
                await _syncPagesIo.Clear();
            }

            return this;
        }

        public async Task<bool> ExistsInSync(int page)
        {
            var pages = await _syncPagesIo.ReadMonitorPagesFromLocalStorage().ConfigureAwait(false);
            if (!pages.Any())
            {
                return false;
            }
            return pages.FirstOrDefault(mp => mp.Page == page) != null;
        }

        public async Task<bool> Any()
        {
            var pages = await _syncPagesIo.ReadMonitorPagesFromLocalStorage().ConfigureAwait(false);
            return pages.Any();
        }

        public async Task Add(int page, MonitorTimespan monitorTimespan, Action onAddComplete = null)
        {
            await this.AddToLocalStore(page, monitorTimespan).ConfigureAwait(false);

            if (onAddComplete != null)
            {
                onAddComplete();
            }
        }

        public async Task Remove(int page, Action onRemoveComplete = null)
        {
            await this.RemoveFromLocalStore(page);

            if (onRemoveComplete != null)
            {
                onRemoveComplete();
            }
        }

        private async Task AddToLocalStore(int page, MonitorTimespan monitorTimespan)
        {
            var content = await this.apiCaller.GetSvtTextForWeb(page);

            List<MonitorPage> monitorPages = await _syncPagesIo.ReadMonitorPagesFromLocalStorage().ConfigureAwait(false);
            if (monitorPages.Any(mp => mp.Page == page))
            {
                MonitorPage monitorPage = monitorPages.First(mp => mp.Page == page);
                monitorPage.Date = content.Date;
                monitorPage.TimeToMonitor = monitorTimespan.Timespan;
                monitorPage.LastModified = content.LastModified;
                monitorPage.ETag = content.ETag;
            }
            else
            {
                MonitorPage monitorPage = new MonitorPage
                {
                    Page = page,
                    TimeToMonitor = monitorTimespan.Timespan,
                    LastModified = content.LastModified,
                    Date = content.Date,
                    ETag = content.ETag
                };
                

                monitorPages.Add(monitorPage);
            }

            await _syncPagesIo.WriteMonitorPagesToLocalStorage(monitorPages).ConfigureAwait(false);

        }

        public async Task Clear()
        {
            await _syncPagesIo.Clear();
        }

        private async Task RemoveFromLocalStore(int page)
        {
            List<MonitorPage> monitorPages = await _syncPagesIo.ReadMonitorPagesFromLocalStorage().ConfigureAwait(false);

            MonitorPage monitorPage = monitorPages.FirstOrDefault(mp => mp.Page == page);
            if (monitorPage != null)
            {
                monitorPages.Remove(monitorPage);
            }

            await _syncPagesIo.WriteMonitorPagesToLocalStorage(monitorPages).ConfigureAwait(false);
        }
    }
}
